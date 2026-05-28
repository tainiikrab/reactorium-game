#if UNITY_EDITOR
using UnityEditor;

namespace ChemSimDiploma.Editor
{
[InitializeOnLoad]
internal static class InspectorDestroyedObjectGuard
{
    static InspectorDestroyedObjectGuard()
    {
        EditorApplication.update += EditorSelectionUtility.PurgeDestroyedFromSelection;
    }
}
}
#endif
