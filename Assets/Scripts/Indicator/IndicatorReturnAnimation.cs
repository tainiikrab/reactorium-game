using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.Indicator
{
public readonly struct IndicatorReturnAnimation
{
    public readonly Transform SlotRoot;
    public readonly IndicatorStickController SlotStick;
    public readonly SpriteRenderer SlotRenderer;
    public readonly Vector3 SlotBaseLocalScale;
    public readonly Vector3 TargetWorldPosition;
    public readonly Quaternion TargetWorldRotation;
    public readonly float StoredPh;
    public readonly bool HasBeenDipped;
    public readonly float MoveDuration;
    public readonly float FadeDuration;
    public readonly float RevealDuration;
    public readonly float EndScaleFactor;
    public readonly Ease MoveEase;
    public readonly Ease FadeEase;
    public readonly Ease RevealEase;

    public IndicatorReturnAnimation(
        Transform slotRoot,
        IndicatorStickController slotStick,
        SpriteRenderer slotRenderer,
        Vector3 slotBaseLocalScale,
        Vector3 targetWorldPosition,
        Quaternion targetWorldRotation,
        float storedPh,
        bool hasBeenDipped,
        float moveDuration,
        float fadeDuration,
        float revealDuration,
        float endScaleFactor,
        Ease moveEase,
        Ease fadeEase,
        Ease revealEase)
    {
        SlotRoot = slotRoot;
        SlotStick = slotStick;
        SlotRenderer = slotRenderer;
        SlotBaseLocalScale = slotBaseLocalScale;
        TargetWorldPosition = targetWorldPosition;
        TargetWorldRotation = targetWorldRotation;
        StoredPh = storedPh;
        HasBeenDipped = hasBeenDipped;
        MoveDuration = moveDuration;
        FadeDuration = fadeDuration;
        RevealDuration = revealDuration;
        EndScaleFactor = endScaleFactor;
        MoveEase = moveEase;
        FadeEase = fadeEase;
        RevealEase = revealEase;
    }
}
}
