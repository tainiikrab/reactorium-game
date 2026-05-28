using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Indicator;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

namespace ChemSimDiploma.SceneObjectController
{
public class SceneObjectController : MonoBehaviour, IGrabber
{
    private enum InteractionPhase
    {
        None,
        SubstanceHold,
        Dragging
    }

    [Header("Config")] [SerializeField] private float snapDuration = 0.2f;
    [SerializeField] private Ease snapEase = Ease.OutCubic;
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private float followSpeed = 20f;

    [Header("Indicator box tap")]
    [SerializeField] private float tapMaxDuration = 0.25f;
    [SerializeField] private float tapMaxMovement = 0.18f;
    [SerializeField] private IndicatorInteractionController _indicatorInteraction;

    [Header("Perspective drop")] [SerializeField]
    private ObjectPerspectiveScaler perspectiveScaler;

    [Header("Fall after free release")] [SerializeField]
    private float fallSecondsPerUnit = 0.045f;

    [SerializeField] private float fallDurationMin = 0.22f;
    [SerializeField] private float fallDurationMax = 0.55f;
    [SerializeField] private Ease fallMainEase = Ease.OutCubic;
    [SerializeField] private float fallHeightEpsilon = 0.02f;
    [SerializeField] private ReleaseFallBounceSettings fallBounce = new();

    [Header("Pour")] [SerializeField] private PourInteractionController _pourInteraction;

    [Header("Substance info")]
    [SerializeField] private SubstanceInfoInteractionController _substanceInfo;

    [Header("Runtime")] private Camera _cam;
    private InputSystem_Actions _input;

    private IDraggable _current;
    private InteractionPhase _phase = InteractionPhase.None;
    private Vector3 _dragOffset;

    private DragService _dragService;
    private HoverService _hoverService;
    private InteractionService _interaction;
    private ReleaseFallService _releaseFall;

    private float _holdStartTime;
    private Vector3 _holdPointerStartWorld;

    public bool IsDragging => _phase == InteractionPhase.Dragging;
    public IDraggable CurrentTarget => _current;

    private void Awake()
    {
        _cam = Camera.main;

        _dragService = new DragService(followSpeed);
        _hoverService = new HoverService(draggableLayer, _cam);
        _interaction = new InteractionService(snapDuration, snapEase);
        _interaction.Attached += OnContainersAttached;

        if (_indicatorInteraction == null)
            _indicatorInteraction = GetComponent<IndicatorInteractionController>();
        if (_indicatorInteraction != null)
            _interaction.Attached += _indicatorInteraction.OnContainersAttached;

        if (perspectiveScaler == null)
            perspectiveScaler = GetComponent<ObjectPerspectiveScaler>();

        if (_pourInteraction == null)
            _pourInteraction = GetComponent<PourInteractionController>();

        if (_substanceInfo == null)
            _substanceInfo = GetComponent<SubstanceInfoInteractionController>();

        fallBounce ??= new ReleaseFallBounceSettings();

        _releaseFall = new ReleaseFallService(
            perspectiveScaler,
            fallSecondsPerUnit,
            fallDurationMin,
            fallDurationMax,
            fallMainEase,
            fallHeightEpsilon,
            fallBounce);

        _input = new InputSystem_Actions();
        _input.Interact.Hold.started += OnHoldStarted;
        _input.Interact.Hold.canceled += OnHoldCanceled;
        _input.Enable();
    }

    private void Update()
    {
        if (_phase == InteractionPhase.SubstanceHold && _current != null)
        {
            if (_substanceInfo == null)
            {
                BeginDrag(_current, _dragOffset);
                return;
            }

            SubstanceHoldTickResult tickResult = _substanceInfo.Tick(ReadPointerScreen(), ReadPointerWorld());
            if (tickResult == SubstanceHoldTickResult.RequestDrag)
            {
                BeginDrag(_current, _dragOffset);
                _dragService.Update(_current);
            }

            return;
        }

        if (_phase != InteractionPhase.Dragging || _current == null) return;

        _dragService.Update(_current);
        _hoverService.Update();

        IDraggable hoverTarget = _hoverService.Current;

        if (hoverTarget != null)
            _hoverService.ApplyState(_current);
    }

    private void OnDestroy()
    {
        if (_interaction != null)
        {
            _interaction.Attached -= OnContainersAttached;
            if (_indicatorInteraction != null)
                _interaction.Attached -= _indicatorInteraction.OnContainersAttached;
        }

        _input.Dispose();
    }

    private void OnContainersAttached(IDraggable source, IDraggable destination)
    {
        _pourInteraction?.OnContainersAttached(source, destination);
    }

