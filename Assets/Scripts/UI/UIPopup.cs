using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
namespace ChemSimDiploma.UI
{

/// Active root, CanvasGroup alpha 0 at start; container child inactive. Open enables container and plays tweens.
public class UIPopup : MonoBehaviour
{
    private const float OpenScaleFrom = 0.83f;
    private const float CloseScaleMult = 0.89f;
    private const float CloseSlideFactor = 0.55f;

    [SerializeField] private RectTransform _container;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _duration = 0.4f;

    [Tooltip("Vertical travel in anchored pixels: drops in from above on open, continues downward on close.")]
    [SerializeField]
    private float _slide = 68f;

    private Vector2 _containerShownPos;
    private Vector3 _containerShownScale;
    private Sequence _sequence;

    private bool _refsCached;

    private void SyncRestPoseFromLayout()
    {
        _containerShownPos = _container.anchoredPosition;
        _containerShownScale = _container.localScale;
    }

    private void OnDestroy()
    {
        if (_sequence.isAlive)
            _sequence.Stop();
    }

    public void CloseInstant()
    {
        StopTransition();

        if (_container.gameObject.activeSelf)
            SyncRestPoseFromLayout();

        RestoreContainerPose();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        _container.gameObject.SetActive(false);
    }

    public void CloseAnimated()
    {
        if (!_container.gameObject.activeSelf)
            return;

        StopTransition();
        SyncRestPoseFromLayout();

        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        float closeDuration = _duration * 0.68f;
        Vector2 posEnd = _containerShownPos + new Vector2(0f, -_slide * CloseSlideFactor);

        _sequence = Sequence.Create();
        _sequence.Group(Tween.Alpha(_canvasGroup, 0f, closeDuration, Ease.InCubic));
        _sequence.Group(Tween.Scale(_container, _containerShownScale * CloseScaleMult, closeDuration, Ease.InBack));
        _sequence.Group(Tween.UIAnchoredPosition(_container, posEnd, closeDuration, Ease.InQuad));
        _sequence.ChainCallback(() =>
        {
            RestoreContainerPose();
            _canvasGroup.alpha = 0f;
            _container.gameObject.SetActive(false);
        });
    }

    public void Open()
    {
        StopTransition();

        _container.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_container);

        SyncRestPoseFromLayout();

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;

        Vector2 posStart = _containerShownPos + new Vector2(0f, _slide);
        _container.anchoredPosition = posStart;
        _container.localScale = _containerShownScale * OpenScaleFrom;

        _sequence = Sequence.Create();
        float fadeDur = _duration * 0.88f;
        _sequence.Group(Tween.Alpha(_canvasGroup, 1f, fadeDur, Ease.OutQuad));
        _sequence.Group(Tween.Scale(_container, _containerShownScale, _duration, Ease.OutBack));
        _sequence.Group(Tween.UIAnchoredPosition(_container, _containerShownPos, _duration, Ease.OutCubic));
    }

    private void StopTransition()
    {
        if (_sequence.isAlive)
            _sequence.Stop();
    }

    private void RestoreContainerPose()
    {
        _container.anchoredPosition = _containerShownPos;
        _container.localScale = _containerShownScale;
    }
}
}
