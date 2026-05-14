using PrimeTween;
using UnityEngine;
using ChemSimDiploma.Chemistry;
namespace ChemSimDiploma.SceneObjectController
{

public class InteractionService
{
    private readonly float _duration;
    private readonly Ease _ease;

    private Tween _move;
    private Tween _rotate;

    public InteractionService(float duration, Ease ease)
    {
        _duration = duration;
        _ease = ease;
    }

    public void Attach(IDraggable from, IDraggable to)
    {
        StopTweens();

        var t = from.Transform;
        var p = to.InteractPoint;

        _move = Tween.Position(t, p.position, _duration, _ease);
        _rotate = Tween.Rotation(t, p.rotation, _duration, _ease);

        from.Receiver = to;
        from.Sender = null;

        to.Sender = from;
        to.Receiver = null;
    }

    public void TryDetach(IDraggable target)
    {
        if (target.Sender != null)
        {
            StopTweens();

            _rotate = Tween.Rotation(target.Sender.Transform, Quaternion.identity, 0.15f);
            _move = Tween.PositionY(
                target.Sender.Transform,
                target.Transform.position.y,
                0.15f);

            target.Sender.Receiver = null;
            target.Sender = null;
        }
        else if (target.Receiver != null)
        {
            StopTweens();
            _rotate = Tween.Rotation(target.Transform, Quaternion.identity, 0.15f);
            target.Receiver.Sender = null;
            target.Receiver = null;
        }
    }

    private void StopTweens()
    {
        _move.Stop();
        _rotate.Stop();
    }
}
}
