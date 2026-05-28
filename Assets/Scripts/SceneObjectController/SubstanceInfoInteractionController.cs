using ChemSimDiploma.Chemistry;
using ChemSimDiploma.UI.Level;
using UnityEngine;

namespace ChemSimDiploma.SceneObjectController
{
public enum SubstanceInfoHideMode
{
    OnRelease,
    UntilNextClick
}

public class SubstanceInfoInteractionController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private SubstanceClickInfo _infoView;

    [Header("Long press")]
    [SerializeField] private float _holdDuration = 0.35f;
    [Tooltip("Small jitter allowed while waiting for the info panel.")]
    [SerializeField] private float _holdJitterScreenPx = 30f;
    [Tooltip("Movement after the panel is shown, or a large swipe before it appears, starts drag.")]
    [SerializeField] private float _dragMovementScreenPx = 12f;
    [SerializeField] private SubstanceInfoHideMode _hideMode = SubstanceInfoHideMode.OnRelease;

    [Header("Dependencies")]
    [SerializeField] private PourInteractionController _pourInteraction;

    private ChemContainer _container;
    private bool _holdActive;
    private bool _infoVisible;
    private bool _persistAfterRelease;
    private float _holdStartTime;
    private Vector2 _holdScreenStart;
    private Vector2 _holdScreenPosition;

    public bool IsHoldActive => _holdActive;

    private void Awake()
    {
        if (_pourInteraction == null)
            _pourInteraction = GetComponent<PourInteractionController>();

        ResolveInfoView();
    }

    private void ResolveInfoView()
    {
        if (_infoView != null)
            return;

        _infoView = FindFirstObjectByType<SubstanceClickInfo>(FindObjectsInactive.Include);
        if (_infoView == null)
            Debug.LogWarning("[SubstanceInfoInteractionController] SubstanceClickInfo view is not assigned.", this);
    }

    public void OnAnyHoldStarted()
    {
        if (_hideMode != SubstanceInfoHideMode.UntilNextClick)
            return;

        if (_persistAfterRelease)
            HidePanel();
    }

    public bool TryBeginHold(ChemContainer container, Vector2 screenPosition, Vector3 pointerWorld)
    {
        ResolveInfoView();

        if (container == null)
            return false;

        if (_pourInteraction != null && _pourInteraction.IsPourActive)
            return false;

        _container = container;
        _holdActive = true;
        _infoVisible = false;
        _persistAfterRelease = false;
        _holdStartTime = Time.time;
        _holdScreenStart = screenPosition;
        _holdScreenPosition = screenPosition;

        return true;
    }

    public SubstanceHoldTickResult Tick(Vector2 screenPosition, Vector3 pointerWorld)
    {
        if (!_holdActive)
            return SubstanceHoldTickResult.Continue;

        _holdScreenPosition = screenPosition;

        if (_pourInteraction != null && _pourInteraction.IsPourActive)
        {
            CancelHold();
            return SubstanceHoldTickResult.Continue;
        }

        float movePx = Vector2.Distance(screenPosition, _holdScreenStart);
        if (ShouldStartDrag(movePx))
        {
            HidePanel();
            _holdActive = false;
            return SubstanceHoldTickResult.RequestDrag;
        }

        if (!_infoVisible && Time.time - _holdStartTime >= _holdDuration)
            TryShowInfo();

        if (_infoVisible)
            _infoView?.UpdatePosition();

        return SubstanceHoldTickResult.Continue;
    }

    public void OnHoldCanceled()
    {
        if (!_holdActive && !_persistAfterRelease)
            return;

        bool wasShowing = _infoVisible;
        _holdActive = false;

        if (_hideMode == SubstanceInfoHideMode.OnRelease || !wasShowing)
        {
            HidePanel();
            return;
        }

        _persistAfterRelease = true;
    }

    public void HideForDrag()
    {
        _holdActive = false;
        _persistAfterRelease = false;
        _infoView?.HideImmediate();
    }

    private bool ShouldStartDrag(float movePx)
    {
        if (_infoVisible)
            return movePx > _dragMovementScreenPx;

        if (Time.time - _holdStartTime < _holdDuration)
            return movePx > _holdJitterScreenPx;

        return movePx > _dragMovementScreenPx;
    }

    private void TryShowInfo()
    {
        ResolveInfoView();

        if (_container == null || _infoView == null)
            return;

        if (!_infoView.TrySetContents(_container.Contents))
            return;

        _infoView.Show(_holdScreenPosition);
        _infoVisible = true;
    }

    private void CancelHold()
    {
        _holdActive = false;
        HidePanel();
    }

    private void HidePanel()
    {
        _infoVisible = false;
        _persistAfterRelease = false;
        _infoView?.Hide();
    }
}

public enum SubstanceHoldTickResult
{
    Continue,
    RequestDrag
}
}
