using UnityEngine;

public interface IDraggable
{
    void OnGrab(Vector2 targetPos);
    void OnDrag(Vector2 targetPos);
    void OnRelease();

    void OnRotateHold();
    void OnRotateRelease();
}