using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;
namespace ChemSimDiploma.SceneObjectController
{

public class SceneObjectController : MonoBehaviour, IGrabber
{
    [Header("Config")] [SerializeField] private float snapDuration = 0.2f;
    [SerializeField] private Ease snapEase = Ease.OutCubic;
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private float followSpeed = 20f;

    [Header("Perspective drop")] [SerializeField]
    private ObjectPerspectiveScaler perspectiveScaler;

    [Header("Fall after free release")] [SerializeField]
    private float fallSecondsPerUnit = 0.045f;

    [SerializeField] private float fallDurationMin = 0.22f;
    [SerializeField] private float fallDurationMax = 0.55f;
    [SerializeField] private Ease fallMainEase = Ease.OutCubic;
    [SerializeField] private float fallHeightEpsilon = 0.02f;
    [SerializeField] private ReleaseFallBounceSettings fallBounce = new();

    [Header("Runtime")] private Camera _cam;
    private InputSystem_Actions _input;

    private IDraggable _current;
    private bool _isDragging;

    private DragService _dragService;
    private HoverService _hoverService;
    private InteractionService _interaction;
    private ReleaseFallService _releaseFall;

    public bool IsDragging => _isDragging;
    public IDraggable CurrentTarget => _current;

    private void Awake()
    {
        _cam = Camera.main;

        _dragService = new DragService(followSpeed);
        _hoverService = new HoverService(draggableLayer, _cam);
        _interaction = new InteractionService(snapDuration, snapEase);

        if (perspectiveScaler == null)
            perspectiveScaler = GetComponent<ObjectPerspectiveScaler>();

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
        if (!_isDragging || _current == null) return;

        _dragService.Update(_current);
        _hoverService.Update();

        IDraggable hoverTarget = _hoverService.Current;

        if (hoverTarget != null)
            _hoverService.ApplyState(_current);
    }

    private void OnDestroy()
    {
        _input.Dispose();
    }

    private void OnHoldStarted(InputAction.CallbackContext _)
    {
        if (!_hoverService.TryGetTarget(out IDraggable target, out Vector3 offset))
            return;

        _isDragging = true;
        _current = target;

        _releaseFall.OnGrabStarted(target.Transform);
        _dragService.Begin(target, offset);
        _interaction.TryDetach(target);
    }

    private void OnHoldCanceled(InputAction.CallbackContext _)
    {
        _isDragging = false;

        IDraggable released = _current;
        var attached = false;

        if (released != null && _hoverService.Current != null)
        {
            _interaction.Attach(released, _hoverService.Current);
            _hoverService.Clear();
            attached = true;
        }

        released?.ToggleCollider(true);

        if (!attached)
            _releaseFall.TryPlayAfterFreeRelease(released);

        _current = null;
    }
}
}
