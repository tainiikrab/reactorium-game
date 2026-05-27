using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.UI.Level
{
public class SubstanceInfoCardHolder : MonoBehaviour
{
    [SerializeField] private List<CanvasGroup> _substanceCards;

    [Header("Animation")]
    [SerializeField] private float _showDuration = 0.25f;
    [SerializeField] private float _hideDuration = 0.2f;
    [SerializeField] private Ease _showEase = Ease.OutCubic;
    [SerializeField] private Ease _hideEase = Ease.InCubic;
    [SerializeField] [Range(0.5f, 1f)] private float _showScaleFrom = 0.92f;
    [SerializeField] [Range(0.5f, 1f)] private float _hideScaleTo = 0.92f;

    [Header("Hide delay")]
    [SerializeField] private float _hideDelay = 0.5f;

    private readonly List<Vector3> _baseScales = new();
    private int _visibleCardIndex = -1;
    private Sequence _sequence;
    private Tween _hideDelayTween;

    private void Awake()
    {
        if (_substanceCards == null)
            _substanceCards = new List<CanvasGroup>();

        if (_substanceCards.Count == 0)
        {
            foreach (Transform child in transform)
            {
                var canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                    _substanceCards.Add(canvasGroup);
            }
        }

        _baseScales.Clear();
        foreach (CanvasGroup card in _substanceCards)
        {
            if (card == null)
            {
                _baseScales.Add(Vector3.one);
                continue;
            }

            var rect = card.transform as RectTransform;
            _baseScales.Add(rect != null ? rect.localScale : Vector3.one);
            SetHiddenImmediate(card, _baseScales.Count - 1);
        }
    }

    private void OnDestroy()
    {
        StopAllTweens();
    }

    public void ShowSubstanceInfoCard(int cardIndex)
    {
        CancelHideDelay();

        if (!IsValidIndex(cardIndex))
            return;

        if (cardIndex == _visibleCardIndex)
        {
            if (IsCardFullyVisible(cardIndex) && !_sequence.isAlive)
                return;

            StopSequence();
            PlayFadeIn(cardIndex);
            return;
        }

        StopSequence();

        if (_visibleCardIndex >= 0)
        {
            int previousIndex = _visibleCardIndex;
            _visibleCardIndex = cardIndex;
            PlayFadeOutThenIn(previousIndex, cardIndex);
            return;
        }

        _visibleCardIndex = cardIndex;
        PlayFadeIn(cardIndex);
    }

    public void Hide()
    {
        CancelHideDelay();

        if (_visibleCardIndex < 0)
            return;

        _hideDelayTween = Tween.Delay(_hideDelay)
            .OnComplete(HideImmediately);
    }

    private void HideImmediately()
    {
        if (_visibleCardIndex < 0)
            return;

        StopSequence();
        PlayFadeOut(_visibleCardIndex);
    }

    private void PlayFadeOutThenIn(int outIndex, int inIndex)
    {
        _sequence = Sequence.Create()
            .Chain(CreateFadeOutSequence(outIndex))
            .Chain(CreateFadeInSequence(inIndex));
    }

    private void PlayFadeIn(int index)
    {
        _sequence = CreateFadeInSequence(index);
    }

    private void PlayFadeOut(int index)
    {
        _sequence = CreateFadeOutSequence(index);
    }

    private Sequence CreateFadeInSequence(int index)
    {
        CanvasGroup group = _substanceCards[index];
        Vector3 baseScale = _baseScales[index];
        var rect = group.transform as RectTransform;

        group.gameObject.SetActive(true);
        group.alpha = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        if (rect != null)
            rect.localScale = baseScale * _showScaleFrom;

        var sequence = Sequence.Create();
        sequence.Group(Tween.Alpha(group, 1f, _showDuration, _showEase));
        if (rect != null)
            sequence.Group(Tween.Scale(rect, baseScale, _showDuration, _showEase));

        return sequence;
    }

    private Sequence CreateFadeOutSequence(int index)
    {
        CanvasGroup group = _substanceCards[index];
        Vector3 baseScale = _baseScales[index];
        var rect = group.transform as RectTransform;

        group.interactable = false;
        group.blocksRaycasts = false;

        var sequence = Sequence.Create();
        sequence.Group(Tween.Alpha(group, 0f, _hideDuration, _hideEase));
        if (rect != null)
            sequence.Group(Tween.Scale(rect, baseScale * _hideScaleTo, _hideDuration, _hideEase));

        sequence.ChainCallback(() =>
        {
            SetHiddenImmediate(group, index);
            if (_visibleCardIndex == index)
                _visibleCardIndex = -1;
        });
        return sequence;
    }

    private void SetHiddenImmediate(CanvasGroup group, int index)
    {
        if (group == null)
            return;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);

        var rect = group.transform as RectTransform;
        if (rect != null && index >= 0 && index < _baseScales.Count)
            rect.localScale = _baseScales[index];
    }

    private bool IsCardFullyVisible(int index)
    {
        if (!IsValidIndex(index))
            return false;

        CanvasGroup group = _substanceCards[index];
        return group.gameObject.activeSelf && group.alpha > 0.99f;
    }

    private bool IsValidIndex(int index) =>
        index >= 0 && index < _substanceCards.Count && _substanceCards[index] != null;

    private void CancelHideDelay()
    {
        if (_hideDelayTween.isAlive)
            _hideDelayTween.Stop();
    }

    private void StopSequence()
    {
        if (_sequence.isAlive)
            _sequence.Stop();
    }

    private void StopAllTweens()
    {
        if (!Application.isPlaying)
            return;

        CancelHideDelay();
        StopSequence();
    }
}
}
