using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;

public class ButtonHoverScaleTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform scaleTarget;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1f);
    [SerializeField] private float tweenDuration = 0.18f;
    [SerializeField] private Ease easing = Ease.OutCubic;

    private Vector3 _normalScale;
    private Tween _scaleTween;

    private void Awake()
    {
        if (scaleTarget == null)
            scaleTarget = transform as RectTransform;

        _normalScale = scaleTarget != null ? scaleTarget.localScale : Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleTarget == null)
            return;

        _scaleTween.Stop();
        _scaleTween = Tween.Scale(scaleTarget, hoverScale, tweenDuration, easing);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleTarget == null)
            return;

        _scaleTween.Stop();
        _scaleTween = Tween.Scale(scaleTarget, _normalScale, tweenDuration, easing);
    }
}
