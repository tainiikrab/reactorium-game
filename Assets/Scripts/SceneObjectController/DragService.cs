using UnityEngine;
using UnityEngine.InputSystem;

public class DragService
{
    private readonly float _followSpeed;
    private Vector3 _offset;

    public DragService(float followSpeed)
    {
        _followSpeed = followSpeed;
    }

    public void Begin(IDraggable target, Vector3 offset)
    {
        _offset = offset;

        target.ToggleCollider(false);
        target.ToggleHover(false);
    }

    public void Update(IDraggable target)
    {
        var world = GetPointerWorldPosition(target.Transform);
        var targetPos = world + _offset;

        target.Transform.position = Vector3.Lerp(
            target.Transform.position,
            targetPos,
            _followSpeed * Time.deltaTime);
    }

    private Vector3 GetPointerWorldPosition(Transform t)
    {
        var screen = Pointer.current.position.ReadValue();
        var cam = Camera.main;
        var world = cam.ScreenToWorldPoint(screen);
        world.z = 0;
        return world;
    }
}