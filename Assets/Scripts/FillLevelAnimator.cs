using UnityEngine;
using PrimeTween;

public class FillLevelAnimator : MonoBehaviour
{
    private Container _container;

    [SerializeField] protected Transform _liquid;
    [SerializeField] protected float _minY;
    [SerializeField] protected float _maxY;

    [Header("Animation Settings")] [SerializeField]
    protected float _fillDuration = 0.4f;

    [Range(0f, 0.5f)] public float _overshootAmount = 0.15f;

    [SerializeField] protected float _tiltAngle = 6f;
    [SerializeField] protected float _tiltDuration = 0.2f;

    [SerializeField] protected Ease _ease = default;

    [SerializeField] private float inertiaTime = 0.2f;
    private float _liquidAngle;
    private float _angularVelocity;

    private float _currentFillLevel;

    private SpriteRenderer _liquidRenderer;
    public SpriteRenderer LiquidRenderer => _liquidRenderer;

    protected void Awake()
    {
        _container = GetComponentInParent<Container>();
        _container.OnFillLevelChangedEvent += AnimateFill;

        _liquidAngle = _liquid.eulerAngles.z;

        _liquidRenderer = _liquid.GetComponent<SpriteRenderer>();

        var fillLevel = _container.CurrentFillLevel;
        var targetY = Mathf.Lerp(_minY, _maxY, fillLevel);
        _liquid.localPosition = new Vector3(_liquid.localPosition.x, targetY, _liquid.localPosition.z);

        _currentFillLevel = _container.CurrentFillLevel;
        if (_currentFillLevel <= 0f)
        {
            TrySetActiveLiquid(false);
            _liquidRenderer.color =
                new Color(_liquidRenderer.color.r, _liquidRenderer.color.g, _liquidRenderer.color.b, 0f);
        }
    }

    private void LateUpdate()
    {
        if (!_liquid) return;

        var currentWorldZ = _liquid.eulerAngles.z;
        var desiredWorldZ = 0f;

        _liquidAngle = Mathf.SmoothDampAngle(
            currentWorldZ,
            desiredWorldZ,
            ref _angularVelocity,
            inertiaTime,
            Mathf.Infinity,
            Time.deltaTime);

        _liquid.rotation = Quaternion.Euler(0f, 0f, _liquidAngle);
    }

    protected virtual void AnimateFill(float fillLevel)
    {
        Tween.StopAll(_liquid);
        Tween.StopAll(_liquidRenderer);

        TrySetActiveLiquid(true);

        if (fillLevel > 0f && _currentFillLevel <= 0f) Tween.Alpha(_liquidRenderer, 1f, _fillDuration * 0.5f, _ease);

        _currentFillLevel = fillLevel;
        var targetY = Mathf.Lerp(_minY, _maxY, fillLevel);

        Tween.LocalPositionY(_liquid, targetY, _fillDuration, _ease)
            .OnComplete(() =>
            {
                if (fillLevel <= 0f)
                    Tween.Alpha(_liquidRenderer, 0f, _fillDuration, _ease)
                        .OnComplete(() => TrySetActiveLiquid(false));
            });
    }

    protected bool TrySetActiveLiquid(bool active)
    {
        if (_liquid.gameObject.activeSelf == active) return false;
        _liquid.gameObject.SetActive(active);
        return true;
    }
}