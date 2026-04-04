using PrimeTween;
using UnityEngine;

public class FlaskFillLevelAnimator : FillLevelAnimator
{
    [Header("Flask parameters")] [SerializeField]
    private float _minScale;

    [SerializeField] private float _maxScale;

    protected override void Awake()
    {
        base.Awake();
        _currentLiquidScale = _maxScale;
    }

    protected override void AnimateFill(float fillLevel)
    {
        base.AnimateFill(fillLevel);
        isChangingScale = true;
        var targetScale = Mathf.Lerp(_maxScale, _minScale, fillLevel);
        _currentLiquidScale = targetScale;
        Tween.Scale(_liquid, new Vector3(_desiredLiquidXScale * targetScale, targetScale, 1f), _fillDuration, _ease)
            .OnComplete(() => { isChangingScale = false; });
    }
}