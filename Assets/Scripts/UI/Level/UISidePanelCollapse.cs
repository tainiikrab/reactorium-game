using ChemSimDiploma.UI.Level;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace ChemSimDiploma.UI
{
[DisallowMultipleComponent]
[ExecuteAlways]
public class UISidePanelCollapse : MonoBehaviour
{
    [SerializeField] private Button _toggleButton;
    [SerializeField] private RectTransform _arrowRect;
    [SerializeField] private GameObject _header;
    [SerializeField] private RectTransform _containerRect;

    [Header("Motion")]
    [SerializeField] private float _duration = 0.32f;
    [SerializeField] private Ease _ease = Ease.OutCubic;
    [SerializeField] private float _collapsedArrowZ = 180f;
    [SerializeField, Range(0.05f, 0.5f)] private float _titleFadePortion = 0.25f;

    private ICollapsableUI[] _collapsableBars;
    private bool _collapsed;
    private CanvasGroup _headerGroup;
    private LayoutElement _headerLayoutElement;
    private bool _headerRespectedLayout = true;
    private Tween _arrowTween;
    private Tween _headerAlphaTween;
    private Tween _headerLayoutRestoreTween;

    private float TitleFadeDuration => _duration * _titleFadePortion;

    public bool IsCollapsed => _collapsed;

    private void Awake()
    {
        _collapsableBars = GetComponentsInChildren<ICollapsableUI>(true);
        CacheHeaderComponents();

        if (_toggleButton != null)
            _toggleButton.onClick.AddListener(Toggle);
    }

    private void OnDestroy()
    {
        if (_toggleButton != null)
            _toggleButton.onClick.RemoveListener(Toggle);

        StopArrowTween();
        StopHeaderTweens();
    }

    [ContextMenu("Toggle")]
    public void Toggle()
    {
        SetCollapsed(!_collapsed, true);
    }

    public void SetCollapsed(bool collapsed, bool animate = true)
    {
        if (_collapsed == collapsed && animate)
            return;

        _collapsed = collapsed;

        AnimateHeader(collapsed, animate);

        for (int i = 0; i < _collapsableBars.Length; i++)
            if (collapsed)
                _collapsableBars[i].Collapse(animate);
            else
                _collapsableBars[i].Open(animate);

        if (_containerRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_containerRect);

        AnimateArrow(collapsed, animate);
    }

    private void CacheHeaderComponents()
    {
        if (_header == null)
            return;

        _headerGroup = _header.GetComponent<CanvasGroup>();
        if (_headerGroup == null)
            _headerGroup = _header.AddComponent<CanvasGroup>();

        _headerLayoutElement = _header.GetComponent<LayoutElement>();
        if (_headerLayoutElement == null)
            _headerLayoutElement = _header.AddComponent<LayoutElement>();

        _headerRespectedLayout = !_headerLayoutElement.ignoreLayout;
    }

    private void AnimateHeader(bool collapsed, bool animate)
    {
        StopHeaderTweens();

        if (_header == null)
            return;

        if (!animate || !Application.isPlaying)
        {
            ApplyHeaderCollapsedState(collapsed);
            return;
        }

        if (collapsed)
            AnimateHeaderCollapse();
        else
            AnimateHeaderOpen();
    }

    private void AnimateHeaderCollapse()
    {
        SetHeaderIgnoresLayout(true);
        _header.SetActive(true);
        _headerGroup.alpha = 1f;

        _headerAlphaTween = Tween.Alpha(_headerGroup, 0f, TitleFadeDuration, _ease)
            .OnComplete(DisableHeader);
    }

    private void AnimateHeaderOpen()
    {
        SetHeaderIgnoresLayout(true);
        EnableHeader();
        _headerGroup.alpha = 0f;

        _headerAlphaTween = Tween.Alpha(_headerGroup, 1f, TitleFadeDuration, _ease);
        _headerLayoutRestoreTween = Tween.Delay(_duration)
            .OnComplete(RestoreHeaderLayout);
    }

    private void ApplyHeaderCollapsedState(bool collapsed)
    {
        if (collapsed)
        {
            SetHeaderIgnoresLayout(true);
            DisableHeader();
        }
        else
        {
            EnableHeader();
            if (_headerGroup != null)
                _headerGroup.alpha = 1f;
            RestoreHeaderLayout();
        }
    }

    private void EnableHeader()
    {
        _header.SetActive(true);
        if (_headerGroup == null)
            return;

        _headerGroup.interactable = true;
        _headerGroup.blocksRaycasts = true;
    }

    private void DisableHeader()
    {
        if (_headerGroup != null)
            _headerGroup.alpha = 0f;

        _header.SetActive(false);
    }

    private void SetHeaderIgnoresLayout(bool ignore)
    {
        if (_headerLayoutElement == null)
            return;

        _headerLayoutElement.ignoreLayout = ignore;
        if (_containerRect != null)
            LayoutRebuilder.MarkLayoutForRebuild(_containerRect);
    }

    private void RestoreHeaderLayout()
    {
        SetHeaderIgnoresLayout(!_headerRespectedLayout);
    }

    private void StopHeaderTweens()
    {
        if (!Application.isPlaying)
            return;

        if (_headerAlphaTween.isAlive)
            _headerAlphaTween.Stop();
        if (_headerLayoutRestoreTween.isAlive)
            _headerLayoutRestoreTween.Stop();
    }

    private void StopArrowTween()
    {
        if (!Application.isPlaying)
            return;

        if (_arrowTween.isAlive)
            _arrowTween.Stop();
    }

    private void AnimateArrow(bool collapsed, bool animate)
    {
        if (_arrowRect == null)
            return;

        float targetZ = collapsed ? _collapsedArrowZ : 0f;
        StopArrowTween();

        if (!animate || !Application.isPlaying)
        {
            Vector3 euler = _arrowRect.localEulerAngles;
            euler.z = targetZ;
            _arrowRect.localEulerAngles = euler;
            return;
        }

        Vector3 from = _arrowRect.localEulerAngles;
        var to = new Vector3(from.x, from.y, targetZ);
        _arrowTween = Tween.LocalEulerAngles(_arrowRect, from, to, _duration, _ease);
    }
}
}
