using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
namespace ChemSimDiploma.SceneObjectController
{

public class ReleaseFallService
{
    private readonly ObjectPerspectiveScaler _scaler;
    private readonly float _secondsPerUnit;
    private readonly float _durationMin;
    private readonly float _durationMax;
    private readonly Ease _mainEase;
    private readonly float _heightEpsilon;
    private readonly ReleaseFallBounceSettings _bounce;

    private readonly Dictionary<Transform, Sequence> _active = new();

    public ReleaseFallService(
        ObjectPerspectiveScaler scaler,
        float secondsPerUnit,
        float durationMin,
        float durationMax,
        Ease mainEase,
        float heightEpsilon,
        ReleaseFallBounceSettings bounce)
    {
        _scaler = scaler;
        _secondsPerUnit = secondsPerUnit;
        _durationMin = durationMin;
        _durationMax = durationMax;
        _mainEase = mainEase;
        _heightEpsilon = heightEpsilon;
        _bounce = bounce ?? new ReleaseFallBounceSettings();
    }

    public void OnGrabStarted(Transform grabbed)
    {
        if (grabbed == null) return;
        StopActive(grabbed);
    }

    public void TryPlayAfterFreeRelease(IDraggable released)
    {
        if (released is not IFallsToRestWhenFree faller) return;
        if (!faller.EnableFallToRest) return;
        if (released.Receiver != null || released.Sender != null) return;

        Transform t = faller.Transform;
        if (t == null) return;

        float currentY = t.position.y;
        float restY = faller.MinFallHeight;
        float distance = currentY - restY;

        if (distance <= _heightEpsilon) return;

        StopActive(t);

        float duration = Mathf.Clamp(distance * _secondsPerUnit, _durationMin, _durationMax);
        var b = _bounce;
        float bounceHeight = Mathf.Clamp(distance * b.heightFactor, b.heightMin, b.heightMax);

        if (_scaler != null)
            _scaler.RegisterExtraScaleTarget(t);

        Sequence seq = Sequence.Create()
            .Chain(Tween.PositionY(t, restY, duration, _mainEase))
            .Chain(Tween.PositionY(t, restY + bounceHeight, b.upDuration, b.upEase))
            .Chain(Tween.PositionY(t, restY, b.downDuration, b.downEase))
            .ChainCallback(() => Finish(t));

        _active[t] = seq;
    }

    private void StopActive(Transform t)
    {
        if (!_active.TryGetValue(t, out Sequence seq)) return;
        if (seq.isAlive) seq.Stop();
        Finish(t);
    }

    private void Finish(Transform t)
    {
        _active.Remove(t);
        if (_scaler != null && t != null)
            _scaler.UnregisterExtraScaleTarget(t);
    }
}
}
