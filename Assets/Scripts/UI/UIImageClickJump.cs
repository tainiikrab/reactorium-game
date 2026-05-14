using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ChemSimDiploma.UI
{

/// <summary>
/// Подпрыгивание UI по клику: быстрый взлёт и мягкое приземление с отскоком через PrimeTween
/// </summary>
[RequireComponent(typeof(Image))]
public class UIImageClickJump : MonoBehaviour, IPointerClickHandler
{
    [Header("Jump")] 
    [SerializeField] private float jumpHeight = 28f;
    [SerializeField] private float riseDuration = 0.12f;
    [SerializeField] private float fallDuration = 0.48f;
    [SerializeField] private Ease riseEase = Ease.OutQuad;
    [SerializeField] private Ease fallEase = Ease.OutBounce;

    private RectTransform _rect;
    private Vector2 _baseAnchoredPosition;
    private Sequence _jump;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _baseAnchoredPosition = _rect.anchoredPosition;
    }

    private void OnDisable()
    {
        if (_jump.isAlive)
            _jump.Stop();

        if (_rect != null)
            _rect.anchoredPosition = _baseAnchoredPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_jump.isAlive)
            return;

        Tween.StopAll(_rect);
        _rect.anchoredPosition = _baseAnchoredPosition;

        Vector2 apex = _baseAnchoredPosition + new Vector2(0f, jumpHeight);

        _jump = Sequence.Create()
            .Chain(Tween.UIAnchoredPosition(_rect, apex, riseDuration, riseEase))
            .Chain(Tween.UIAnchoredPosition(_rect, _baseAnchoredPosition, fallDuration, fallEase));
    }
}
}
