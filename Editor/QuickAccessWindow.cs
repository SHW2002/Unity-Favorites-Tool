using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFavoriteTool
{
    /// <summary>
    /// QuickAccess 编辑器窗口：上部显示收藏夹，下部显示最近编辑过的资源。
    /// 每项右侧有收藏复选框，勾选后同步到收藏夹。
    /// </summary>
    public sealed class QuickAccessWindow : EditorWindow
    {
        private const string k_PrefsKeyFavorites = "QuickAccess_Favorites";
        private const string k_PrefsKeyRecent = "QuickAccess_Recent";
        private const int k_MaxRecentCount = 50;
        private const float k_RowHeight = 20f;
        private const float k_ToggleWidth = 20f;
        private const float k_IconSize = 16f;

        private static List<string> s_Favorites = new();
        private static List<string> s_Recent = new();
        private static bool s_Loaded;

        private Vector2 m_FavScrollPos;
        private Vector2 m_RecentScrollPos;
        private float m_SplitRatio = 0.35f;
        private bool m_Dragging;

        [MenuItem("Assets/Add to QuickAccess Favorites", false, 20)]
        private static void AddSelectionToFavorites()
        {
            EnsureLoaded();
            bool changed = false;
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && !s_Favorites.Contains(path))
                {
                    s_Favorites.Insert(0, path);
                    changed = true;
                }
            }
            if (changed)
            {
                SaveAll();
                if (HasOpenInstances<QuickAccessWindow>())
                    GetWindow<QuickAccessWindow>().Repaint();
            }
        }

        [MenuItem("Assets/Add to QuickAccess Favorites", true)]
        private static bool AddSelectionToFavoritesValidate()
        {
            if (Selection.assetGUIDs.Length == 0) return false;
            EnsureLoaded();
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && !s_Favorites.Contains(path))
                    return true;
            }
            return false;
        }

        [MenuItem("Assets/Remove from QuickAccess Favorites", false, 21)]
        private static void RemoveSelectionFromFavorites()
        {
            EnsureLoaded();
            bool changed = false;
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && s_Favorites.Remove(path))
                    changed = true;
            }
            if (changed)
            {
                SaveAll();
                if (HasOpenInstances<QuickAccessWindow>())
                    GetWindow<QuickAccessWindow>().Repaint();
            }
        }

        [MenuItem("Assets/Remove from QuickAccess Favorites", true)]
        private static bool RemoveSelectionFromFavoritesValidate()
        {
            if (Selection.assetGUIDs.Length == 0) return false;
            EnsureLoaded();
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && s_Favorites.Contains(path))
                    return true;
            }
            return false;
        }

        [MenuItem("Tools/QuickAccess %#q")]
        private static void Open()
        {
            var win = GetWindow<QuickAccessWindow>();
            win.titleContent = new GUIContent("QuickAccess", EditorGUIUtility.IconContent("d_Favorite Icon").image);
            win.Show();
        }

        #region 数据持久化

        internal static void EnsureLoaded()
        {
            if (s_Loaded) return;
            s_Loaded = true;

            s_Favorites = DeserializeList(EditorPrefs.GetString(k_PrefsKeyFavorites, ""));
            s_Recent = DeserializeList(EditorPrefs.GetString(k_PrefsKeyRecent, ""));

            // 清理已不存在的资源
            s_Favorites.RemoveAll(p => string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(p)));
            s_Recent.RemoveAll(p => string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(p)));
        }

        private static void SaveAll()
        {
            EditorPrefs.SetString(k_PrefsKeyFavorites, SerializeList(s_Favorites));
            EditorPrefs.SetString(k_PrefsKeyRecent, SerializeList(s_Recent));
        }

        private static string SerializeList(List<string> list) => string.Join("|", list);

        private static List<string> DeserializeList(string data)
        {
            if (string.IsNullOrEmpty(data)) return new List<string>();
            return data.Split('|').Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
        }

        #endregion

        #region 最近访问记录

        internal static void RecordAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || assetPath.StartsWith("Packages/")) return;
            // 忽略 .meta 和 ProjectSettings
            if (assetPath.EndsWith(".meta") || assetPath.StartsWith("ProjectSettings/")) return;

            EnsureLoaded();

            s_Recent.Remove(assetPath);
            s_Recent.Insert(0, assetPath);

            if (s_Recent.Count > k_MaxRecentCount)
                s_Recent.RemoveRange(k_MaxRecentCount, s_Recent.Count - k_MaxRecentCount);

            SaveAll();

            // 刷新已打开的窗口
            if (HasOpenInstances<QuickAccessWindow>())
                GetWindow<QuickAccessWindow>().Repaint();
        }

        #endregion

        #region GUI

        private void OnEnable()
        {
            EnsureLoaded();
        }

        private void OnGUI()
        {
            float totalHeight = position.height;
            float splitterHeight = 6f;

            float favHeight = totalHeight * m_SplitRatio;
            float recentHeight = totalHeight - favHeight - splitterHeight;

            // ── 收藏夹 ──
            DrawSection("Favorites", ref m_FavScrollPos, s_Favorites, favHeight, isFavoriteSection: true);

            // ── 分割条 ──
            var splitterRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(splitterHeight));
            EditorGUI.DrawRect(splitterRect, new Color(0.15f, 0.15f, 0.15f, 1f));
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);
            HandleSplitterDrag(splitterRect);

            // ── 最近访问 ──
            DrawSection("Recent", ref m_RecentScrollPos, s_Recent, recentHeight, isFavoriteSection: false);
        }

        private void DrawSection(string title, ref Vector2 scrollPos, List<string> list, float height, bool isFavoriteSection)
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };

            GUILayout.BeginVertical(GUILayout.Height(height));

            // 标题栏
            var headerRect = GUILayoutUtility.GetRect(0, 22f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, new Color(0.22f, 0.22f, 0.22f, 1f));
            GUI.Label(new Rect(headerRect.x + 4, headerRect.y + 1, headerRect.width, headerRect.height), title, headerStyle);

            if (list.Count == 0)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(isFavoriteSection ? "No favorites" : "No recent access", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }
            else
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                DrawAssetList(list, isFavoriteSection);
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        private void DrawAssetList(List<string> list, bool isFavoriteSection)
        {
            // 用副本遍历，避免迭代中修改集合
            var snapshot = list.ToArray();

            foreach (var assetPath in snapshot)
            {
                var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (obj == null) continue;

                var rowRect = GUILayoutUtility.GetRect(0, k_RowHeight, GUILayout.ExpandWidth(true));

                // 悬停高亮
                bool hover = rowRect.Contains(Event.current.mousePosition);
                if (hover)
                    EditorGUI.DrawRect(rowRect, new Color(0.3f, 0.3f, 0.3f, 0.4f));

                // 选中高亮
                if (Selection.activeObject == obj)
                    EditorGUI.DrawRect(rowRect, new Color(0.17f, 0.36f, 0.53f, 0.5f));

                // 图标
                var icon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D
                           ?? EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;
                var iconRect = new Rect(rowRect.x + 4, rowRect.y + (k_RowHeight - k_IconSize) * 0.5f, k_IconSize, k_IconSize);
                if (icon != null)
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                // 名称 + 路径
                float labelX = iconRect.xMax + 4;
                float toggleSpace = k_ToggleWidth + 8;
                float labelWidth = rowRect.width - labelX - toggleSpace;

                string fileName = System.IO.Path.GetFileName(assetPath);
                string dirPath = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                string displayText = string.IsNullOrEmpty(dirPath) ? fileName : $"{fileName}  <color=#888888>{dirPath}</color>";

                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    fontSize = 11,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                var labelRect = new Rect(labelX, rowRect.y, labelWidth, k_RowHeight);
                GUI.Label(labelRect, displayText, labelStyle);

                // 收藏复选框
                bool isFav = s_Favorites.Contains(assetPath);
                var toggleRect = new Rect(rowRect.xMax - k_ToggleWidth - 4, rowRect.y + (k_RowHeight - k_IconSize) * 0.5f, k_IconSize, k_IconSize);
                var starIcon = isFav
                    ? EditorGUIUtility.IconContent("d_Favorite Icon")
                    : EditorGUIUtility.IconContent("d_Favorite");

                // 用按钮模拟星标切换
                var prevColor = GUI.color;
                GUI.color = isFav ? new Color(1f, 0.85f, 0.2f) : new Color(0.6f, 0.6f, 0.6f);
                if (GUI.Button(toggleRect, starIcon, GUIStyle.none))
                {
                    ToggleFavorite(assetPath);
                }
                GUI.color = prevColor;

                // 点击行选中资源（排除星标区域）
                var clickRect = new Rect(rowRect.x, rowRect.y, rowRect.width - toggleSpace, rowRect.height);
                if (Event.current.type == EventType.MouseDown && clickRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.clickCount == 2)
                        AssetDatabase.OpenAsset(obj);
                    else
                    {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                    Event.current.Use();
                }
            }
        }

        private static void ToggleFavorite(string assetPath)
        {
            if (s_Favorites.Contains(assetPath))
                s_Favorites.Remove(assetPath);
            else
                s_Favorites.Insert(0, assetPath);

            SaveAll();
        }

        #endregion

        #region 分割条拖拽

        private void HandleSplitterDrag(Rect splitterRect)
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown && splitterRect.Contains(e.mousePosition))
            {
                m_Dragging = true;
                e.Use();
            }
            if (m_Dragging && e.type == EventType.MouseDrag)
            {
                m_SplitRatio = Mathf.Clamp(e.mousePosition.y / position.height, 0.1f, 0.8f);
                Repaint();
                e.Use();
            }
            if (e.type == EventType.MouseUp)
                m_Dragging = false;
        }

        #endregion
    }

    /// <summary>
    /// 监听资源保存事件，自动记录最近编辑过的资源。
    /// </summary>
    internal sealed class QuickAccessAssetProcessor : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
                QuickAccessWindow.RecordAsset(path);
            return paths;
        }
    }

    /// <summary>
    /// 监听资源导入/移动/删除，补充记录并清理失效路径。
    /// </summary>
    internal sealed class QuickAccessPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            QuickAccessWindow.EnsureLoaded();

            foreach (var path in importedAssets)
                QuickAccessWindow.RecordAsset(path);
        }
    }
}
