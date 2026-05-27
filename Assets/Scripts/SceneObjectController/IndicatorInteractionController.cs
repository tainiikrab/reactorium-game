using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Indicator;
using ChemSimDiploma.Tasks.Signals;
using UnityEngine;
using Zenject;

namespace ChemSimDiploma.SceneObjectController
{
public class IndicatorInteractionController : MonoBehaviour
{
    private SignalBus _signalBus;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void OnContainersAttached(IDraggable source, IDraggable destination)
    {
        if (source == null || destination == null) return;

        if (!source.Transform.TryGetComponent(out IndicatorStickController stick))
            return;

        if (destination.Transform.TryGetComponent(out IndicatorBoxController _))
            return;

        if (!destination.Transform.TryGetComponent(out ChemContainer container))
            return;

        stick.DipInto(container);

        if (container.Contents.CurrentFillLevel <= 0f || !stick.HasBeenDipped)
            return;

        _signalBus?.Fire(new IndicatorDippedSignal
        {
            Container = container,
            MeasuredPh = stick.StoredPh,
            Stick = stick
        });
    }
}
}