    private Vector3 ReadPointerWorld()
    {
        Vector2 screen = Pointer.current.position.ReadValue();
        float depth = -_cam.transform.position.z;
        Vector3 world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        world.z = 0f;
        return world;
    }

    private Vector2 ReadPointerScreen() => Pointer.current.position.ReadValue();

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        _substanceInfo?.OnAnyHoldStarted();

        if (!_hoverService.TryGetTarget(out IDraggable target, out Vector3 offset))
            return;

        _holdStartTime = Time.time;
        _holdPointerStartWorld = ReadPointerWorld();
        _current = target;
        _dragOffset = offset;

        if (TryGetChemContainer(target, out ChemContainer container)
            && _substanceInfo != null
            && _substanceInfo.TryBeginHold(container, ReadPointerScreen(), _holdPointerStartWorld))
        {
            _phase = InteractionPhase.SubstanceHold;
            _substanceInfo.Tick(ReadPointerScreen(), _holdPointerStartWorld);
            return;
        }

        BeginDrag(target, offset);
    }

    private void BeginDrag(IDraggable target, Vector3 offset)
    {
        _phase = InteractionPhase.Dragging;

        if (target.Transform.TryGetComponent(out IndicatorStickController stick))
            stick.CompleteEmerge();

        _pourInteraction?.OnInteractionEnded();
        _substanceInfo?.HideForDrag();

        _releaseFall.OnGrabStarted(target.Transform);
        _dragService.Begin(target, offset);
        _interaction.TryDetach(target);
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        if (_phase == InteractionPhase.SubstanceHold)
        {
            _substanceInfo?.OnHoldCanceled();

            if (IsDraggableAlive(_current))
                _current.ToggleCollider(true);

            _current = null;
            _phase = InteractionPhase.None;
            return;
        }

        _phase = InteractionPhase.None;

        IDraggable released = _current;
        bool attached = false;
        bool skipReleaseFall = false;
        bool returnedToBox = false;

        if (TryGetDraggableComponent(released, out IndicatorBoxController box))
        {
            float duration = Time.time - _holdStartTime;
            float move = Vector3.Distance(ReadPointerWorld(), _holdPointerStartWorld);
            if (duration <= tapMaxDuration && move <= tapMaxMovement && box.TrySpawnStick(out _))
                skipReleaseFall = true;
        }

        if (!skipReleaseFall
            && TryGetDraggableComponent(released, out IndicatorStickController stick)
            && TryGetDraggableComponent(_hoverService.Current, out IndicatorBoxController returnBox)
            && stick.CanReturnTo(returnBox)
            && returnBox.TryReturnStick(stick))
        {
            _hoverService.Clear();
            attached = true;
            skipReleaseFall = true;
            returnedToBox = true;
        }
        else if (!skipReleaseFall
            && IsDraggableAlive(released)
            && IsDraggableAlive(_hoverService.Current)
            && AttachRules.CanAttach(released, _hoverService.Current))
        {
            float attachDuration = AttachRules.GetAttachDuration(
                released, _hoverService.Current, snapDuration);
            Ease attachEase = AttachRules.GetAttachEase(
                released, _hoverService.Current, snapEase);
            _interaction.Attach(released, _hoverService.Current, attachDuration, attachEase);
            _hoverService.Clear();
            attached = true;
        }
        else if (!skipReleaseFall && (_pourInteraction == null || !_pourInteraction.IsPourActive))
        {
            _pourInteraction?.OnInteractionEnded();
        }

        if (IsDraggableAlive(released) && !returnedToBox)
            released.ToggleCollider(true);

        if (!skipReleaseFall && !attached && IsDraggableAlive(released))
            _releaseFall.TryPlayAfterFreeRelease(released);

        if (!attached)
            _hoverService.Clear();

        _current = null;
    }

    private static bool TryGetChemContainer(IDraggable draggable, out ChemContainer container)
    {
        container = null;
        if (!IsDraggableAlive(draggable))
            return false;

        var component = (Component)draggable;
        container = component.GetComponent<ChemContainer>();
        if (container != null)
            return true;

        container = component.GetComponentInParent<ChemContainer>();
        return container != null;
    }

    private static bool IsDraggableAlive(IDraggable draggable) =>
        draggable is Component component && component != null;

    private static bool TryGetDraggableComponent<T>(IDraggable draggable, out T component) where T : Component
    {
        component = null;
        if (!IsDraggableAlive(draggable))
            return false;

        return ((Component)draggable).TryGetComponent(out component);
    }
}
}
