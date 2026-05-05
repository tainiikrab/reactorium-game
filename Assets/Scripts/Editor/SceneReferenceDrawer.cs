using System.IO;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public sealed class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var guidProp = property.FindPropertyRelative("sceneAssetGuid");
        var nameProp = property.FindPropertyRelative("sceneName");

        var scene = SceneReferenceDrawerHelpers.LoadSceneByGuid(guidProp.stringValue);

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
                var path = AssetDatabase.GetAssetPath(newScene);
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
        var path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
    }
}
