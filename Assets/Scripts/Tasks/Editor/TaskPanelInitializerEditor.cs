#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChemSimDiploma.Tasks.Editor
{
[CustomEditor(typeof(UI.TaskPanelInitializer))]
public class TaskPanelInitializerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(6f);

        if (GUILayout.Button("Синхронизировать UITaskBar сейчас"))
            foreach (UI.TaskPanelInitializer panel in targets)
                panel.SyncPanel();
    }
}
}
#endif