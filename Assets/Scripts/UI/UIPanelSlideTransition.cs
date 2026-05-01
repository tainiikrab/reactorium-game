using System;
using UnityEngine.Events;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelSlideTransition : MonoBehaviour
{
    [Serializable]
    public class PanelPair
    {
        [SerializeField] public CanvasGroup homeCanvasGroup;
        [SerializeField] public CanvasGroup overlayCanvasGroup;
        [SerializeField] public Button openOverlayButton;
        [SerializeField] public Button closeOverlayButton;
    }

    [SerializeField] private PanelPair[] transitionPairs;

    [Header("Motion")] 
    [SerializeField] private float duration = 0.42f;
    [SerializeField] private float slideDistance = 160f;
    [SerializeField] private float incomingScaleFrom = 0.94f;
    [SerializeField] private Ease moveEase = Ease.OutQuint;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private PairRuntime[] _runtimes;
    private UnityAction[] _openActions;
    private UnityAction[] _closeActions;

    private struct PairRuntime
    {
        public Vector2 HomePos;
        public Vector2 OverlayPos;
        public bool ShowingOverlay;
        public Sequence Transition;
    }

    private static RectTransform Rect(CanvasGroup cg)
    {
        return (RectTransform)cg.transform;
    }

    private void Awake()
    {
        _runtimes = new PairRuntime[transitionPairs.Length];
        _openActions = new UnityAction[transitionPairs.Length];
        _closeActions = new UnityAction[transitionPairs.Length];

        for (int i = 0; i < transitionPairs.Length; i++)
        {
            PanelPair p = transitionPairs[i];
            ref PairRuntime r = ref _runtimes[i];

            r.HomePos = Rect(p.homeCanvasGroup).anchoredPosition;
            r.OverlayPos = Rect(p.overlayCanvasGroup).anchoredPosition;

            GameObject homeRoot = p.homeCanvasGroup.gameObject;
            GameObject overlayRoot = p.overlayCanvasGroup.gameObject;
            bool homeOn = homeRoot.activeSelf;
            bool overlayOn = overlayRoot.activeSelf;
            r.ShowingOverlay = homeOn != overlayOn && overlayOn;

            int index = i;
            _openActions[i] = () => CrossFade(index, true);
            _closeActions[i] = () => CrossFade(index, false);
            p.openOverlayButton.onClick.AddListener(_openActions[i]);
            p.closeOverlayButton.onClick.AddListener(_closeActions[i]);

            homeRoot.SetActive(true);
            overlayRoot.SetActive(true);
        }
    }

    private void Start()
    {
        for (int i = 0; i < transitionPairs.Length; i++)
        {
            PanelPair p = transitionPairs[i];
            ref PairRuntime r = ref _runtimes[i];
            Rect(p.homeCanvasGroup).anchoredPosition = r.HomePos;
            Rect(p.homeCanvasGroup).localScale = Vector3.one;
            Rect(p.overlayCanvasGroup).anchoredPosition = r.OverlayPos;
            Rect(p.overlayCanvasGroup).localScale = Vector3.one;
        }

        ApplyIdleLayout();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < transitionPairs.Length; i++)
        {
            PanelPair p = transitionPairs[i];
            p.openOverlayButton.onClick.RemoveListener(_openActions[i]);
            p.closeOverlayButton.onClick.RemoveListener(_closeActions[i]);

            if (_runtimes[i].Transition.isAlive)
                _runtimes[i].Transition.Stop();
        }
    }

    /// <summary>Для UI Event: индекс пары в массиве <see cref="transitionPairs"/>.</summary>
    public void CrossFade(int pairIndex, bool toOverlay)
    {
        if (pairIndex < 0 || pairIndex >= transitionPairs.Length)
            return;

        if (toOverlay)
        {
            for (int j = 0; j < transitionPairs.Length; j++)
            {
                if (j == pairIndex)
                    continue;
                if (_runtimes[j].ShowingOverlay)
                    ForceClosePair(j);
            }

            ApplyIdleLayout();
        }

        PanelPair pair = transitionPairs[pairIndex];
        ref PairRuntime state = ref _runtimes[pairIndex];

        if (toOverlay == state.ShowingOverlay)
            return;

        if (state.Transition.isAlive)
            state.Transition.Stop();

        state.ShowingOverlay = toOverlay;

        RectTransform outgoing = toOverlay ? Rect(pair.homeCanvasGroup) : Rect(pair.overlayCanvasGroup);
        RectTransform incoming = toOverlay ? Rect(pair.overlayCanvasGroup) : Rect(pair.homeCanvasGroup);
        CanvasGroup outgoingCg = toOverlay ? pair.homeCanvasGroup : pair.overlayCanvasGroup;
        CanvasGroup incomingCg = toOverlay ? pair.overlayCanvasGroup : pair.homeCanvasGroup;

        Vector2 outEnd;
        Vector2 inStart;
        if (toOverlay)
        {
            outEnd = state.HomePos + Vector2.left * slideDistance;
            inStart = state.OverlayPos + Vector2.right * slideDistance;
        }
        else
        {
            outEnd = state.OverlayPos + Vector2.right * slideDistance;
            inStart = state.HomePos + Vector2.left * slideDistance;
        }

        incoming.anchoredPosition = inStart;
        incoming.localScale = Vector3.one * incomingScaleFrom;
        incomingCg.alpha = 0f;

        outgoing.anchoredPosition = toOverlay ? state.HomePos : state.OverlayPos;
        outgoing.localScale = Vector3.one;
        outgoingCg.alpha = 1f;

        SetPairRaycastsBlocked(pairIndex, true);

        Vector2 targetHome = toOverlay ? state.OverlayPos : state.HomePos;

        int idx = pairIndex;
        bool opening = toOverlay;

        Tween outMove = Tween.UIAnchoredPosition(outgoing, outEnd, duration, moveEase);
        outMove.OnComplete(() =>
        {
            ref PairRuntime st = ref _runtimes[idx];
            outgoing.anchoredPosition = opening ? st.HomePos : st.OverlayPos;
            outgoing.localScale = Vector3.one;
            outgoingCg.alpha = 0f;
            SetPairRaycastsBlocked(idx, false);
        });

        state.Transition = Sequence.Create();
        state.Transition.Group(outMove);
        state.Transition.Group(Tween.Alpha(outgoingCg, 0f, duration, moveEase));
        state.Transition.Group(Tween.UIAnchoredPosition(incoming, targetHome, duration, moveEase));
        state.Transition.Group(Tween.Alpha(incomingCg, 1f, duration, moveEase));
        state.Transition.Group(Tween.Scale(incoming, Vector3.one, duration, scaleEase));
    }

    private void ForceClosePair(int index)
    {
        ref PairRuntime state = ref _runtimes[index];

        if (state.Transition.isAlive)
            state.Transition.Stop();

        state.ShowingOverlay = false;
        PanelPair p = transitionPairs[index];
        RectTransform overlayRt = Rect(p.overlayCanvasGroup);
        overlayRt.anchoredPosition = state.OverlayPos;
        overlayRt.localScale = Vector3.one;
        p.overlayCanvasGroup.alpha = 0f;
        p.overlayCanvasGroup.blocksRaycasts = false;
    }

    private void ApplyIdleLayout()
    {
        for (int h = 0; h < transitionPairs.Length; h++)
        {
            CanvasGroup homeCg = transitionPairs[h].homeCanvasGroup;
            bool hideHome = false;
            for (int i = 0; i < transitionPairs.Length; i++)
            {
                if (transitionPairs[i].homeCanvasGroup != homeCg)
                    continue;
                if (_runtimes[i].ShowingOverlay)
                {
                    hideHome = true;
                    break;
                }
            }

            homeCg.alpha = hideHome ? 0f : 1f;
            homeCg.blocksRaycasts = !hideHome;
        }

        for (int i = 0; i < transitionPairs.Length; i++)
        {
            PanelPair p = transitionPairs[i];
            ref PairRuntime state = ref _runtimes[i];
            p.overlayCanvasGroup.alpha = state.ShowingOverlay ? 1f : 0f;
            p.overlayCanvasGroup.blocksRaycasts = state.ShowingOverlay;
        }
    }

    private void SetPairRaycastsBlocked(int pairIndex, bool blocked)
    {
        PanelPair p = transitionPairs[pairIndex];
        if (blocked)
        {
            p.homeCanvasGroup.blocksRaycasts = false;
            p.overlayCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            ApplyIdleLayout();
        }
    }
}