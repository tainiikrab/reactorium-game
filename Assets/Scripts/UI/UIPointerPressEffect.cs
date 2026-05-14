using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ChemSimDiploma.UI
{

/// <summary>
/// UI-фидбек: равномерно чуть уменьшить при зажатии и вернуть скейл при отпускании.
/// </summary>
[DisallowMultipleComponent]
public class UIPointerPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICancelHandler
{
    [Header("Target")]
    [Tooltip("Что масштабировать (пусто — этот transform)")]
    [SerializeField] private Transform scaleTarget;

    [Header("Press")]
    [SerializeField] private float pressedScale = 0.96f;

    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private Ease pressEase = Ease.OutQuad;

    [Header("Release")] 
    [SerializeField] private float releaseDuration = 0.18f;
    [SerializeField] private Ease releaseEase = Ease.OutQuad;

    [Header("Options")]
    [Tooltip("Если курсор ушёл с объекта во время зажатия — сразу вернуть масштаб")]
    [SerializeField] private bool cancelPressOnExit;

    private Transform _scaleTransform;
    private Vector3 _baseScale;

    private Selectable _selectable;

    private bool _pointerDownOnThis;

    private void Awake()
    {
        _scaleTransform = scaleTarget != null ? scaleTarget : transform;
        _baseScale = _scaleTransform.localScale;
        _selectable = GetComponent<Selectable>();
        if (_selectable == null)
            _selectable = GetComponentInParent<Selectable>();
    }

    private void OnDisable()
    {
        _pointerDownOnThis = false;
        if (_scaleTransform != null)
            Tween.StopAll(_scaleTransform);
        RestoreBaseScaleImmediate();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        _pointerDownOnThis = true;

        Vector3 pressed = _baseScale * pressedScale;

        StopActive();
        Tween.Scale(_scaleTransform, pressed, pressDuration, pressEase);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_pointerDownOnThis)
            return;
        _pointerDownOnThis = false;
        PlayReleaseIfNeeded();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_pointerDownOnThis || !cancelPressOnExit)
            return;

        _pointerDownOnThis = false;
        PlayReleaseIfNeeded();
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (!_pointerDownOnThis)
            return;

        _pointerDownOnThis = false;
        PlayReleaseIfNeeded();
    }

    private void PlayReleaseIfNeeded()
    {
        StopActive();
        Tween.Scale(_scaleTransform, _baseScale, releaseDuration, releaseEase);
    }

    private void StopActive()
    {
        if (_scaleTransform != null)
            Tween.StopAll(_scaleTransform);
    }

    private void RestoreBaseScaleImmediate()
    {
        if (_scaleTransform != null)
            _scaleTransform.localScale = _baseScale;
    }

    private bool IsInteractable()
    {
        return _selectable == null || _selectable.IsInteractable();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        pressDuration = Mathf.Max(0.01f, pressDuration);
        releaseDuration = Mathf.Max(0.01f, releaseDuration);
    }
#endif
}
}
