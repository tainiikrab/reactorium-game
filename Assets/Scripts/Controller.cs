using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour, IGrabber
{
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private float followSpeed = 20f;

    private InputSystem_Actions inputSystem;
    public bool IsDragging { get; private set; }

    private Camera cam;
    public Transform CurrentTarget { get; private set; }
    private Vector3 offset;

    private void Awake()
    {
        cam = Camera.main;

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
        CurrentTarget = null;
    }

    private void Update()
    {
        if (!IsDragging || CurrentTarget == null) return;

        var world = GetPointerWorldPosition();
        var targetPos = world + offset;

        CurrentTarget.position = Vector3.Lerp(
            CurrentTarget.position,
            targetPos,
            followSpeed * Time.deltaTime);
    }

    private void TryBeginDrag()
    {
        var world = GetPointerWorldPosition();

        var hit = Physics2D.Raycast(world, Vector2.zero, 0f, draggableLayer);
        if (!hit) return;
        IsDragging = true;

        CurrentTarget = hit.transform;
        offset = CurrentTarget.position - world;
    }

    private Vector3 GetPointerWorldPosition()
    {
        var screenPos = Pointer.current.position.ReadValue();
        var world = cam.ScreenToWorldPoint(screenPos);
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
    Transform CurrentTarget { get; }
    bool IsDragging { get; }
}