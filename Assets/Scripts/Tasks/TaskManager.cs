using System;
using ChemSimDiploma.Chemistry.Signals;
using ChemSimDiploma.Tasks.Data;
using ChemSimDiploma.Tasks.Signals;
using UnityEngine;
using Zenject;

namespace ChemSimDiploma.Tasks
{
public class TaskManager : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly LevelTaskSetSO _taskSet;

    private int _currentIndex;
    private bool _allTasksCompletedFired;

    private Action<LiquidPouredSignal> _onLiquidPoured;
    private Action<ContainerChemistryUpdatedSignal> _onChemistryUpdated;
    private Action<IndicatorDippedSignal> _onIndicatorDipped;
    private Action<IndicatorStickSpawnedSignal> _onIndicatorStickSpawned;

    public TaskManager(SignalBus signalBus, LevelTaskSetSO taskSet)
    {
        _signalBus = signalBus;
        _taskSet = taskSet;
    }

    public void Initialize()
    {
        if (_taskSet == null || _taskSet.Tasks == null || _taskSet.Tasks.Length == 0)
        {
            Debug.LogWarning("[TaskManager] LevelTaskSetSO is missing or has no tasks.");
            return;
        }

        _onLiquidPoured = OnLiquidPoured;
        _onChemistryUpdated = OnChemistryUpdated;
        _onIndicatorDipped = OnIndicatorDipped;
        _onIndicatorStickSpawned = OnIndicatorStickSpawned;

        _signalBus.Subscribe(_onLiquidPoured);
        _signalBus.Subscribe(_onChemistryUpdated);
        _signalBus.Subscribe(_onIndicatorDipped);
        _signalBus.Subscribe(_onIndicatorStickSpawned);
    }

    public void Dispose()
    {
        if (_signalBus == null) return;

        if (_onLiquidPoured != null) _signalBus.Unsubscribe(_onLiquidPoured);
        if (_onChemistryUpdated != null) _signalBus.Unsubscribe(_onChemistryUpdated);
        if (_onIndicatorDipped != null) _signalBus.Unsubscribe(_onIndicatorDipped);
        if (_onIndicatorStickSpawned != null) _signalBus.Unsubscribe(_onIndicatorStickSpawned);

        _onLiquidPoured = null;
        _onChemistryUpdated = null;
        _onIndicatorDipped = null;
        _onIndicatorStickSpawned = null;
    }

    private void OnLiquidPoured(LiquidPouredSignal signal) =>
        TryCompleteCurrentTask(LevelTaskEvaluationContext.FromLiquidPoured(signal));

    private void OnChemistryUpdated(ContainerChemistryUpdatedSignal signal) =>
        TryCompleteCurrentTask(LevelTaskEvaluationContext.FromChemistryUpdated(signal));

    private void OnIndicatorDipped(IndicatorDippedSignal signal) =>
        TryCompleteCurrentTask(LevelTaskEvaluationContext.FromIndicatorDipped(signal));

    private void OnIndicatorStickSpawned(IndicatorStickSpawnedSignal signal) =>
        TryCompleteCurrentTask(LevelTaskEvaluationContext.FromIndicatorStickSpawned(signal));

    private void TryCompleteCurrentTask(LevelTaskEvaluationContext ctx)
    {
        if (_taskSet?.Tasks == null) return;

        while (_currentIndex < _taskSet.Tasks.Length)
        {
            LevelTaskEntry task = _taskSet.Tasks[_currentIndex];
            if (task == null) return;

            if (!task.HandlesSignal(ctx.SignalKind)) return;
            if (!task.IsMet(ctx)) return;

            _signalBus.Fire(new TaskCompletedSignal
            {
                TaskIndex = _currentIndex,
                TaskId = task.TaskId
            });

            _currentIndex++;
        }

        if (_currentIndex >= _taskSet.Tasks.Length)
            FireAllTasksCompletedOnce();
    }

    private void FireAllTasksCompletedOnce()
    {
        if (_allTasksCompletedFired) return;

        _allTasksCompletedFired = true;
        _signalBus.Fire(new AllTasksCompletedSignal());
    }
}
}
