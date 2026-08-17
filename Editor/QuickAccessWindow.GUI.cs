using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityFavoriteTool
{
    public sealed partial class QuickAccessWindow
    {
        private const float k_RowHeight = 20f;
        private const float k_ToggleWidth = 20f;
        private const float k_IconSize = 16f;
        private const float k_SplitterHeight = 6f;

        private Vector2 m_FavScrollPos;
        private Vector2 m_RecentScrollPos;
        private float m_SplitRatio = 0.35f;
        private bool m_Dragging;
        private QuickAccessTheme m_Theme;
        private GUIStyle m_HeaderStyle;
        private GUIStyle m_AssetLabelStyle;

        private void OnEnable()
        {
            s_Instance = this;
            EnsureLoaded();
            ApplyCurrentTheme();
        }

        private void OnDisable()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

        private void OnGUI()
        {
            EnsureCurrentThemeAndStyles();

            float favoriteHeight = position.height * m_SplitRatio;
            float recentHeight = position.height - favoriteHeight - k_SplitterHeight;

            DrawSection("Favorites", ref m_FavScrollPos, s_Favorites, favoriteHeight, true);

            var splitterRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(k_SplitterHeight));
            EditorGUI.DrawRect(splitterRect, m_Theme.Splitter);
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);
            HandleSplitterDrag(splitterRect);

            DrawSection("Recent", ref m_RecentScrollPos, s_Recent, recentHeight, false);
        }

        private void ApplyCurrentTheme()
        {
            m_Theme = QuickAccessTheme.Current;
            titleContent = new GUIContent("QuickAccess", m_Theme.GetFavoriteIcon(true).image);
            m_HeaderStyle = null;
            m_AssetLabelStyle = null;
        }

        private void EnsureCurrentThemeAndStyles()
        {
            if (!ReferenceEquals(m_Theme, QuickAccessTheme.Current))
                ApplyCurrentTheme();

            m_HeaderStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
            m_AssetLabelStyle ??= new GUIStyle(EditorStyles.label)
            {
                richText = true,
                fontSize = 11,
                padding = new RectOffset(0, 0, 0, 0)
            };
        }

        private void DrawSection(
            string title,
            ref Vector2 scrollPosition,
            List<string> assets,
            float height,
            bool isFavoriteSection)
        {
            GUILayout.BeginVertical(GUILayout.Height(height));

            var headerRect = GUILayoutUtility.GetRect(0, 22f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, m_Theme.HeaderBackground);
            var headerLabelRect = new Rect(
                headerRect.x + 4f,
                headerRect.y + 1f,
                headerRect.width - 8f,
                headerRect.height);
            GUI.Label(headerLabelRect, title, m_HeaderStyle);

            if (assets.Count == 0)
            {
                GUILayout.FlexibleSpace();
                string emptyText = isFavoriteSection ? "No favorites" : "No recent access";
                EditorGUILayout.LabelField(emptyText, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                DrawAssetList(assets);
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        private void DrawAssetList(List<string> assets)
        {
            foreach (string assetPath in assets.ToArray())
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset == null) continue;

                var rowRect = GUILayoutUtility.GetRect(0, k_RowHeight, GUILayout.ExpandWidth(true));
                if (rowRect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rowRect, m_Theme.Hover);
                if (Selection.activeObject == asset)
                    EditorGUI.DrawRect(rowRect, m_Theme.Selection);

                var icon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D
                           ?? EditorGUIUtility.ObjectContent(asset, asset.GetType()).image;
                var iconRect = new Rect(
                    rowRect.x + 4f,
                    rowRect.y + (k_RowHeight - k_IconSize) * 0.5f,
                    k_IconSize,
                    k_IconSize);
                if (icon != null)
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                float labelX = iconRect.xMax + 4f;
                float toggleSpace = k_ToggleWidth + 8f;
                float labelWidth = rowRect.xMax - labelX - toggleSpace;
                string fileName = Path.GetFileName(assetPath);
                string directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                string displayText = string.IsNullOrEmpty(directory)
                    ? fileName
                    : $"{fileName}  <color={m_Theme.PathColor}>{directory}</color>";
                GUI.Label(new Rect(labelX, rowRect.y, labelWidth, k_RowHeight), displayText, m_AssetLabelStyle);

                bool isFavorite = s_Favorites.Contains(assetPath);
                var toggleRect = new Rect(
                    rowRect.xMax - k_ToggleWidth - 4f,
                    rowRect.y + (k_RowHeight - k_IconSize) * 0.5f,
                    k_IconSize,
                    k_IconSize);
                var previousColor = GUI.color;
                GUI.color = isFavorite ? m_Theme.FavoriteTint : m_Theme.InactiveFavoriteTint;
                if (GUI.Button(toggleRect, m_Theme.GetFavoriteIcon(isFavorite), GUIStyle.none))
                    ToggleFavorite(assetPath);
                GUI.color = previousColor;

                var clickRect = new Rect(rowRect.x, rowRect.y, rowRect.width - toggleSpace, rowRect.height);
                if (Event.current.type != EventType.MouseDown || !clickRect.Contains(Event.current.mousePosition))
                    continue;

                if (Event.current.clickCount == 2)
                    AssetDatabase.OpenAsset(asset);
                else
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
                Event.current.Use();
            }
        }

        private void HandleSplitterDrag(Rect splitterRect)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && splitterRect.Contains(currentEvent.mousePosition))
            {
                m_Dragging = true;
                currentEvent.Use();
            }
            if (m_Dragging && currentEvent.type == EventType.MouseDrag)
            {
                m_SplitRatio = Mathf.Clamp(currentEvent.mousePosition.y / position.height, 0.1f, 0.8f);
                Repaint();
                currentEvent.Use();
            }
            if (currentEvent.type == EventType.MouseUp)
                m_Dragging = false;
        }
    }
}
