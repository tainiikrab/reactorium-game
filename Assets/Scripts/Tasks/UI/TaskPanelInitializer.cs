using ChemSimDiploma.Tasks.Data;
using ChemSimDiploma.UI.Level;
using UnityEngine;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChemSimDiploma.Tasks.UI
{
[ExecuteAlways]
public class TaskPanelInitializer : MonoBehaviour, IInitializable
{
    [SerializeField] private Transform _taskHolder;
    [SerializeField] private UITaskBar _taskBarPrefab;
    [SerializeField] private LevelTaskSetSO _taskSetFallback;

    private LevelTaskSetSO _injectedTaskSet;

    [Inject]
    public void Construct([Inject(Optional = true)] LevelTaskSetSO taskSet)
    {
        _injectedTaskSet = taskSet;
        RequestSync();
    }

    private void OnEnable()
    {
        RequestSync();
    }

    private void OnValidate()
    {
        RequestSyncDeferred();
    }

    public void Initialize()
    {
        SyncPanel();
    }

    public LevelTaskSetSO ResolvedTaskSet => _injectedTaskSet != null ? _injectedTaskSet : _taskSetFallback;

    public bool UsesTaskSet(LevelTaskSetSO taskSet)
    {
        return taskSet != null && ResolvedTaskSet == taskSet;
    }

    public void SyncPanel()
    {
        LevelTaskSetSO taskSet = ResolvedTaskSet;
        if (taskSet == null) return;

        Transform holder = _taskHolder != null ? _taskHolder : transform;
        bool recordUndo = !Application.isPlaying;

        if (TaskPanelSync.Apply(taskSet, holder, _taskBarPrefab, recordUndo))
            CacheTaskBarReferences(holder);
    }

    private void CacheTaskBarReferences(Transform holder)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(this);
#endif
    }

    private void RequestSync()
    {
        if (!isActiveAndEnabled) return;
        SyncPanel();
    }

    private void RequestSyncDeferred()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;

        EditorApplication.delayCall += DeferredSync;
#endif
    }

#if UNITY_EDITOR
    private void DeferredSync()
    {
        EditorApplication.delayCall -= DeferredSync;
        if (this == null) return;
        SyncPanel();
    }
#endif
}
}