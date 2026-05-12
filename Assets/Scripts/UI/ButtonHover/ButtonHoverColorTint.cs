using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverColorTint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Target graphics")]
    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text label;

    [Header("Hover colors")]
    [SerializeField] private Color hoverImageColor = Color.white;
    [SerializeField] private Color hoverTextColor = Color.white;

    private Color _defaultImageColor;
    private Color _defaultTextColor;

    private void Awake()
    {
        if (targetImage != null)
            _defaultImageColor = targetImage.color;
        if (label != null)
            _defaultTextColor = label.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = hoverImageColor;
        if (label != null)
            label.color = hoverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = _defaultImageColor;
        if (label != null)
            label.color = _defaultTextColor;
    }
}
