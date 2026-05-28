#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChemSimDiploma.Editor
{
public static class EditorSelectionUtility
{
    public static void RemoveFromSelection(Object obj)
    {
        if (obj == null) return;

        Object[] current = Selection.objects;
        if (current == null || current.Length == 0) return;

        bool contains = false;
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] == obj)
            {
                contains = true;
                break;
            }
        }

        if (!contains) return;

        var filtered = new List<Object>(current.Length);
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] != null && current[i] != obj)
                filtered.Add(current[i]);
        }

        Selection.objects = filtered.ToArray();
    }

    public static void PurgeDestroyedFromSelection()
    {
        Object[] current = Selection.objects;
        if (current == null || current.Length == 0) return;

        bool anyDestroyed = false;
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] == null)
            {
                anyDestroyed = true;
                break;
            }
        }

        if (!anyDestroyed) return;

        var filtered = new List<Object>(current.Length);
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] != null)
                filtered.Add(current[i]);
        }

        Selection.objects = filtered.ToArray();
    }
}
}
#endif
