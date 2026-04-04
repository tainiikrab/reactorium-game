using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

public class Controller : MonoBehaviour, IGrabber
{
    [Header("Config")] [SerializeField] private float snapDuration = 0.2f;
    [SerializeField] private Ease snapEase = Ease.OutCubic;
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private float followSpeed = 20f;

    [Header("Runtime")] private Camera _cam;
    private InputSystem_Actions _input;

    private IDraggable _current;
    private bool _isDragging;

    private DragService _dragService;
    private HoverService _hoverService;
    private InteractionService _interaction;

    public bool IsDragging => _isDragging;
    public IDraggable CurrentTarget => _current;

    private void Awake()
    {
        _cam = Camera.main;

        _dragService = new DragService(followSpeed);
        _hoverService = new HoverService(draggableLayer, _cam);
        _interaction = new InteractionService(snapDuration, snapEase);

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

        var hoverTarget = _hoverService.Current;

        if (hoverTarget != null)
            _hoverService.ApplyState(_current);
    }

    private void OnDestroy()
    {
        _input.Dispose();
    }

    private void OnHoldStarted(InputAction.CallbackContext _)
    {
        if (!_hoverService.TryGetTarget(out var target, out var offset))
            return;

        _isDragging = true;
        _current = target;

        _dragService.Begin(target, offset);
        _interaction.TryDetach(target);
    }

    private void OnHoldCanceled(InputAction.CallbackContext _)
    {
        _isDragging = false;

        if (_current != null && _hoverService.Current != null)
        {
            _interaction.Attach(_current, _hoverService.Current);
            _hoverService.Clear();
        }

        _current?.ToggleCollider(true);
        _current = null;
    }
}