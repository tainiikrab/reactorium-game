using UnityEngine;
using UnityEngine.InputSystem;
namespace ChemSimDiploma.SceneObjectController
{

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
        Vector3 world = GetPointerWorldPosition(target.Transform);
        Vector3 targetPos = world + _offset;

        targetPos = ClampToScreen(targetPos, target.Transform);

        target.Transform.position = Vector3.Lerp(
            target.Transform.position,
            targetPos,
            _followSpeed * Time.deltaTime);
    }

    private Vector3 GetPointerWorldPosition(Transform t)
    {
        Vector2 screen = Pointer.current.position.ReadValue();
        Camera cam = Camera.main;
        Vector3 world = cam.ScreenToWorldPoint(screen);
        world.z = 0;
        return world;
    }

    private Vector3 ClampToScreen(Vector3 pos, Transform t)
    {
        Camera cam = Camera.main;
        float depth = t.position.z - cam.transform.position.z;
        Vector3 min = cam.ScreenToWorldPoint(new Vector3(0f, 150f, depth));
        Vector3 max = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);
        return pos;
    }
}
}
