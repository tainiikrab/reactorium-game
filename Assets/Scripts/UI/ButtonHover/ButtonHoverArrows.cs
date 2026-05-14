using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;
namespace ChemSimDiploma.UI.ButtonHover
{

public class ButtonHoverArrows : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Arrows")]
    [SerializeField] private bool useArrows = true;
    [SerializeField] private RectTransform leftArrow;
    [SerializeField] private RectTransform rightArrow;

    [SerializeField] private float arrowOffset = 24f;
    [SerializeField] private float tweenDuration = 0.18f;
    [SerializeField] private Ease easing = Ease.OutCubic;

    private CanvasGroup _leftGroup;
    private CanvasGroup _rightGroup;

    private Vector2 _leftShown;
    private Vector2 _rightShown;
    private Vector2 _leftHidden;
    private Vector2 _rightHidden;

    private Tween _leftMoveTween;
    private Tween _rightMoveTween;
    private Tween _leftAlphaTween;
    private Tween _rightAlphaTween;

    private void Awake()
    {
        if (useArrows)
        {
            SetupArrow(leftArrow, out _leftGroup, out _leftShown, out _leftHidden, Vector2.left);
            SetupArrow(rightArrow, out _rightGroup, out _rightShown, out _rightHidden, Vector2.right);
        }
        else
        {
            if (leftArrow != null)
                leftArrow.gameObject.SetActive(false);
            if (rightArrow != null)
                rightArrow.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!useArrows)
            return;

        ShowArrow(leftArrow, _leftGroup, _leftShown, ref _leftMoveTween, ref _leftAlphaTween);
        ShowArrow(rightArrow, _rightGroup, _rightShown, ref _rightMoveTween, ref _rightAlphaTween);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!useArrows)
            return;

        HideArrow(leftArrow, _leftGroup, _leftHidden, ref _leftMoveTween, ref _leftAlphaTween);
        HideArrow(rightArrow, _rightGroup, _rightHidden, ref _rightMoveTween, ref _rightAlphaTween);
    }

    private void SetupArrow(
        RectTransform arrow,
        out CanvasGroup group,
        out Vector2 shown,
        out Vector2 hidden,
        Vector2 direction)
    {
        group = null;
        shown = default;
        hidden = default;

        if (arrow == null)
            return;

        group = arrow.GetComponent<CanvasGroup>();
        if (group == null)
            group = arrow.gameObject.AddComponent<CanvasGroup>();

        shown = arrow.anchoredPosition;
        hidden = shown + direction * arrowOffset;

        arrow.anchoredPosition = hidden;

        group.alpha = 0f;
        group.gameObject.SetActive(false);
    }

    private void ShowArrow(
        RectTransform arrow,
        CanvasGroup group,
        Vector2 targetPos,
        ref Tween moveTween,
        ref Tween alphaTween)
    {
        if (arrow == null || group == null)
            return;

        moveTween.Stop();
        alphaTween.Stop();

        group.gameObject.SetActive(true);

        moveTween = Tween.UIAnchoredPosition(arrow, targetPos, tweenDuration, easing);
        alphaTween = Tween.Alpha(group, 1f, tweenDuration, easing);
    }

    private void HideArrow(
        RectTransform arrow,
        CanvasGroup group,
        Vector2 targetPos,
        ref Tween moveTween,
        ref Tween alphaTween)
    {
        if (arrow == null || group == null)
            return;

        moveTween.Stop();
        alphaTween.Stop();

        moveTween = Tween.UIAnchoredPosition(arrow, targetPos, tweenDuration, easing);
        alphaTween = Tween.Alpha(group, 0f, tweenDuration, easing)
            .OnComplete(() =>
            {
                if (group != null)
                    group.gameObject.SetActive(false);
            });
    }
}
}
