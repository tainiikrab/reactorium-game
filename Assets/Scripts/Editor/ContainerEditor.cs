using ChemSimDiploma.Chemistry;
using UnityEditor;
using UnityEngine;

namespace ChemSimDiploma.Editor
{
    [CustomEditor(typeof(ChemContainer))]
    public class ContainerEditor : UnityEditor.Editor
    {
        private float _editorFillValue;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(8);
            EditorGUILayout.LabelField("Editor Fill Control", EditorStyles.boldLabel);

            var container = (ChemContainer)target;

            _editorFillValue = EditorGUILayout.Slider(
                "Fill (0-1)",
                _editorFillValue,
                0f,
                1f);

            if (GUILayout.Button("Apply Fill Level"))
            {
                Undo.RecordObject(container, "Set Fill Level");

                float clamped = Mathf.Clamp01(_editorFillValue);
                container.Contents.SetFillLevel(clamped);

                EditorUtility.SetDirty(container);
            }
        }
    }
}