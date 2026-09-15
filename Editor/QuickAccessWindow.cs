using System.Collections.Generic;
using UnityEditor;

namespace UnityFavoriteTool
{
    /// <summary>
    /// QuickAccess 编辑器窗口：上部显示收藏夹，下部显示最近编辑过的资源。
    /// 每项右侧有收藏复选框，勾选后同步到收藏夹。
    /// </summary>
    public sealed partial class QuickAccessWindow : EditorWindow
    {
        private const int k_MaxRecentCount = 50;

        private static List<string> s_Favorites = new();
        private static List<string> s_Recent = new();
        private static QuickAccessWindow s_Instance;

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
                SaveFavorites();
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
                SaveFavorites();
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
                if (!string.IsNullOrEmpty(path) && s_Favorites.Contains(path))
                    return true;
            }
            return false;
        }

        [MenuItem("Window/QuickAccess")]
        private static void OpenFromWindowMenu()
        {
            Open();
        }

        [MenuItem("Tools/QuickAccess %#q")]
        private static void Open()
        {
            var win = GetWindow<QuickAccessWindow>();
            win.ApplyCurrentTheme();
            win.Show();
        }

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

            SaveRecent();

            // 刷新已打开的窗口
            RepaintOpenWindows();
        }

        private static void RepaintOpenWindows()
        {
            s_Instance?.Repaint();
        }

        #endregion
    }
}
