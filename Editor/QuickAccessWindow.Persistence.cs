using System.Linq;
using UnityEditor;

namespace UnityFavoriteTool
{
    public sealed partial class QuickAccessWindow
    {
        private static bool s_Loaded;
        #region 数据持久化

        internal static void EnsureLoaded()
        {
            if (s_Loaded) return;
            var settings = QuickAccessSettings.instance;
            // Existing GUID lists are safe to load without resolving any assets.
            // First-time path migration must wait until the database is ready.
            if (!settings.IsInitialized && (EditorApplication.isUpdating || EditorApplication.isCompiling))
                return;
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
            EnsureLoaded();
            if (!s_Loaded) return;
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

    }
}
