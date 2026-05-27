#if UNITY_EDITOR
using ChemSimDiploma.Tasks.Data;
using UnityEditor;

namespace ChemSimDiploma.Tasks.Editor
{
public class LevelTaskSetAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (!path.EndsWith(".asset")) continue;

            LevelTaskSetSO taskSet = AssetDatabase.LoadAssetAtPath<LevelTaskSetSO>(path);
            if (taskSet != null)
                TaskPanelSyncEditorUtility.SyncAllPanelsForTaskSet(taskSet);
        }
    }
}
}
#endif
