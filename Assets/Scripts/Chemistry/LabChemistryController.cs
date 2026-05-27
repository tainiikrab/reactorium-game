using System;
using ChemSimDiploma.Chemistry.ScriptableObjects;
using ChemSimDiploma.Chemistry.Signals;
using ChemSimDiploma.Tasks.Signals;
using UnityEngine;
using Zenject;

namespace ChemSimDiploma.Chemistry
{
public class LabChemistryController : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private ReactionRegistry _registry;

    private SignalBus _signalBus;
    private ReactionService _reactionService;
    private Action<LiquidPouredSignal> _onLiquidPoured;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        if (_registry == null)
        {
            Debug.LogError("[LabChemistryController] ReactionRegistry is not assigned.", this);
            return;
        }

        if (_signalBus == null)
        {
            Debug.LogError("[LabChemistryController] SignalBus was not injected.", this);
            return;
        }

        _reactionService = new ReactionService(_registry);
        _onLiquidPoured = OnLiquidPoured;
        _signalBus.Subscribe(_onLiquidPoured);
    }

    public void Dispose()
    {
        if (_signalBus == null || _onLiquidPoured == null) return;

        _signalBus.Unsubscribe(_onLiquidPoured);
        _onLiquidPoured = null;
    }

    private void OnLiquidPoured(LiquidPouredSignal signal)
    {
        if (signal.VolumeMl <= 1e-6f || signal.Destination == null) return;

        ChemContainer destination = signal.Destination;
        _reactionService.Process(destination.Contents);

        _signalBus.Fire(new ContainerChemistryUpdatedSignal
        {
            Container = destination,
            Contents = destination.Contents
        });
    }
}
}
