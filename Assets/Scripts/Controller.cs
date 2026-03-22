using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour, IGrabber
{
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
        TryBeginDrag();
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        IsDragging = false;
        CurrentTarget?.OnToggleCollider(true);
        CurrentTarget = null;
    }

    private IDraggable hoverTarget;

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
            if (hoverTarget == null)
            {
                target.OnToggleHover(true);
                hoverTarget = target;
            }

            if (hoverTarget != target)
            {
                hoverTarget.OnToggleHover(false);
                target.OnToggleHover(true);
                hoverTarget = target;
            }

            return;
        }

        if (hoverTarget != null)
        {
            hoverTarget.OnToggleHover(false);
            hoverTarget = null;
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

    private void TryBeginDrag()
    {
        IsDragging = TryFindTarget(out var target, out var offset);
        if (IsDragging)
        {
            CurrentTarget = target;
            _offset = offset;
            target.OnToggleCollider(false);
        }
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