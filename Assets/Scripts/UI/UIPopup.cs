using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

/// Active root, CanvasGroup alpha 0 at start; container child inactive. Open enables container and plays tweens.
public class UIPopup : MonoBehaviour
{
    private const float OpenScaleFrom = 0.83f;
    private const float CloseScaleMult = 0.89f;
    private const float CloseSlideFactor = 0.55f;

    private CanvasGroup _canvasGroup;

    [SerializeField] private RectTransform container;

    [SerializeField] private float duration = 0.4f;
    [Tooltip("Vertical travel in anchored pixels: drops in from above on open, continues downward on close.")]
    [SerializeField] private float slide = 68f;

    private Vector2 _containerShownPos;
    private Vector3 _containerShownScale;
    private Sequence _sequence;

    private bool _refsCached;

    private void EnsureRefs()
    {
        if (_refsCached)
            return;

        _refsCached = true;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (container == null)
            container = transform as RectTransform;
    }

    private void SyncRestPoseFromLayout()
    {
        _containerShownPos = container.anchoredPosition;
        _containerShownScale = container.localScale;
    }

    private void OnDestroy()
    {
        if (_sequence.isAlive)
            _sequence.Stop();
    }

    public void CloseInstant()
    {
        EnsureRefs();
        StopTransition();

        if (container.gameObject.activeSelf)
            SyncRestPoseFromLayout();

        RestoreContainerPose();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        container.gameObject.SetActive(false);
    }

    public void CloseAnimated()
    {
        EnsureRefs();
        if (!container.gameObject.activeSelf)
            return;

        StopTransition();
        SyncRestPoseFromLayout();

        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        float closeDuration = duration * 0.68f;
        Vector2 posEnd = _containerShownPos + new Vector2(0f, -slide * CloseSlideFactor);

        _sequence = Sequence.Create();
        _sequence.Group(Tween.Alpha(_canvasGroup, 0f, closeDuration, Ease.InCubic));
        _sequence.Group(Tween.Scale(container, _containerShownScale * CloseScaleMult, closeDuration, Ease.InBack));
        _sequence.Group(Tween.UIAnchoredPosition(container, posEnd, closeDuration, Ease.InQuad));
        _sequence.ChainCallback(() =>
        {
            RestoreContainerPose();
            _canvasGroup.alpha = 0f;
            container.gameObject.SetActive(false);
        });
    }

    public void Open()
    {
        EnsureRefs();
        StopTransition();

        container.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        SyncRestPoseFromLayout();

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;

        Vector2 posStart = _containerShownPos + new Vector2(0f, slide);
        container.anchoredPosition = posStart;
        container.localScale = _containerShownScale * OpenScaleFrom;

        _sequence = Sequence.Create();
        float fadeDur = duration * 0.88f;
        _sequence.Group(Tween.Alpha(_canvasGroup, 1f, fadeDur, Ease.OutQuad));
        _sequence.Group(Tween.Scale(container, _containerShownScale, duration, Ease.OutBack));
        _sequence.Group(Tween.UIAnchoredPosition(container, _containerShownPos, duration, Ease.OutCubic));
    }

    private void StopTransition()
    {
        if (_sequence.isAlive)
            _sequence.Stop();
    }

    private void RestoreContainerPose()
    {
        container.anchoredPosition = _containerShownPos;
        container.localScale = _containerShownScale;
    }
}
