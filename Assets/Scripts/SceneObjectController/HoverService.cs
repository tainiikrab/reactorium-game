using UnityEngine;
using UnityEngine.InputSystem;
using ChemSimDiploma.Chemistry;
namespace ChemSimDiploma.SceneObjectController
{

public class HoverService
{
    private readonly LayerMask _layer;
    private readonly Camera _cam;

    private IDraggable _current;

    public IDraggable Current => _current;

    public HoverService(LayerMask layer, Camera cam)
    {
        _layer = layer;
        _cam = cam;
    }

    public void Update()
    {
        if (TryGetTarget(out var target, out _))
        {
            Set(target);
            return;
        }

        Clear();
    }

    public bool TryGetTarget(out IDraggable target, out Vector3 offset)
    {
        target = null;
        offset = Vector3.zero;

        var world = GetPointerWorldPosition();
        var hit = Physics2D.OverlapPoint(world, _layer);

        if (hit == null) return false;

        if (hit.TryGetComponent(out IDraggable draggable))
        {
            target = draggable;
            offset = target.Transform.position - world;
            return true;
        }

        return false;
    }

    public void ApplyState(IDraggable dragging)
    {
        if (_current == null) return;

        if (_current == dragging) return;

        if (_current.Receiver != null || _current.Sender != null)
        {
            Clear();
            return;
        }

        _current.ToggleHover(true);
    }

    private void Set(IDraggable target)
    {
        if (_current == target) return;

        _current?.ToggleHover(false);
        _current = target;
    }

    public void Clear()
    {
        if (_current == null) return;

        _current.ToggleHover(false);
        _current = null;
    }

    private Vector3 GetPointerWorldPosition()
    {
        var screen = Pointer.current.position.ReadValue();
        var world = _cam.ScreenToWorldPoint(screen);
        world.z = 0;
        return world;
    }
}
}
