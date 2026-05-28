using System.Collections.Generic;
using ChemSimDiploma.Tasks.Data;
using ChemSimDiploma.UI.Level;
using UnityEngine;

namespace ChemSimDiploma.Tasks.UI
{
public static class TaskPanelSync
{
    private const string DefaultTaskBarPrefabPath = "Assets/Prefabs/UITaskBar.prefab";

    public static bool Apply(
        LevelTaskSetSO taskSet,
        Transform taskHolder,
        UITaskBar taskBarPrefab,
        bool recordUndo)
    {
        if (taskSet == null || taskHolder == null)
            return false;

        UITaskBar prefab = ResolvePrefab(taskBarPrefab);
        if (prefab == null)
        {
            Debug.LogWarning("[TaskPanelSync] UITaskBar prefab is not assigned.", taskHolder);
            return false;
        }

        LevelTaskEntry[] tasks = taskSet.Tasks;
        int targetCount = tasks?.Length ?? 0;

        List<UITaskBar> bars = CollectDirectTaskBars(taskHolder);

        for (int i = bars.Count - 1; i >= targetCount; i--)
            DestroyTaskBar(bars[i], recordUndo);

        bars = CollectDirectTaskBars(taskHolder);

        while (bars.Count < targetCount)
        {
            UITaskBar created = CreateTaskBar(prefab, taskHolder, recordUndo);
            if (created == null) break;
            bars.Add(created);
        }

        bars = CollectDirectTaskBars(taskHolder);
        int applyCount = Mathf.Min(bars.Count, targetCount);

        for (int i = 0; i < applyCount; i++)
        {
            LevelTaskEntry entry = tasks[i];
            UITaskBar bar = bars[i];

            if (entry != null && !string.IsNullOrWhiteSpace(entry.Description))
                bar.SetLabel(entry.Description);

            if (!Application.isPlaying)
                bar.SetCompleted(false);
        }

        return true;
    }

    public static List<UITaskBar> CollectDirectTaskBars(Transform taskHolder)
    {
        var bars = new List<UITaskBar>();
        if (taskHolder == null) return bars;

        for (int i = 0; i < taskHolder.childCount; i++)
        {
            Transform child = taskHolder.GetChild(i);
            if (child.TryGetComponent(out UITaskBar bar))
                bars.Add(bar);
        }

        bars.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        return bars;
    }

    private static UITaskBar ResolvePrefab(UITaskBar assigned)
    {
        if (assigned != null) return assigned;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            return UnityEditor.AssetDatabase.LoadAssetAtPath<UITaskBar>(DefaultTaskBarPrefabPath);
#endif
        return null;
    }

    private static UITaskBar CreateTaskBar(UITaskBar prefab, Transform holder, bool recordUndo)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UITaskBar instance = (UITaskBar)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, holder);
            if (instance == null) return null;

            instance.name = prefab.name;
            instance.transform.SetAsLastSibling();

            if (recordUndo)
                UnityEditor.Undo.RegisterCreatedObjectUndo(instance.gameObject, "Sync Task Panel");

            UnityEditor.EditorUtility.SetDirty(holder);
            return instance;
        }
#endif
        UITaskBar runtimeInstance = Object.Instantiate(prefab, holder);
        runtimeInstance.name = prefab.name;
        runtimeInstance.transform.SetAsLastSibling();
        return runtimeInstance;
    }

    private static void DestroyTaskBar(UITaskBar bar, bool recordUndo)
    {
        if (bar == null) return;

        GameObject go = bar.gameObject;

#if UNITY_EDITOR
        RemoveFromEditorSelection(go);

        if (!Application.isPlaying)
        {
            if (recordUndo)
                UnityEditor.Undo.DestroyObjectImmediate(go);
            else
                Object.DestroyImmediate(go);

            return;
        }
#endif
        Object.Destroy(go);
    }

#if UNITY_EDITOR
    private static void RemoveFromEditorSelection(GameObject go)
    {
        if (go == null) return;

        Object[] current = UnityEditor.Selection.objects;
        if (current == null || current.Length == 0) return;

        bool contains = false;
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] == go)
            {
                contains = true;
                break;
            }
        }

        if (!contains) return;

        var filtered = new System.Collections.Generic.List<Object>(current.Length);
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] != null && current[i] != go)
                filtered.Add(current[i]);
        }

        UnityEditor.Selection.objects = filtered.ToArray();
    }
#endif
}
}
