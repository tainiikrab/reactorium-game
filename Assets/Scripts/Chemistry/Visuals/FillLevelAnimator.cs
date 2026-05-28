using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.Chemistry.Visuals
{
[ExecuteAlways]
public class FillLevelAnimator : MonoBehaviour
{
    [Header("References")] [SerializeField]
    protected Transform _liquid;

    [Header("Fill Settings")] [SerializeField]
    protected float _minY;

    [SerializeField] protected float _maxY;
    [SerializeField] protected float _fillDuration = 0.4f;
    [SerializeField] protected Ease _ease;

    [Header("Color")]
    [SerializeField] private float _colorTransitionDuration = 0.45f;
    [SerializeField] private Ease _colorEase = Ease.OutCubic;

    [Header("Scale Settings")] [SerializeField]
    private float _defaultXScale = 1f;

    [SerializeField] private float _maxXScale = 2f;
    [SerializeField] private float _scaleSmoothTime = 0.1f;

    [Header("Tilt Settings")] [SerializeField]
    private float _tiltStrength = 2f;

    [SerializeField] private float _maxTilt = 25f;
    [SerializeField] private float _inertiaTime = 0.2f;

    [Header("Horizontal Offset")]
    [Tooltip("Max local X shift at 90° tilt and 0%/100% fill. Zero at upright or 50% fill.")]
    [SerializeField] private float _maxHorizontalOffset = 2f;

    [Tooltip("How strongly X offset grows above 50% fill, relative to below 50%. 0.5 = half speed.")]
    [SerializeField] [Range(0f, 1f)] private float _highFillHorizontalOffsetScale = 0.5f;

    [Tooltip("Past 90°, offset grows this many times faster per degree than from 0–90°. 2 = double rate.")]
    [SerializeField] private float _past90RotationMultiplier = 2f;

    private const float ReferenceTiltDegrees = 90f;

    protected ChemContainer ChemContainer { get; private set; }

    private float _baseLiquidX;
    private float _currentFillLevel;
    protected float _currentLiquidScale;
    protected float _desiredLiquidXScale;

    private float _currentLiquidAngle;
    private float _angularVelocity;

    private Vector3 _previousPosition;
    private Vector3 _worldVelocity;
    private float _scaleVelocity;

    protected bool isChangingScale;

    public SpriteRenderer LiquidRenderer { get; private set; }

    private Tween _colorTween;

    private void OnEnable()
    {
        if (!BindContainer()) return;

        ChemContainer.Contents.OnFillLevelChanged += AnimateFill;
        ChemContainer.Contents.OnColorChanged += ApplyLiquidColor;

        CacheLiquidRefs();
        _previousPosition = transform.position;

        if (Application.isPlaying)
            SyncFromContentsAtStartup();
        else
            ApplyImmediateState();
    }

    private void OnDisable()
    {
        if (_colorTween.isAlive)
            _colorTween.Stop();

        UnsubscribeFromContainer();
    }

    protected bool BindContainer()
    {
        if (ChemContainer) return true;

        ChemContainer = GetComponentInParent<ChemContainer>();
        return ChemContainer;
    }

    private void UnsubscribeFromContainer()
    {
        if (!ChemContainer) return;

        ChemContainer.Contents.OnFillLevelChanged -= AnimateFill;
        ChemContainer.Contents.OnColorChanged -= ApplyLiquidColor;
    }

    private void CacheLiquidRefs()
    {
        if (!_liquid) return;

        _baseLiquidX = _liquid.localPosition.x;
        _currentLiquidAngle = _liquid.eulerAngles.z;
        LiquidRenderer = _liquid.GetComponent<SpriteRenderer>();
    }

    private void SyncFromContentsAtStartup()
    {
        ApplyLiquidColor(ChemContainer.Contents.CurrentColor);

        float fillLevel = ChemContainer.Contents.CurrentFillLevel;
        float targetY = Mathf.Lerp(_minY, _maxY, fillLevel);

        _liquid.localPosition = new Vector3(
            _liquid.localPosition.x,
            targetY,
            _liquid.localPosition.z);

        _currentFillLevel = fillLevel;
        _currentLiquidScale = GetLiquidScaleForFill(fillLevel);

        if (_currentFillLevel <= 0f)
        {
            TrySetActiveLiquid(false);
            if (LiquidRenderer)
            {
                LiquidRenderer.color = new Color(
                    LiquidRenderer.color.r,
                    LiquidRenderer.color.g,
                    LiquidRenderer.color.b,
                    0f);
            }
        }
        else
        {
            ApplyScaleState();
        }

        ApplyHorizontalOffset();
    }

    private void LateUpdate()
    {
        if (!_liquid || !Application.isPlaying) return;

        UpdateVelocity();
        UpdateTilt();

        if (!isChangingScale)
            UpdateScale();

        ApplyHorizontalOffset();
    }

    private void UpdateVelocity()
    {
        _worldVelocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
    }

    private void UpdateTilt()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(_worldVelocity);

        float inertiaTilt = -localVelocity.x * _tiltStrength;
        inertiaTilt = Mathf.Clamp(inertiaTilt, -_maxTilt, _maxTilt);

        float currentWorldZ = _liquid.eulerAngles.z;
        float desiredWorldZ = inertiaTilt;

        _currentLiquidAngle = Mathf.SmoothDampAngle(
            currentWorldZ,
            desiredWorldZ,
            ref _angularVelocity,
            _inertiaTime,
            Mathf.Infinity,
            Time.deltaTime);

        _liquid.rotation = Quaternion.Euler(0f, 0f, _currentLiquidAngle);
    }

    private void UpdateScale()
    {
        ApplyScaleState();
    }

    protected virtual float GetLiquidScaleForFill(float fillLevel)
    {
        return _liquid.localScale.x;
    }

    private static float NormalizeSignedZ(Transform t)
    {
        float z = t.eulerAngles.z;
        return z > 180f ? z - 360f : z;
    }

    private float ComputeFillBias()
    {
        if (_currentFillLevel <= 0.5f)
            return 0.5f - _currentFillLevel;

        return -(_currentFillLevel - 0.5f) * _highFillHorizontalOffsetScale;
    }

    private float ComputeRotationFactor()
    {
        float z = NormalizeSignedZ(transform);
        float absZ = Mathf.Abs(z);
        if (absZ <= 0f) return 0f;

        float sign = Mathf.Sign(z);

        if (absZ <= ReferenceTiltDegrees)
            return z / ReferenceTiltDegrees;

        float past90 = absZ - ReferenceTiltDegrees;
        return sign * (1f + past90 / ReferenceTiltDegrees * _past90RotationMultiplier);
    }

    private float ComputeHorizontalOffset()
    {
        return _maxHorizontalOffset * ComputeFillBias() * ComputeRotationFactor();
    }

    protected void ApplyHorizontalOffset()
    {
        if (!_liquid) return;

        Vector3 pos = _liquid.localPosition;
        pos.x = _baseLiquidX + ComputeHorizontalOffset();
        _liquid.localPosition = pos;
    }

    protected void ApplyScaleState()
    {
        float z = NormalizeSignedZ(transform);

        float abs = Mathf.Abs(z);
        float t = 1f - Mathf.Abs(abs - 90f) / 90f;

        float scaleModifier = Mathf.Lerp(1f, 3f * (_currentFillLevel + 1f), t);

        _desiredLiquidXScale = scaleModifier;

        _liquid.localScale = new Vector3(
            scaleModifier * _currentLiquidScale,
            _currentLiquidScale,
            _currentLiquidScale);
    }

    protected virtual void AnimateFill(float fillLevel)
    {
        if (!Application.isPlaying)
        {
            ApplyImmediateState();
            return;
        }

        Tween.StopAll(_liquid);
        Tween.StopAll(LiquidRenderer);

        TrySetActiveLiquid(true);

        if (fillLevel > 0f && _currentFillLevel <= 0f)
        {
            Color liquidColor = ChemContainer.Contents.CurrentColor;
            LiquidRenderer.color = new Color(liquidColor.r, liquidColor.g, liquidColor.b, 0f);
            Tween.Alpha(LiquidRenderer, liquidColor.a, _fillDuration * 0.5f, _ease);
        }

        _currentFillLevel = fillLevel;

        float targetY = Mathf.Lerp(_minY, _maxY, fillLevel);

        Tween.LocalPositionY(_liquid, targetY, _fillDuration, _ease)
            .OnComplete(() =>
            {
                if (fillLevel <= 0f)
                    Tween.Alpha(LiquidRenderer, 0f, _fillDuration, _ease)
                        .OnComplete(() => TrySetActiveLiquid(false));
            });
    }

    protected bool TrySetActiveLiquid(bool isActive)
    {
        if (_liquid.gameObject.activeSelf == isActive) return false;

        _liquid.gameObject.SetActive(isActive);
        return true;
    }

    private void ApplyLiquidColor(Color color)
    {
        if (!Application.isPlaying)
        {
            ApplyImmediateState();
            return;
        }

        if (!LiquidRenderer) return;

        if (ChemContainer.Contents.CurrentFillLevel <= 0f)
            return;

        if (_colorTransitionDuration <= 0f)
        {
            LiquidRenderer.color = color;
            return;
        }

        if (_colorTween.isAlive)
            _colorTween.Stop();

        Color from = LiquidRenderer.color;
        _colorTween = Tween.Custom(LiquidRenderer, 0f, 1f, _colorTransitionDuration,
            (renderer, t) => renderer.color = Color.Lerp(from, color, t),
            _colorEase);
    }

    public virtual void ApplyImmediateState()
    {
        if (!_liquid) return;

        if (!BindContainer()) return;

        CacheLiquidRefs();
        isChangingScale = false;

        float fillLevel = ChemContainer.Contents.CurrentFillLevel;
        _currentFillLevel = fillLevel;

        float targetY = Mathf.Lerp(_minY, _maxY, fillLevel);
        Vector3 pos = _liquid.localPosition;
        _liquid.localPosition = new Vector3(pos.x, targetY, pos.z);

        Color color = ChemContainer.Contents.CurrentColor;
        if (fillLevel <= 0f)
        {
            TrySetActiveLiquid(false);
            if (LiquidRenderer)
                LiquidRenderer.color = new Color(color.r, color.g, color.b, 0f);
            return;
        }

        TrySetActiveLiquid(true);
        if (LiquidRenderer)
            LiquidRenderer.color = new Color(color.r, color.g, color.b, color.a);

        if (_currentLiquidScale <= 0f)
            _currentLiquidScale = _liquid.localScale.y;

        ApplyScaleState();
        ApplyHorizontalOffset();
    }

    private void OnDestroy()
    {
        UnsubscribeFromContainer();
    }
}
}
