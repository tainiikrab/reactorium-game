using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.Chemistry.Visuals
{
[ExecuteAlways]
public class FlaskFillLevelAnimator : FillLevelAnimator
{
    [Header("Flask parameters")] [SerializeField]
    private float _minScale;

    [SerializeField] private float _maxScale;

    protected override float GetLiquidScaleForFill(float fillLevel)
    {
        return Mathf.Lerp(_maxScale, _minScale, fillLevel);
    }

    protected override void AnimateFill(float fillLevel)
    {
        if (!Application.isPlaying)
        {
            ApplyImmediateState();
            return;
        }

        base.AnimateFill(fillLevel);
        isChangingScale = true;
        float targetScale = GetLiquidScaleForFill(fillLevel);
        _currentLiquidScale = targetScale;
        Tween.Scale(_liquid, new Vector3(_desiredLiquidXScale * targetScale, targetScale, targetScale), _fillDuration,
                _ease)
            .OnComplete(() => { isChangingScale = false; });
    }

    public override void ApplyImmediateState()
    {
        if (ChemContainer == null)
            BindContainer();

        if (ChemContainer != null)
            _currentLiquidScale = GetLiquidScaleForFill(ChemContainer.Contents.CurrentFillLevel);

        isChangingScale = false;
        base.ApplyImmediateState();
    }
}
}