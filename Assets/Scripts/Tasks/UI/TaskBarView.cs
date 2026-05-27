using System;
using ChemSimDiploma.Tasks.Signals;
using ChemSimDiploma.UI.Level;
using UnityEngine;
using Zenject;

namespace ChemSimDiploma.Tasks.UI
{
public class TaskBarView : MonoBehaviour
{
    [SerializeField] private int _taskIndex;
    [SerializeField] private UITaskBar _taskBar;

    private SignalBus _signalBus;
    private Action<TaskCompletedSignal> _onTaskCompleted;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    private void Awake()
    {
        if (_taskBar == null)
            _taskBar = GetComponent<UITaskBar>();

        if (transform.parent != null)
            _taskIndex = transform.GetSiblingIndex();
    }

    private void Start()
    {
        if (_signalBus == null) return;

        _onTaskCompleted = OnTaskCompleted;
        _signalBus.Subscribe(_onTaskCompleted);
    }

    private void OnDestroy()
    {
        if (_signalBus == null || _onTaskCompleted == null) return;
        _signalBus.Unsubscribe(_onTaskCompleted);
        _onTaskCompleted = null;
    }

    private void OnTaskCompleted(TaskCompletedSignal signal)
    {
        if (signal.TaskIndex != _taskIndex) return;
        _taskBar.SetCompleted(true);
    }
}
}
