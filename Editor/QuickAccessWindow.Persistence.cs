using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFavoriteTool
{
    public sealed partial class QuickAccessWindow
    {
        private const string k_PrefsKeyFavorites = "QuickAccess_Favorites";
        private const string k_PrefsKeyRecent = "QuickAccess_Recent";
        private static bool s_Loaded;

        private static string ProjectPrefsKey(string key) =>
            $"{key}_{Hash128.Compute(Application.dataPath.Replace('\\', '/'))}";

        internal static void EnsureLoaded()
        {
            if (s_Loaded) return;

            // 启动或导入期间资源数据库可能尚未就绪，不能据此删除持久化记录。
            s_Favorites = LoadList(k_PrefsKeyFavorites);
            s_Recent = LoadList(k_PrefsKeyRecent);
            s_Loaded = true;
        }

        private static List<string> LoadList(string legacyKey)
        {
            string projectKey = ProjectPrefsKey(legacyKey);
            if (!EditorPrefs.HasKey(projectKey))
            {
                // 每个项目只迁移一次；保留旧键，避免破坏其他项目尚未迁移的数据。
                EditorPrefs.SetString(projectKey, EditorPrefs.GetString(legacyKey, ""));
            }

            string data = EditorPrefs.GetString(projectKey, "");
            return data.Split('|').Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
        }

        private static void SaveFavorites() =>
            EditorPrefs.SetString(ProjectPrefsKey(k_PrefsKeyFavorites), string.Join("|", s_Favorites));

        private static void SaveRecent() =>
            EditorPrefs.SetString(ProjectPrefsKey(k_PrefsKeyRecent), string.Join("|", s_Recent));

        private static void ToggleFavorite(string assetPath)
        {
            EnsureLoaded();
            if (s_Favorites.Contains(assetPath))
                s_Favorites.Remove(assetPath);
            else
                s_Favorites.Insert(0, assetPath);

            SaveFavorites();
        }

        internal static void RemoveDeletedAssets(string[] deletedAssets)
        {
            if (deletedAssets.Length == 0) return;
            EnsureLoaded();

            // 只依据明确的删除事件清理，包含已删除文件夹下的资源。
            bool IsDeleted(string path) => deletedAssets.Any(deleted =>
                path == deleted || path.StartsWith(deleted + "/", StringComparison.Ordinal));

            bool favoritesChanged = s_Favorites.RemoveAll(IsDeleted) > 0;
            bool recentChanged = s_Recent.RemoveAll(IsDeleted) > 0;
            if (favoritesChanged) SaveFavorites();
            if (recentChanged) SaveRecent();
            if (favoritesChanged || recentChanged) RepaintOpenWindows();
        }
    }
}
