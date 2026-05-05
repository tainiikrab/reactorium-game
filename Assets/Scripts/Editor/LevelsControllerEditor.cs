using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelsController))]
public sealed class LevelsControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        var levelsProp = serializedObject.FindProperty("_levels");
        if (levelsProp == null || !levelsProp.isArray)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        for (var i = 0; i < levelsProp.arraySize; i++)
        {
            var levelProp = levelsProp.GetArrayElementAtIndex(i);
            var sceneRefProp = levelProp.FindPropertyRelative(nameof(Level.Scene));
            if (sceneRefProp == null)
                continue;

            var sceneNameProp = sceneRefProp.FindPropertyRelative("sceneName");
            var sceneName = sceneNameProp?.stringValue;
            if (string.IsNullOrEmpty(sceneName))
            {
                EditorGUILayout.HelpBox($"Уровень [{i}]: сцена не назначена.", MessageType.Warning);
                continue;
            }

            if (!IsSceneInEnabledBuildSettings(sceneName))
            {
                EditorGUILayout.HelpBox(
                    $"Уровень [{i}] («{sceneName}»): сцена не добавлена в Build Settings или выключена.",
                    MessageType.Error);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static bool IsSceneInEnabledBuildSettings(string sceneNameWithoutExtension)
    {
        foreach (var entry in EditorBuildSettings.scenes)
        {
            if (!entry.enabled)
                continue;
            if (Path.GetFileNameWithoutExtension(entry.path) == sceneNameWithoutExtension)
                return true;
        }

        return false;
    }
}
