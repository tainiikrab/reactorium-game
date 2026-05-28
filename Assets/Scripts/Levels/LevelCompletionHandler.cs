using System;
using ChemSimDiploma.Tasks.Signals;
using Zenject;

namespace ChemSimDiploma.Levels
{
public sealed class LevelCompletionHandler : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly ILevelProgressService _progressService;
    private readonly int _levelNumber;
    private Action<AllTasksCompletedSignal> _onAllTasksCompleted;

    public LevelCompletionHandler(SignalBus signalBus, ILevelProgressService progressService, int levelNumber)
    {
        _signalBus = signalBus;
        _progressService = progressService;
        _levelNumber = Math.Max(1, levelNumber);
    }

    public void Initialize()
    {
        _onAllTasksCompleted = OnAllTasksCompleted;
        _signalBus.Subscribe(_onAllTasksCompleted);
    }

    public void Dispose()
    {
        if (_onAllTasksCompleted == null)
            return;

        _signalBus.Unsubscribe(_onAllTasksCompleted);
        _onAllTasksCompleted = null;
    }

    private void OnAllTasksCompleted(AllTasksCompletedSignal _)
    {
        _progressService.CompleteLevel(_levelNumber);
    }
}
}
