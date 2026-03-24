using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

public class Controller : MonoBehaviour, IGrabber
{
    [SerializeField] private float snapDuration = 0.2f;
    [SerializeField] private Ease snapEase = Ease.OutCubic;

    private Tween _moveTween;
    private Tween _rotateTween;
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private float followSpeed = 20f;

    private InputSystem_Actions inputSystem;
    public bool IsDragging { get; private set; }

    private Camera _cam;
    public IDraggable CurrentTarget { get; private set; }
    private Vector3 _offset;

    private void Awake()
    {
        _cam = Camera.main;

        inputSystem = new InputSystem_Actions();
        inputSystem.Interact.Hold.started += OnHoldStarted;
        inputSystem.Interact.Hold.canceled += OnHoldCanceled;


        inputSystem.Enable();
    }

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        IsDragging = TryFindTarget(out var target, out var offset);
        if (IsDragging)
        {
            CurrentTarget = target;

            _rotateTween.Stop();
            _rotateTween = Tween.Rotation(CurrentTarget.Transform, Quaternion.identity, 0.15f);

            _offset = offset;
            target.ToggleCollider(false);
            target.ToggleHover(false);
            if (target.InteractionTargetSender != null)
            {
                _rotateTween.Stop();
                _rotateTween = Tween.Rotation(target.InteractionTargetSender.Transform, Quaternion.identity, 0.15f);
                target.InteractionTargetSender.InteractionTargetReceiver = null;
                target.InteractionTargetReceiver = null;
            }

            else if (target.InteractionTargetReceiver != null)
            {
                target.InteractionTargetReceiver.InteractionTargetSender = null;
                target.InteractionTargetSender = null;
            }
        }
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        IsDragging = false;

        if (_hoverTarget != null)
        {
            HandleInteract();
            _hoverTarget.ToggleHover(false);
        }

        CurrentTarget?.ToggleCollider(true);
        CurrentTarget = null;
    }

    private void HandleInteract()
    {
        var t = CurrentTarget.Transform;
        var p = _hoverTarget.InteractPoint;

        _moveTween.Stop();
        _rotateTween.Stop();

        _moveTween = Tween.Position(t, p.position, snapDuration, snapEase);
        _rotateTween = Tween.Rotation(t, p.rotation, snapDuration, snapEase);

        CurrentTarget.InteractionTargetReceiver = _hoverTarget;
        CurrentTarget.InteractionTargetSender = null;

        _hoverTarget.InteractionTargetSender = CurrentTarget;
        _hoverTarget.InteractionTargetReceiver = null;
    }

    private IDraggable _hoverTarget;

    private void Update()
    {
        if (!IsDragging || CurrentTarget == null) return;

        var world = GetPointerWorldPosition();
        var targetPos = world + _offset;

        CurrentTarget.Transform.position = Vector3.Lerp(
            CurrentTarget.Transform.position,
            targetPos,
            followSpeed * Time.deltaTime);

        if (TryFindTarget(out var target, out var offset))
        {
            if (_hoverTarget == null)
            {
                target.ToggleHover(true);
                _hoverTarget = target;
            }

            if (_hoverTarget != target)
            {
                _hoverTarget.ToggleHover(false);
                target.ToggleHover(true);
                _hoverTarget = target;
            }

            return;
        }

        if (_hoverTarget != null)
        {
            _hoverTarget.ToggleHover(false);
            _hoverTarget = null;
        }
    }

    private bool TryFindTarget(out IDraggable target, out Vector3 offset)
    {
        target = null;
        offset = Vector3.zero;
        var world = GetPointerWorldPosition();

        var hit = Physics2D.Raycast(world, Vector2.zero, 0f, draggableLayer);
        if (!hit) return false;

        if (hit.transform.TryGetComponent(out IDraggable draggable))
        {
            target = draggable;
            offset = target.Transform.position - world;
            return true;
        }

        return false;
    }


    private Vector3 GetPointerWorldPosition()
    {
        var screenPos = Pointer.current.position.ReadValue();
        var world = _cam.ScreenToWorldPoint(screenPos);
        world.z = 0f;
        return world;
    }

    private void OnDestroy()
    {
        inputSystem.Dispose();
    }
}

public interface IGrabber
{
    IDraggable CurrentTarget { get; }
    bool IsDragging { get; }
}