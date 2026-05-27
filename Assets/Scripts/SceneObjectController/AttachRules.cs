using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Indicator;
using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.SceneObjectController
{
public static class AttachRules
{
    public static bool CanAttach(IDraggable from, IDraggable to)
    {
        if (from == null || to == null) return false;

        if (from.Transform.TryGetComponent(out IndicatorStickController stick))
        {
            if (to.Transform.TryGetComponent(out IndicatorBoxController box))
                return stick.CanReturnTo(box);

            return to.Transform.TryGetComponent(out ChemContainer _)
                   && to.Transform.TryGetComponent(out IndicatorDipPoint _);
        }

        if (from.Transform.TryGetComponent(out ChemContainer _))
            return to.Transform.TryGetComponent(out ChemContainer _)
                   && !to.Transform.TryGetComponent(out IndicatorStickController _)
                   && !to.Transform.TryGetComponent(out IndicatorBoxController _);

        return false;
    }

    public static Transform GetAttachTransform(IDraggable from, IDraggable to)
    {
        if (from.Transform.TryGetComponent(out IndicatorStickController _)
            && to.Transform.TryGetComponent(out IndicatorDipPoint dip))
            return dip.AttachTransform;

        if (from.Transform.TryGetComponent(out IndicatorStickController stick)
            && to.Transform.TryGetComponent(out IndicatorBoxController box))
            return box.GetReturnAnchorFor(stick);

        return to.InteractPoint;
    }

    public static float GetAttachDuration(IDraggable from, IDraggable to, float defaultDuration)
    {
        if (from.Transform.TryGetComponent(out IndicatorStickController _)
            && to.Transform.TryGetComponent(out IndicatorBoxController box))
            return box.ReturnAttachDuration;

        return defaultDuration;
    }

    public static Ease GetAttachEase(IDraggable from, IDraggable to, Ease defaultEase)
    {
        if (from.Transform.TryGetComponent(out IndicatorStickController _)
            && to.Transform.TryGetComponent(out IndicatorBoxController box))
            return box.ReturnAttachEase;

        return defaultEase;
    }
}
}