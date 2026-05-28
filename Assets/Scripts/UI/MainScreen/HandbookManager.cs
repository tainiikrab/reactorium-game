using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.UI
{
public class HandbookManager : MonoBehaviour
{
    [SerializeField] private Transform _cardsHolder;

    [Header("Card animation")]
    [SerializeField] private float _showDuration = 0.28f;
    [SerializeField] private float _hideDuration = 0.2f;
    [SerializeField] private Ease _showEase = Ease.OutBack;
    [SerializeField] private Ease _hideEase = Ease.InCubic;
    [SerializeField] [Range(0.5f, 1f)] private float _showScaleFrom = 0.9f;
    [SerializeField] [Range(0.5f, 1f)] private float _hideScaleTo = 0.94f;
    [SerializeField] private float _slideOffset = 36f;

    private HandbookSubstanceBar[] _substanceBars;
    private CanvasGroup[] _cards;
    private readonly List<Vector3> _baseScales = new();
    private int _visibleCardIndex = -1;
    private Sequence _sequence;

    private void Awake()
    {
        _substanceBars = GetComponentsInChildren<HandbookSubstanceBar>();
        _cards = _cardsHolder.GetComponentsInChildren<CanvasGroup>();

        _baseScales.Clear();
        for (int i = 0; i < _cards.Length; i++)
        {
            var rect = _cards[i].transform as RectTransform;
            _baseScales.Add(rect != null ? rect.localScale : Vector3.one);
            SetCardHiddenImmediate(_cards[i], i);
        }

        DisableBarsImmediate();
        _substanceBars[0].Enable();
        ShowCard(0);
    }

    private void OnDestroy()
    {
        StopSequence();
    }

    public void DisableBars()
    {
        foreach (HandbookSubstanceBar bar in _substanceBars)
            bar.Disable();
    }

    public void ShowCard(int index)
    {
        if (!IsValidCardIndex(index))
            return;

        if (index == _visibleCardIndex)
        {
            if (IsCardFullyVisible(index) && !_sequence.isAlive)
                return;

            StopSequence();
            PlayFadeIn(index, 0);
            return;
        }

        StopSequence();

        int direction = _visibleCardIndex >= 0 ? Mathf.Clamp(index - _visibleCardIndex, -1, 1) : 0;

        if (_visibleCardIndex >= 0)
        {
            int previousIndex = _visibleCardIndex;
            _visibleCardIndex = index;
            PlayFadeOutThenIn(previousIndex, index, direction);
            return;
        }

        _visibleCardIndex = index;
        PlayFadeIn(index, direction);
    }

    private void DisableBarsImmediate()
    {
        foreach (HandbookSubstanceBar bar in _substanceBars)
            bar.DisableImmediate();
    }

    private void PlayFadeOutThenIn(int outIndex, int inIndex, int direction)
    {
        _sequence = Sequence.Create()
            .Chain(CreateFadeOutSequence(outIndex, direction))
            .Chain(CreateFadeInSequence(inIndex, direction));
    }

    private void PlayFadeIn(int index, int direction)
    {
        _sequence = CreateFadeInSequence(index, direction);
    }

    private Sequence CreateFadeInSequence(int index, int direction)
    {
        CanvasGroup group = _cards[index];
        Vector3 baseScale = _baseScales[index];
        var rect = group.transform as RectTransform;
        Vector2 shownPos = GetCardShownPosition(rect);

        group.gameObject.SetActive(true);
        group.alpha = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        if (rect != null)
        {
            rect.localScale = baseScale * _showScaleFrom;
            if (direction != 0 && _slideOffset > 0f)
                rect.anchoredPosition = shownPos + new Vector2(direction * _slideOffset, 0f);
        }

        var sequence = Sequence.Create();
        sequence.Group(Tween.Alpha(group, 1f, _showDuration, _showEase));
        if (rect != null)
        {
            sequence.Group(Tween.Scale(rect, baseScale, _showDuration, _showEase));
            if (direction != 0 && _slideOffset > 0f)
                sequence.Group(Tween.UIAnchoredPosition(rect, shownPos, _showDuration, Ease.OutCubic));
        }

        return sequence;
    }

    private Sequence CreateFadeOutSequence(int index, int direction)
    {
        CanvasGroup group = _cards[index];
        Vector3 baseScale = _baseScales[index];
        var rect = group.transform as RectTransform;
        Vector2 shownPos = GetCardShownPosition(rect);

        group.interactable = false;
        group.blocksRaycasts = false;

        var sequence = Sequence.Create();
        sequence.Group(Tween.Alpha(group, 0f, _hideDuration, _hideEase));
        if (rect != null)
        {
            sequence.Group(Tween.Scale(rect, baseScale * _hideScaleTo, _hideDuration, _hideEase));
            if (direction != 0 && _slideOffset > 0f)
            {
                Vector2 exitPos = shownPos - new Vector2(direction * _slideOffset * 0.65f, 0f);
                sequence.Group(Tween.UIAnchoredPosition(rect, exitPos, _hideDuration, Ease.InCubic));
            }
        }

        sequence.ChainCallback(() =>
        {
            SetCardHiddenImmediate(group, index);
            if (_visibleCardIndex == index)
                _visibleCardIndex = -1;
        });
        return sequence;
    }

    private void SetCardHiddenImmediate(CanvasGroup group, int index)
    {
        if (group == null)
            return;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);

        var rect = group.transform as RectTransform;
        if (rect != null && index >= 0 && index < _baseScales.Count)
        {
            rect.localScale = _baseScales[index];
            rect.anchoredPosition = GetCardShownPosition(rect);
        }
    }

    private Vector2 GetCardShownPosition(RectTransform rect)
    {
        if (rect == null)
            return Vector2.zero;

        if (!_shownPositions.TryGetValue(rect, out Vector2 pos))
        {
            pos = rect.anchoredPosition;
            _shownPositions[rect] = pos;
        }

        return pos;
    }

    private readonly Dictionary<RectTransform, Vector2> _shownPositions = new();

    private bool IsCardFullyVisible(int index)
    {
        if (!IsValidCardIndex(index))
            return false;

        CanvasGroup group = _cards[index];
        return group.gameObject.activeSelf && group.alpha > 0.99f;
    }

    private bool IsValidCardIndex(int index) =>
        index >= 0 && index < _cards.Length && _cards[index] != null;

    private void StopSequence()
    {
        if (_sequence.isAlive)
            _sequence.Stop();

        if (_cards == null)
            return;

        foreach (CanvasGroup card in _cards)
        {
            if (card == null)
                continue;

            Tween.StopAll(card);
            if (card.transform is RectTransform rect)
                Tween.StopAll(rect);
        }
    }
}
}
