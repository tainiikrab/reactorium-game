using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Chemistry.Data;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemSimDiploma.UI.Level
{
public class SubstanceClickInfo : MonoBehaviour
{
    private const float MoleEpsilon = 1e-6f;
    private const string EmptyContentsText = "Пусто";

    [SerializeField] private RectTransform _root;
    [SerializeField] private RectTransform _linesHolder;
    [SerializeField] private TextMeshProUGUI _lineTemplate;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Placement")]
    [Tooltip("Screen-space offset above the click point (pixels).")]
    [SerializeField] private float _screenOffsetY = 80f;
    [Tooltip("Extra rise during the show animation (canvas units).")]
    [SerializeField] private float _showSlideOffsetY = 24f;
    [Tooltip("Drop during the hide animation (canvas units).")]
    [SerializeField] private float _hideSlideOffsetY = 16f;

    [Header("Animation")]
    [SerializeField] private float _showDuration = 0.25f;
    [SerializeField] private float _hideDuration = 0.18f;
    [SerializeField] private Ease _showEase = Ease.OutBack;
    [SerializeField] private Ease _hideEase = Ease.InCubic;
    [SerializeField] [Range(0.5f, 1f)] private float _showScaleFrom = 0.92f;
    [SerializeField] [Range(0.5f, 1f)] private float _hideScaleTo = 0.92f;

    private readonly List<TextMeshProUGUI> _lines = new();
    private RectTransform _canvasRect;
    private Camera _uiCamera;
    private Vector2 _screenAnchor;
    private Vector2 _targetAnchoredPosition;
    private Vector3 _baseScale = Vector3.one;
    private bool _isVisible;
    private Sequence _sequence;
    private Coroutine _showRetryCoroutine;

    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    private void Awake()
    {
        if (_root == null)
            _root = transform as RectTransform;

        ResolveLineRefs();

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (_lineTemplate != null)
            _lineTemplate.gameObject.SetActive(false);

        _baseScale = _root != null ? _root.localScale : Vector3.one;
        ClearLines();
        HideImmediate();
    }

    private void OnDestroy()
    {
        StopShowRetry();
        StopSequence();
    }

    public bool IsVisible => _isVisible;

    public bool Show(Vector2 screenPosition)
    {
        _screenAnchor = screenPosition;
        ResolveCanvas();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        RefreshLayout();

        if (TryBeginShowAnimation())
            return true;

        StopShowRetry();
        _showRetryCoroutine = StartCoroutine(RetryShowNextFrame());
        return true;
    }

    public void UpdatePosition()
    {
        if (!_isVisible || _root == null) return;
        if (!TryResolveTargetPosition(out Vector2 target)) return;

        _targetAnchoredPosition = target;

        if (_sequence.isAlive)
            return;

        _root.anchoredPosition = _targetAnchoredPosition;
    }

    public void Hide()
    {
        if (!gameObject.activeSelf && !_isVisible)
            return;

        _isVisible = false;
        StopShowRetry();
        StopSequence();
        PlayHideAnimation(false);
    }

    public void HideImmediate()
    {
        _isVisible = false;
        StopShowRetry();
        StopSequence();
        SetHiddenImmediate();
    }

    public bool TrySetContents(ContainerContents contents)
    {
        ResolveLineRefs();

        if (_linesHolder == null || _lineTemplate == null || contents == null)
            return false;

        if (!TryBuildContentLines(contents, out List<string> lines))
        {
            ClearLines();
            return false;
        }

        if (lines.Count == 0)
            lines.Add(EmptyContentsText);

        EnsureLineCount(lines.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            _lines[i].text = lines[i];
            _lines[i].ForceMeshUpdate();
            _lines[i].gameObject.SetActive(true);
        }

        if (gameObject.activeInHierarchy)
            RefreshLayout();

        return true;
    }

    public static bool TryBuildContentsText(ContainerContents contents, out string text)
    {
        text = string.Empty;
        if (!TryBuildContentLines(contents, out List<string> lines))
            return false;

        text = lines.Count > 0 ? string.Join('\n', lines) : EmptyContentsText;
        return true;
    }

    public static bool TryBuildContentLines(ContainerContents contents, out List<string> lines)
    {
        lines = new List<string>();
        if (contents == null) return false;

        foreach (RuntimeSubstance substance in contents.Substances)
        {
            if (substance == null || substance.Moles <= MoleEpsilon || substance.SubstanceSO == null)
                continue;

            lines.Add(FormatLine(substance));
        }

        return true;
    }

    private static string FormatLine(RuntimeSubstance substance)
    {
        string label = substance.SubstanceSO.Formula;
        if (string.IsNullOrWhiteSpace(label))
            label = substance.SubstanceSO.Name;

        string moles = substance.Moles.ToString("0.##", RuCulture);
        return $"{label}: {moles} моль";
    }

    private void EnsureLineCount(int count)
    {
        while (_lines.Count < count)
        {
            TextMeshProUGUI line = Instantiate(_lineTemplate, _linesHolder);
            line.gameObject.SetActive(false);
            _lines.Add(line);
        }

        for (int i = 0; i < _lines.Count; i++)
            _lines[i].gameObject.SetActive(i < count);
    }

    private void ClearLines()
    {
        foreach (TextMeshProUGUI line in _lines)
        {
            if (line == null) continue;
            line.text = string.Empty;
            line.gameObject.SetActive(false);
        }
    }

    private bool TryBeginShowAnimation()
    {
        if (!TryResolveTargetPosition(out _targetAnchoredPosition))
            return false;

        StopSequence();

        transform.SetAsLastSibling();
        _isVisible = true;

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0f;
        _root.localScale = _baseScale * _showScaleFrom;
        _root.anchoredPosition = _targetAnchoredPosition - new Vector2(0f, _showSlideOffsetY);

        _sequence = Sequence.Create()
            .Group(Tween.Alpha(_canvasGroup, 1f, _showDuration, _showEase))
            .Group(Tween.Scale(_root, _baseScale, _showDuration, _showEase))
            .Group(Tween.UIAnchoredPosition(_root, _targetAnchoredPosition, _showDuration, Ease.OutCubic));

        return true;
    }

    private IEnumerator RetryShowNextFrame()
    {
        yield return null;

        if (!gameObject.activeSelf)
            yield break;

        RefreshLayout();
        TryBeginShowAnimation();
        _showRetryCoroutine = null;
    }

    private void StopShowRetry()
    {
        if (_showRetryCoroutine == null)
            return;

        StopCoroutine(_showRetryCoroutine);
        _showRetryCoroutine = null;
    }

    private void PlayHideAnimation(bool immediate)
    {
        if (_root == null || _canvasGroup == null)
        {
            SetHiddenImmediate();
            return;
        }

        if (immediate)
        {
            SetHiddenImmediate();
            return;
        }

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        Vector2 exitPos = _root.anchoredPosition - new Vector2(0f, _hideSlideOffsetY);
        Vector3 hideScale = _baseScale * _hideScaleTo;

        _sequence = Sequence.Create()
            .Group(Tween.Alpha(_canvasGroup, 0f, _hideDuration, _hideEase))
            .Group(Tween.Scale(_root, hideScale, _hideDuration, _hideEase))
            .Group(Tween.UIAnchoredPosition(_root, exitPos, _hideDuration, Ease.InCubic))
            .ChainCallback(SetHiddenImmediate);
    }

    private void SetHiddenImmediate()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        if (_root != null)
        {
            _root.localScale = _baseScale;
            _root.anchoredPosition = _targetAnchoredPosition;
        }

        ClearLines();

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    private bool TryResolveTargetPosition(out Vector2 localPosition)
    {
        localPosition = default;
        if (_root == null) return false;

        var parentRect = _root.parent as RectTransform;
        if (parentRect == null) return false;

        ResolveCanvas();

        Vector2 screen = _screenAnchor;
        screen.y += _screenOffsetY;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, screen, _uiCamera, out localPosition);
    }

    private void RefreshLayout()
    {
        if (_root == null) return;

        for (int i = 0; i < _lines.Count; i++)
        {
            TextMeshProUGUI line = _lines[i];
            if (line == null || !line.gameObject.activeSelf)
                continue;

            line.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(line.rectTransform);
        }

        Canvas.ForceUpdateCanvases();

        if (_linesHolder != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_linesHolder);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
        Canvas.ForceUpdateCanvases();
    }

    private void ResolveLineRefs()
    {
        if (_linesHolder == null)
        {
            Transform contents = transform.Find("Contents");
            if (contents != null)
                _linesHolder = contents as RectTransform;
        }

        if (_lineTemplate == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.gameObject.name != "LineTemplate")
                    continue;

                _lineTemplate = text;
                break;
            }
        }

        if (_linesHolder == null || _lineTemplate == null)
            Debug.LogWarning("[SubstanceClickInfo] Lines holder or line template is not assigned.", this);
    }

    private void ResolveCanvas()
    {
        if (_root == null)
            _root = transform as RectTransform;
        if (_root == null) return;

        Canvas canvas = _root.parent != null
            ? _root.parent.GetComponent<Canvas>()
            : null;
        if (canvas == null)
            canvas = _root.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _canvasRect = canvas.transform as RectTransform;
        _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
    }

    private void StopSequence()
    {
        if (_sequence.isAlive)
            _sequence.Stop();
    }
}
}