using System;
using PrimeTween;
using UnityEngine;
namespace ChemSimDiploma.SceneObjectController
{

public class InteractionService
{
    private readonly float _duration;
    private readonly Ease _ease;

    private Tween _move;
    private Tween _rotate;

    public event Action<IDraggable, IDraggable> Attached;

    public InteractionService(float duration, Ease ease)
    {
        _duration = duration;
        _ease = ease;
    }

    public void Attach(IDraggable from, IDraggable to)
    {
        Attach(from, to, _duration, _ease);
    }

    public void Attach(IDraggable from, IDraggable to, float duration, Ease ease)
    {
        StopTweens();

        var t = from.Transform;
        Transform p = AttachRules.GetAttachTransform(from, to);

        _move = Tween.Position(t, p.position, duration, ease);
        _rotate = Tween.Rotation(t, p.rotation, duration, ease)
            .OnComplete(() => Attached?.Invoke(from, to));

        from.Receiver = to;
        from.Sender = null;

        to.Sender = from;
        to.Receiver = null;

        if (duration <= 0f)
            Attached?.Invoke(from, to);
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
