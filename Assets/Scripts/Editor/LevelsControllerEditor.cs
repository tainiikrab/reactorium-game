using System.IO;
using ChemSimDiploma.Levels;
using UnityEditor;
using UnityEngine;

namespace ChemSimDiploma.Editor
{
    [CustomEditor(typeof(LevelsController))]
    public sealed class LevelsControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            SerializedProperty levelsProp = serializedObject.FindProperty("_levels");
            if (levelsProp == null || !levelsProp.isArray)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            for (int i = 0; i < levelsProp.arraySize; i++)
            {
                SerializedProperty levelProp = levelsProp.GetArrayElementAtIndex(i);
                SerializedProperty sceneRefProp = levelProp.FindPropertyRelative(nameof(Level.Scene));
                if (sceneRefProp == null)
                    continue;

                SerializedProperty sceneNameProp = sceneRefProp.FindPropertyRelative("sceneName");
                string sceneName = sceneNameProp?.stringValue;
                if (string.IsNullOrEmpty(sceneName))
                {
                    EditorGUILayout.HelpBox($"Уровень [{i}]: сцена не назначена.", MessageType.Warning);
                    continue;
                }

                if (!IsSceneInEnabledBuildSettings(sceneName))
                    EditorGUILayout.HelpBox(
                        $"Уровень [{i}] («{sceneName}»): сцена не добавлена в Build Settings или выключена.",
                        MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsSceneInEnabledBuildSettings(string sceneNameWithoutExtension)
        {
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            {
                if (!entry.enabled)
                    continue;
                if (Path.GetFileNameWithoutExtension(entry.path) == sceneNameWithoutExtension)
                    return true;
            }

            return false;
        }
    }
}