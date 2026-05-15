using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.Chemistry.Visuals
{
public class FillLevelAnimator : MonoBehaviour
{
    [Header("References")] [SerializeField]
    protected Transform _liquid;

    [Header("Fill Settings")] [SerializeField]
    protected float _minY;

    [SerializeField] protected float _maxY;
    [SerializeField] protected float _fillDuration = 0.4f;
    [SerializeField] protected Ease _ease;

    [Header("Scale Settings")] [SerializeField]
    private float _defaultXScale = 1f;

    [SerializeField] private float _maxXScale = 2f;
    [SerializeField] private float _scaleSmoothTime = 0.1f;

    [Header("Tilt Settings")] [SerializeField]
    private float _tiltStrength = 2f;

    [SerializeField] private float _maxTilt = 25f;
    [SerializeField] private float _inertiaTime = 0.2f;

    private ChemContainer _chemContainer;

    private float _currentFillLevel;
    protected float _currentLiquidScale;
    protected float _desiredLiquidXScale;

    private float _currentLiquidAngle;
    private float _angularVelocity;

    private Vector3 _previousPosition;
    private Vector3 _worldVelocity;
    private float _scaleVelocity;

    protected bool isChangingScale = false;

    public SpriteRenderer LiquidRenderer { get; private set; }

    protected virtual void Awake()
    {
        _chemContainer = GetComponentInParent<ChemContainer>();
        _chemContainer.Contents.OnFillLevelChanged += AnimateFill;
        _chemContainer.Contents.OnColorChanged += ApplyLiquidColor;

        _currentLiquidScale = _liquid.localScale.x;
        _currentLiquidAngle = _liquid.eulerAngles.z;

        LiquidRenderer = _liquid.GetComponent<SpriteRenderer>();

        ApplyLiquidColor(_chemContainer.Contents.CurrentColor);

        float fillLevel = _chemContainer.Contents.CurrentFillLevel;
        float targetY = Mathf.Lerp(_minY, _maxY, fillLevel);

        _liquid.localPosition = new Vector3(
            _liquid.localPosition.x,
            targetY,
            _liquid.localPosition.z);

        _currentFillLevel = fillLevel;

        if (_currentFillLevel <= 0f)
        {
            TrySetActiveLiquid(false);
            LiquidRenderer.color = new Color(
                LiquidRenderer.color.r,
                LiquidRenderer.color.g,
                LiquidRenderer.color.b,
                0f);
        }

        _previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (!_liquid) return;

        UpdateVelocity();
        UpdateTilt();

        if (isChangingScale) return;

        UpdateScale();
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
        float z = transform.eulerAngles.z;
        z = z > 180f ? z - 360f : z;

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
        Tween.StopAll(_liquid);
        Tween.StopAll(LiquidRenderer);

        TrySetActiveLiquid(true);

        if (fillLevel > 0f && _currentFillLevel <= 0f)
        {
            Color liquidColor = _chemContainer.Contents.CurrentColor;
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
        if (!LiquidRenderer) return;

        if (_chemContainer.Contents.CurrentFillLevel <= 0f)
            return;

        LiquidRenderer.color = color;
    }

    public void ApplyImmediateState()
    {
        if (!_liquid) return;

        if (!_chemContainer)
            _chemContainer = GetComponentInParent<ChemContainer>();
        if (!_chemContainer) return;

        if (!LiquidRenderer)
            LiquidRenderer = _liquid.GetComponent<SpriteRenderer>();

        float fillLevel = _chemContainer.Contents.CurrentFillLevel;
        _currentFillLevel = fillLevel;

        float targetY = Mathf.Lerp(_minY, _maxY, fillLevel);
        Vector3 pos = _liquid.localPosition;
        _liquid.localPosition = new Vector3(pos.x, targetY, pos.z);

        Color color = _chemContainer.Contents.CurrentColor;
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
    }

    private void OnDestroy()
    {
        if (!_chemContainer) return;

        _chemContainer.Contents.OnFillLevelChanged -= AnimateFill;
        _chemContainer.Contents.OnColorChanged -= ApplyLiquidColor;
    }
}
}