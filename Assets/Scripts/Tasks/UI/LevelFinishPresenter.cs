using System;
using System.Collections;
using ChemSimDiploma.Tasks.Data;
using ChemSimDiploma.Tasks.Signals;
using ChemSimDiploma.UI;
using UnityEngine;
using Zenject;

namespace ChemSimDiploma.Tasks.UI
{
public class LevelFinishPresenter : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private UIPopup _finishPopup;
    [SerializeField] private float _delayOverride = -1f;
    [SerializeField] private LevelTaskSetSO _taskSetFallback;

    private SignalBus _signalBus;
    private LevelTaskSetSO _taskSet;
    private Action<AllTasksCompletedSignal> _onAllTasksCompleted;
    private Coroutine _openCoroutine;

    [Inject]
    public void Construct(SignalBus signalBus, [Inject(Optional = true)] LevelTaskSetSO taskSet)
    {
        _signalBus = signalBus;
        _taskSet = taskSet != null ? taskSet : _taskSetFallback;
    }

    private void Awake()
    {
        if (_finishPopup == null)
            _finishPopup = GetComponent<UIPopup>();
    }

    public void Initialize()
    {
        _onAllTasksCompleted = OnAllTasksCompleted;
        _signalBus.Subscribe(_onAllTasksCompleted);
    }

    public void Dispose()
    {
        if (_signalBus != null && _onAllTasksCompleted != null)
            _signalBus.Unsubscribe(_onAllTasksCompleted);

        _onAllTasksCompleted = null;

        if (_openCoroutine != null)
        {
            StopCoroutine(_openCoroutine);
            _openCoroutine = null;
        }
    }

    private void OnAllTasksCompleted(AllTasksCompletedSignal _)
    {
        if (_openCoroutine != null)
            StopCoroutine(_openCoroutine);

        float delay = _delayOverride >= 0f
            ? _delayOverride
            : _taskSet != null ? _taskSet.FinishPopupDelaySeconds : 2.5f;

        _openCoroutine = StartCoroutine(OpenAfterDelay(delay));
    }

    private IEnumerator OpenAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (_finishPopup != null)
            _finishPopup.Open();

        _openCoroutine = null;
    }
}
}
