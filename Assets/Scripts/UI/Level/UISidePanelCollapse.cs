using ChemSimDiploma.UI.Level;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace ChemSimDiploma.UI
{
[DisallowMultipleComponent]
public class UISidePanelCollapse : MonoBehaviour
{
    [SerializeField] private Button _toggleButton;
    [SerializeField] private RectTransform _arrowRect;
    [SerializeField] private GameObject _header;
    [SerializeField] private RectTransform _containerRect;

    [Header("Motion")] [SerializeField] private float _duration = 0.32f;
    [SerializeField] private Ease _ease = Ease.OutCubic;
    [SerializeField] private float _collapsedArrowZ = 180f;

    private UISubstanceBar[] _substanceBars;
    private bool _collapsed;
    private Tween _arrowTween;

    public bool IsCollapsed => _collapsed;

    private void Awake()
    {
        _substanceBars = GetComponentsInChildren<UISubstanceBar>(true);

        if (_toggleButton != null)
            _toggleButton.onClick.AddListener(Toggle);
    }

    private void OnDestroy()
    {
        if (_toggleButton != null)
            _toggleButton.onClick.RemoveListener(Toggle);

        StopArrowTween();
    }

    public void Toggle()
    {
        SetCollapsed(!_collapsed, true);
    }

    public void SetCollapsed(bool collapsed, bool animate = true)
    {
        if (_collapsed == collapsed && animate)
            return;

        _collapsed = collapsed;

        if (_header != null)
            _header.SetActive(!collapsed);

        for (int i = 0; i < _substanceBars.Length; i++)
            if (collapsed)
                _substanceBars[i].Collapse();
            else
                _substanceBars[i].Open();

        if (_containerRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_containerRect);

        AnimateArrow(collapsed, animate);
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