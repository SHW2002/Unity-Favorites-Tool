using UnityEditor;

namespace UnityFavoriteTool
{
    /// <summary>
    /// 监听资源保存事件，自动记录最近编辑过的资源。
    /// </summary>
    internal sealed class QuickAccessAssetProcessor : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
                QuickAccessWindow.RecordAsset(path);
            return paths;
        }
    }

    /// <summary>
    /// 监听资源导入事件并补充最近访问记录。
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

            foreach (string path in importedAssets)
                QuickAccessWindow.RecordAsset(path);
        }
    }
}
