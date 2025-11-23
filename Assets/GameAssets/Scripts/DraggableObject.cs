using DG.Tweening;
using UnityEngine;

public class DraggableObject : MonoBehaviour, IDraggable
{
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotateSpeed = 90f; 
    [SerializeField] private float returnSpeed = 45f;
    [SerializeField] private float maxRotationAngle = 180f;

    private Vector2 targetPos;
    private bool isDragging;
    private bool isRotating;

    public void OnGrab(Vector2 pos)
    {
        isDragging = true;
        targetPos = pos;
    }

    public void OnDrag(Vector2 pos)
    {
        if (isRotating)
        {
            return;
        }
        targetPos = pos;
    }

    public void OnRelease()
    {
        isDragging = false;
    }

    // public void OnRotateHold()
    // {
    //     isRotating = true;
    // }
    //
    // public void OnRotateRelease()
    // {
    //     isRotating = false;
    // }
    private Tween rotateTween;
    [SerializeField] private float rotateDuration = 1f;
    [SerializeField] private float returnDuration = 1f;
    public void OnRotateHold()
    {
        if (isRotating) return;
        isRotating = true;

        // Kill any existing rotation tween
        rotateTween?.Kill();

        // Rotate to upside down
        rotateTween = transform.DORotate(new Vector3(0, 0, maxRotationAngle), rotateDuration)
            .SetEase(Ease.OutSine);
    }

    public void OnRotateRelease()
    {
        if (!isRotating) return;
        isRotating = false;
        
        rotateTween?.Kill();
        
        rotateTween = transform.DORotate(Vector3.zero, returnDuration)
            .SetEase(Ease.OutSine);
    }

    void Update()
    {
        if (isDragging)
        {
            Vector2 newPos = Vector2.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            transform.position = newPos;
        }

        // if (isRotating)
        // {
        //     // Rotate until upside down (180 degrees local rotation)
        //     float step = rotateSpeed * Time.deltaTime;
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, maxRotationAngle), step);
        // }
        // else
        // {
        //     // Return to original rotation (0 degrees)
        //     float step = returnSpeed * Time.deltaTime;
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, step);
        // }
    }
    
}