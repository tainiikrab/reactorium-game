using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Container))]
public class ContainerEditor : Editor
{
    private float _editorFillValue;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Editor Fill Control", EditorStyles.boldLabel);

        var container = (Container)target;

        _editorFillValue = EditorGUILayout.Slider(
            "Fill (0-1)",
            _editorFillValue,
            0f,
            1f);

        if (GUILayout.Button("Apply Fill Level"))
        {
            Undo.RecordObject(container, "Set Fill Level");

            var clamped = Mathf.Clamp01(_editorFillValue);
            container.SetFillLevel(clamped);

            EditorUtility.SetDirty(container);
        }
    }
}