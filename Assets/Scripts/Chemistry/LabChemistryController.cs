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

    private Action<ContainerHeatedSignal> _onContainerHeated;



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

        _onContainerHeated = OnContainerHeated;

        _signalBus.Subscribe(_onLiquidPoured);

        _signalBus.Subscribe(_onContainerHeated);

    }



    public void Dispose()

    {

        if (_signalBus == null) return;



        if (_onLiquidPoured != null) _signalBus.Unsubscribe(_onLiquidPoured);

        if (_onContainerHeated != null) _signalBus.Unsubscribe(_onContainerHeated);

        _onLiquidPoured = null;

        _onContainerHeated = null;

    }



    private void OnLiquidPoured(LiquidPouredSignal signal)

    {

        if (signal.VolumeMl <= 1e-6f || signal.Destination == null) return;

        ProcessAndNotify(signal.Destination);

    }



    private void OnContainerHeated(ContainerHeatedSignal signal)

    {

        if (signal.Container == null) return;

        ProcessAndNotify(signal.Container);

    }



    private void ProcessAndNotify(ChemContainer container)

    {

        _reactionService.Process(container.Contents);

        _signalBus.Fire(new ContainerChemistryUpdatedSignal

        {

            Container = container,

            Contents = container.Contents

        });

    }

}

}

