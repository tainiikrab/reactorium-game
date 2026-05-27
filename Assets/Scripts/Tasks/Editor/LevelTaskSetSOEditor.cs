#if UNITY_EDITOR
using ChemSimDiploma.Tasks.Data;
using UnityEditor;
using UnityEngine;

namespace ChemSimDiploma.Tasks.Editor
{
[CustomEditor(typeof(LevelTaskSetSO))]
public class LevelTaskSetSOEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty tasks = serializedObject.FindProperty("_tasks");
        SerializedProperty delay = serializedObject.FindProperty("_finishPopupDelaySeconds");

        EditorGUILayout.LabelField("Задания уровня", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Порядок элементов = порядок UITaskBar в TaskHolder. При изменении списка панель в открытой сцене обновится автоматически.",
            MessageType.Info);

        if (tasks != null)
            EditorGUILayout.PropertyField(tasks, new GUIContent("Tasks"), true);

        EditorGUILayout.Space(8f);
        EditorGUILayout.PropertyField(delay, new GUIContent("Задержка финишного попапа (сек)"));

        if (tasks != null && tasks.isArray)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField($"Всего заданий: {tasks.arraySize}", EditorStyles.miniLabel);
        }

        if (serializedObject.ApplyModifiedProperties())
            TaskPanelSyncEditorUtility.SyncAllPanelsForTaskSet((LevelTaskSetSO)target);
    }
}
}
#endif
