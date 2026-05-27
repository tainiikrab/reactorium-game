using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemSimDiploma.UI.Level
{
[RequireComponent(typeof(RectTransform))]
public class UITaskBar : MonoBehaviour, ICollapsableUI
{
    [SerializeField] private RectTransform _label;
    [SerializeField] private float _collapsedWidth = 64f;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _bulletPointImage;
    [SerializeField] private LayoutElement _layoutElement;

    [Header("Motion")]
    [SerializeField] private float _duration = 0.32f;
    [SerializeField] private Ease _ease = Ease.OutCubic;
    [SerializeField] [Range(0.05f, 0.5f)] private float _labelFadePortion = 0.25f;

    [Header("Task")]
    [SerializeField] private Color _completedColor;
    private Color _defaultColor;
    private TextMeshProUGUI _labelText;

    private float _expandedWidth;
    private CanvasGroup _labelGroup;
    private LayoutElement _labelLayoutElement;
    private bool _labelRespectedLayout = true;
    private Tween _widthTween;
    private Tween _labelAlphaTween;

    private float LabelFadeDuration => _duration * _labelFadePortion;

    private void Awake()
    {
        if (_label == null)
            return;

        _labelGroup = _label.GetComponent<CanvasGroup>();
        if (_labelGroup == null)
            _labelGroup = _label.gameObject.AddComponent<CanvasGroup>();

        _labelLayoutElement = _label.GetComponent<LayoutElement>();
        if (_labelLayoutElement == null)
            _labelLayoutElement = _label.gameObject.AddComponent<LayoutElement>();

        _labelRespectedLayout = !_labelLayoutElement.ignoreLayout;
        _expandedWidth = _rectTransform.sizeDelta.x;

        _labelText = _label.GetComponent<TextMeshProUGUI>();
        _defaultColor = _labelText.color;
    }

    public void SetCompleted(bool completed)
    {
        if (!EnsureLabelText()) return;
        _labelText.color = completed ? _completedColor : _defaultColor;
        _bulletPointImage.color = completed ? _completedColor : _defaultColor;
    }

    public void SetLabel(string text)
    {
        if (!EnsureLabelText() || string.IsNullOrEmpty(text)) return;
        _labelText.text = text;
    }

    private bool EnsureLabelText()
    {
        if (_labelText != null) return true;

        if (_label == null)
            return false;

        _labelText = _label.GetComponent<TextMeshProUGUI>();
        if (_labelText == null) return false;

        _defaultColor = _labelText.color;
        return true;
    }

    private void OnDestroy()
    {
        StopTweens();
    }

    #region Collapse / Expand

    public void Collapse(bool animate)
    {
        StopTweens();
        SetLabelIgnoresLayout(true);

        float currentWidth = _rectTransform.sizeDelta.x;
        _layoutElement.minHeight = _rectTransform.sizeDelta.y;

        if (!animate || !Application.isPlaying)
        {
            DisableLabel();
            SetWidth(_collapsedWidth);
            return;
        }

        PrepareLabelForFadeOut();

        if (_labelGroup != null)
            _labelAlphaTween = Tween.Alpha(_labelGroup, 0f, LabelFadeDuration, _ease)
                .OnComplete(DisableLabel);
        else
            DisableLabel();

        float fromWidth = currentWidth;
        _widthTween = Tween.Custom(this, fromWidth, _collapsedWidth, _duration, SetWidthFromTween, _ease);
    }

    public void Open(bool animate)
    {
        StopTweens();

        if (!animate || !Application.isPlaying)
        {
            EnableLabel();
            if (_labelGroup != null)
                _labelGroup.alpha = 1f;
            SetWidth(_expandedWidth);
            RestoreLabelLayout();
            return;
        }

        SetLabelIgnoresLayout(true);

        EnableLabel();
        if (_labelGroup != null)
            _labelGroup.alpha = 0f;

        if (_labelGroup != null)
            _labelAlphaTween = Tween.Alpha(_labelGroup, 1f, LabelFadeDuration, _ease);

        float fromWidth = _rectTransform.sizeDelta.x;
        _widthTween = Tween.Custom(this, fromWidth, _expandedWidth, _duration, SetWidthFromTween, _ease)
            .OnComplete(RestoreLabelLayout);
    }

    private void SetWidthFromTween(UITaskBar bar, float width)
    {
        bar.SetWidth(width);
    }

    private void SetLabelIgnoresLayout(bool ignore)
    {
        if (_labelLayoutElement == null)
            return;

        _labelLayoutElement.ignoreLayout = ignore;
        LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
    }

    private void RestoreLabelLayout()
    {
        SetLabelIgnoresLayout(!_labelRespectedLayout);
    }

    private void PrepareLabelForFadeOut()
    {
        if (_label == null)
            return;

        _label.gameObject.SetActive(true);
        if (_labelGroup != null)
            _labelGroup.alpha = 1f;
    }

    private void EnableLabel()
    {
        if (_label == null)
            return;

        _label.gameObject.SetActive(true);
        if (_labelGroup != null)
        {
            _labelGroup.alpha = 1f;
            _labelGroup.interactable = true;
            _labelGroup.blocksRaycasts = true;
        }
    }

    private void DisableLabel()
    {
        if (_label == null)
            return;

        if (_labelGroup != null)
        {
            _labelGroup.alpha = 0f;
            _labelGroup.interactable = false;
            _labelGroup.blocksRaycasts = false;
        }

        _label.gameObject.SetActive(false);
    }

    private void StopTweens()
    {
        if (!Application.isPlaying)
            return;

        if (_widthTween.isAlive)
            _widthTween.Stop();
        if (_labelAlphaTween.isAlive)
            _labelAlphaTween.Stop();
    }

    private void SetWidth(float width)
    {
        Vector2 size = _rectTransform.sizeDelta;
        size.x = width;
        _rectTransform.sizeDelta = size;
    }

    #endregion
}
}