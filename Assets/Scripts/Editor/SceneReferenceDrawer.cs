using System.IO;
using ChemSimDiploma.Levels;
using UnityEditor;
using UnityEngine;

namespace ChemSimDiploma.Editor
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public sealed class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty guidProp = property.FindPropertyRelative("sceneAssetGuid");
            SerializedProperty nameProp = property.FindPropertyRelative("sceneName");

            SceneAsset scene = SceneReferenceDrawerHelpers.LoadSceneByGuid(guidProp.stringValue);

            EditorGUI.BeginChangeCheck();
            var newScene = EditorGUI.ObjectField(position, label, scene, typeof(SceneAsset), false) as SceneAsset;
            if (EditorGUI.EndChangeCheck())
            {
                if (newScene == null)
                {
                    guidProp.stringValue = string.Empty;
                    nameProp.stringValue = string.Empty;
                }
                else
                {
                    string path = AssetDatabase.GetAssetPath(newScene);
                    guidProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                    nameProp.stringValue = Path.GetFileNameWithoutExtension(path);
                }
            }

            EditorGUI.EndProperty();
        }
    }

    internal static class SceneReferenceDrawerHelpers
    {
        public static SceneAsset LoadSceneByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return null;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }
    }
}