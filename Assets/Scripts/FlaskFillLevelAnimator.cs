using PrimeTween;
using UnityEngine;

public class FlaskFillLevelAnimator : FillLevelAnimator
{
    [Header("Flask parameters")] [SerializeField]
    private float _minScale;

    [SerializeField] private float _maxScale;

    protected override void AnimateFill(float fillLevel)
    {
        base.AnimateFill(fillLevel);

        var targetScale = Mathf.Lerp(_maxScale, _minScale, fillLevel);
        Tween.Scale(_liquid, new Vector3(targetScale, targetScale, 1f), _fillDuration, _ease);
    }
}