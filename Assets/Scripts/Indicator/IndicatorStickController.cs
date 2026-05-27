using System;
using ChemSimDiploma.Chemistry;
using ChemSimDiploma.SceneObjectController;
using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.Indicator
{
[RequireComponent(typeof(Draggable))]
public class IndicatorStickController : MonoBehaviour, IIndicatorStick
{
    private static readonly Quaternion VerticalRotation = Quaternion.identity;
    private const float MoveSnapDistance = 0.15f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private Transform _dipTip;

    [SerializeField] private Color _dryColor = new(0.99f, 0.78f, 0.34f, 1f);

    [Header("Animation")]
    [SerializeField] private float _emergeDuration = 0.35f;

    [SerializeField] private Ease _emergeEase = Ease.OutCubic;

    [SerializeField] private float _colorChangeDuration = 0.55f;

    [SerializeField] private Ease _colorChangeEase = Ease.InOutSine;

    [SerializeField] private float _emergeStartScale = 0.35f;

    private float _storedPh = 7f;
    private bool _hasBeenDipped;
    private bool _isReturning;
    private IndicatorBoxController _spawnedFromBox;
    private Tween _colorTween;
    private Sequence _returnSequence;
    private Sequence _emergeSequence;

    public float StoredPh => _storedPh;
    public bool HasBeenDipped => _hasBeenDipped;
    public bool IsReturning => _isReturning;
    public Color IndicatorColor => _spriteRenderer != null ? _spriteRenderer.color : _dryColor;
    public Transform DipTip => _dipTip != null ? _dipTip : transform;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (_dipTip == null && _spriteRenderer != null)
            _dipTip = _spriteRenderer.transform;
    }

    private void OnDestroy()
    {
        _colorTween.Stop();
        if (_returnSequence.isAlive)
            _returnSequence.Stop();
        if (_emergeSequence.isAlive)
            _emergeSequence.Stop();

        _spawnedFromBox?.NotifyStickDestroyed(this);
    }

    void IIndicatorStick.ApplyPh(float ph) => ApplyPh(ph, animate: true);

    public void ApplyPh(float ph, bool animate)
    {
        _storedPh = ph;
        _hasBeenDipped = true;
        if (_spriteRenderer == null) return;

        Color target = IndicatorPhColor.ForPh(ph);
        if (!animate)
        {
            SetRendererColorOpaque(target);
            return;
        }

        Color from = _spriteRenderer.color;
        Color targetColor = target;
        _colorTween.Stop();
        _colorTween = Tween.Custom(this, 0f, 1f, _colorChangeDuration, (_, t) =>
        {
            if (_spriteRenderer != null)
            {
                Color c = Color.Lerp(from, targetColor, t);
                c.a = 1f;
                _spriteRenderer.color = c;
            }
        }, _colorChangeEase);
    }

    public void DipInto(ChemContainer container)
    {
        if (container == null) return;

        if (container.Contents.CurrentFillLevel <= 0f)
        {
            Debug.Log("[IndicatorStick] Пустая колба — pH не применяется.", container);
            return;
        }

        ApplyPh(container.Contents.MixturePh, animate: true);
    }

    public void ResetToDryVisual()
    {
        StopVisualTweens();
        ApplyVisualStateImmediate(dipped: false, storedPh: 7f);
    }

    public void ApplyVisualStateImmediate(bool dipped, float storedPh)
    {
        _colorTween.Stop();

        if (dipped)
        {
            _storedPh = storedPh;
            _hasBeenDipped = true;
            SetRendererColorOpaque(IndicatorPhColor.ForPh(storedPh));
            return;
        }

        _storedPh = 7f;
        _hasBeenDipped = false;
        SetRendererColorOpaque(_dryColor);
    }

    public void CopyIndicatorStateFrom(IndicatorStickController source)
    {
        if (source == null || !source.HasBeenDipped)
        {
            ResetToDryVisual();
            return;
        }

        ApplyVisualStateImmediate(dipped: true, storedPh: source.StoredPh);
    }

    public void SetSpawnedFrom(IndicatorBoxController box)
    {
        _spawnedFromBox = box;
    }

    public bool CanReturnTo(IndicatorBoxController box) =>
        box != null && _spawnedFromBox == box && !_isReturning;

    public void BeginReturnAnimation()
    {
        _isReturning = true;
        StopVisualTweens();

        if (!TryGetComponent(out Draggable draggable)) return;

        draggable.enabled = false;
        draggable.ToggleCollider(false);
        draggable.ToggleHover(false);
    }

    public void PlayEmergeFrom(Vector3 fromWorld, Vector3 toWorld, bool preserveIndicatorColor = false)
    {
        StopVisualTweens();

        Transform t = transform;
        t.position = fromWorld;
        t.rotation = VerticalRotation;
        t.localScale = Vector3.one * _emergeStartScale;

        if (!preserveIndicatorColor && _spriteRenderer != null)
            SetRendererColorOpaque(_dryColor);

        _emergeSequence = Sequence.Create()
            .Group(Tween.Position(t, toWorld, _emergeDuration, _emergeEase))
            .Group(Tween.Scale(t, Vector3.one, _emergeDuration, _emergeEase))
            .Group(Tween.Rotation(t, VerticalRotation, _emergeDuration, _emergeEase));
    }

    public void PlayReturnToBox(
        Vector3 targetWorldPosition,
        Quaternion targetWorldRotation,
        float moveDuration,
        float fadeDuration,
        float endScaleFactor,
        Ease moveEase,
        Ease fadeEase,
        Action onStowed)
    {
        CompleteEmerge();
        StopVisualTweens();

        Transform t = transform;
        Sequence seq = Sequence.Create();

        if (Vector3.Distance(t.position, targetWorldPosition) > MoveSnapDistance
            || Quaternion.Angle(t.rotation, targetWorldRotation) > 2f)
        {
            seq.Group(Tween.Position(t, targetWorldPosition, moveDuration, moveEase));
            seq.Group(Tween.Rotation(t, targetWorldRotation, moveDuration, moveEase));
        }

        Vector3 endScale = Vector3.one * endScaleFactor;
        seq.Chain(Tween.Scale(t, endScale, fadeDuration, fadeEase));
        if (_spriteRenderer != null)
            seq.Group(Tween.Alpha(_spriteRenderer, 0f, fadeDuration, fadeEase));

        _returnSequence = seq.OnComplete(onStowed);
    }

    public void EnsureFullOpacity()
    {
        if (_spriteRenderer == null) return;
        Color c = _spriteRenderer.color;
        c.a = 1f;
        _spriteRenderer.color = c;
    }

    public void CompleteEmerge()
    {
        if (_emergeSequence.isAlive)
            _emergeSequence.Stop();

        transform.localScale = Vector3.one;
        transform.rotation = VerticalRotation;
        EnsureFullOpacity();
    }

    private void SetRendererColorOpaque(Color color)
    {
        if (_spriteRenderer == null) return;
        color.a = 1f;
        _spriteRenderer.color = color;
    }

    private void StopVisualTweens()
    {
        _colorTween.Stop();
        if (_returnSequence.isAlive)
            _returnSequence.Stop();
        if (_emergeSequence.isAlive)
            _emergeSequence.Stop();
        Tween.StopAll(transform);
        if (_spriteRenderer != null)
            Tween.StopAll(_spriteRenderer);
    }
}
}
