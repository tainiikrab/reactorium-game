#if UNITY_EDITOR
using ChemSimDiploma.Tasks.Data;
using ChemSimDiploma.Tasks.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChemSimDiploma.Tasks.Editor
{
public static class TaskPanelSyncEditorUtility
{
    public static void SyncAllPanelsForTaskSet(LevelTaskSetSO taskSet)
    {
        if (taskSet == null) return;

        bool anyChanged = false;

        foreach (TaskPanelInitializer panel in Object.FindObjectsByType<TaskPanelInitializer>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (!panel.UsesTaskSet(taskSet)) continue;

            panel.SyncPanel();
            anyChanged = true;
        }

        if (anyChanged)
            MarkOpenScenesDirty();
    }

    public static void SyncAllPanelsInOpenScenes()
    {
        bool anyChanged = false;

        foreach (TaskPanelInitializer panel in Object.FindObjectsByType<TaskPanelInitializer>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            panel.SyncPanel();
            anyChanged = true;
        }

        if (anyChanged)
            MarkOpenScenesDirty();
    }

    private static void MarkOpenScenesDirty()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
                EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
}
#endif