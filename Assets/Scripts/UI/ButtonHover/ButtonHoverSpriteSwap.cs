using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ChemSimDiploma.UI.ButtonHover
{

public class ButtonHoverSpriteSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprite")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite hoverSprite;

    private Sprite _defaultSprite;

    private void Awake()
    {
        if (targetImage != null)
            _defaultSprite = targetImage.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null && hoverSprite != null)
            targetImage.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.sprite = _defaultSprite;
    }
}
}
