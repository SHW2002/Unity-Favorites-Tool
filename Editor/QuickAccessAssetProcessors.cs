using System.Linq;
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
            QuickAccessWindow.RecordAssets(paths);
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
            // Do not resolve or prune stored references while an import is running.
            string[] changedPaths = importedAssets.Concat(movedAssets).ToArray();
            bool hadDeletions = deletedAssets.Length > 0;
            void ProcessWhenReady()
            {
                if (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    EditorApplication.delayCall += ProcessWhenReady;
                    return;
                }
                QuickAccessWindow.RecordAssets(changedPaths);
                if (hadDeletions)
                    QuickAccessWindow.RefreshAssetReferences();
            }
            EditorApplication.delayCall += ProcessWhenReady;
        }
    }
}
