using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFavoriteTool
{
    [FilePath("UserSettings/QuickAccessSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class QuickAccessSettings : ScriptableSingleton<QuickAccessSettings>
    {
        internal const int MaxRecentCount = 50;
        private const string LegacyFavoritesKey = "QuickAccess_Favorites";
        private const string LegacyRecentKey = "QuickAccess_Recent";

        [SerializeField] private bool m_Initialized;
        [SerializeField] private List<string> m_Favorites = new();
        [SerializeField] private List<string> m_Recent = new();

        internal IReadOnlyList<string> Favorites => m_Favorites;
        internal IReadOnlyList<string> Recent => m_Recent;

        internal void EnsureInitialized()
        {
            if (m_Initialized) return;
            ImportLegacyPaths(EditorPrefs.GetString(LegacyFavoritesKey, ""),
                EditorPrefs.GetString(LegacyRecentKey, ""));
        }

        private void ImportLegacyPaths(string favorites, string recent)
        {
            if (m_Initialized) return;
            m_Favorites = ConvertLegacyPaths(favorites);
            m_Recent = ConvertLegacyPaths(recent).Take(MaxRecentCount).ToList();
            m_Initialized = true;
            try
            {
                Save(true);
            }
            catch
            {
                m_Initialized = false;
                throw;
            }
            // These keys were shared by every project. Leave them untouched so
            // another project can still migrate its own resolvable paths.
        }

        private static List<string> ConvertLegacyPaths(string value) => value.Split('|')
            .Where(path => !string.IsNullOrEmpty(path))
            .Select(path => AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets))
            .Where(guid => !string.IsNullOrEmpty(guid))
            .Distinct().ToList();

        internal void SaveLists(IEnumerable<string> favorites, IEnumerable<string> recent)
        {
            m_Favorites = favorites.ToList();
            m_Recent = recent.Take(MaxRecentCount).ToList();
            Save(true);
        }
    }
}
