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
    public sealed partial class QuickAccessWindow : EditorWindow
    {
        private const int k_MaxRecentCount = QuickAccessSettings.MaxRecentCount;

        // Resource identity is a GUID; paths are resolved only when needed.
        private static List<string> s_Favorites = new();
        private static List<string> s_Recent = new();
        private static bool s_Loaded;

        [MenuItem("Assets/Add to QuickAccess Favorites", false, 20)]
        private static void AddSelectionToFavorites()
        {
            EnsureLoaded();
            bool changed = false;
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && !s_Favorites.Contains(guid))
                {
                    s_Favorites.Insert(0, guid);
                    changed = true;
                }
            }
            if (changed)
            {
                SaveAll();
                RepaintOpenWindows();
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
                if (!string.IsNullOrEmpty(path) && !s_Favorites.Contains(guid))
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
                if (!string.IsNullOrEmpty(path) && s_Favorites.Remove(guid))
                    changed = true;
            }
            if (changed)
            {
                SaveAll();
                RepaintOpenWindows();
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
                if (!string.IsNullOrEmpty(path) && s_Favorites.Contains(guid))
                    return true;
            }
            return false;
        }

        [MenuItem("Tools/QuickAccess %#q")]
        private static void Open()
        {
            var win = GetWindow<QuickAccessWindow>();
            win.ApplyCurrentTheme();
            win.Show();
        }

        #region 数据持久化

        internal static void EnsureLoaded()
        {
            if (s_Loaded) return;
            var settings = QuickAccessSettings.instance;
            settings.EnsureInitialized();
            s_Favorites = settings.Favorites.ToList();
            s_Recent = settings.Recent.ToList();
            s_Loaded = true;
        }

        private static void SaveAll()
        {
            QuickAccessSettings.instance.SaveLists(s_Favorites, s_Recent);
        }

        private static void ToggleFavorite(string assetGuid)
        {
            if (s_Favorites.Contains(assetGuid))
                s_Favorites.Remove(assetGuid);
            else
                s_Favorites.Insert(0, assetGuid);

            SaveAll();
            RepaintOpenWindows();
        }

        // A null destination means the end. Resolve GUIDs at commit time so a
        // moved/renamed resource still identifies the same favorite.
        private static void MoveFavorite(string assetGuid, string beforeGuid)
        {
            int source = s_Favorites.IndexOf(assetGuid);
            int destination = beforeGuid == null ? s_Favorites.Count : s_Favorites.IndexOf(beforeGuid);
            if (source < 0 || destination < 0 || source == destination) return;
            if (source < destination) destination--;
            if (source == destination) return;

            s_Favorites.RemoveAt(source);
            s_Favorites.Insert(destination, assetGuid);
            SaveAll();
            RepaintOpenWindows();
        }

        #endregion

        #region 最近访问记录

        internal static void RecordAsset(string assetPath) => RecordAssets(new[] { assetPath });

        internal static void RecordAssets(IEnumerable<string> assetPaths)
        {
            EnsureLoaded();
            bool changed = false;
            foreach (string assetPath in assetPaths)
            {
                if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets/") || assetPath.EndsWith(".meta"))
                    continue;
                string guid = AssetDatabase.AssetPathToGUID(assetPath, AssetPathToGUIDOptions.OnlyExistingAssets);
                if (string.IsNullOrEmpty(guid) || (s_Recent.Count > 0 && s_Recent[0] == guid)) continue;

                s_Recent.Remove(guid);
                s_Recent.Insert(0, guid);
                changed = true;
            }

            if (s_Recent.Count > k_MaxRecentCount)
                s_Recent.RemoveRange(k_MaxRecentCount, s_Recent.Count - k_MaxRecentCount);

            if (changed) SaveAll();

            // 刷新已打开的窗口
            RepaintOpenWindows();
        }

        internal static void RefreshAssetReferences()
        {
            EnsureLoaded();
            // Run after asset processing, not while the database is loading.
            bool changed = s_Favorites.RemoveAll(IsMissingAsset) > 0;
            changed |= s_Recent.RemoveAll(IsMissingAsset) > 0;
            if (changed) SaveAll();
            RepaintOpenWindows();
        }

        private static bool IsMissingAsset(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ||
                AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets) != guid;
        }

        private static void RepaintOpenWindows()
        {
            // GetWindow focuses an existing EditorWindow. Finding the open instances
            // directly lets background asset imports repaint without stealing focus.
            foreach (var window in Resources.FindObjectsOfTypeAll<QuickAccessWindow>())
                window.Repaint();
        }

        #endregion
    }
}
