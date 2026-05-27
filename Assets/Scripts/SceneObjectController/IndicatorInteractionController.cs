using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Indicator;
using UnityEngine;

namespace ChemSimDiploma.SceneObjectController
{
public class IndicatorInteractionController : MonoBehaviour
{
    public void OnContainersAttached(IDraggable source, IDraggable destination)
    {
        if (source == null || destination == null) return;

        if (!source.Transform.TryGetComponent(out IndicatorStickController stick))
            return;

        if (destination.Transform.TryGetComponent(out IndicatorBoxController _))
            return;

        if (destination.Transform.TryGetComponent(out ChemContainer container))
            stick.DipInto(container);
    }
}
}