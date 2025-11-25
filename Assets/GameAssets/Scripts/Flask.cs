using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Flask : MonoBehaviour, IDraggable
{
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotateSpeed = 90f; 
    [SerializeField] private float returnSpeed = 45f;
    [SerializeField] private float maxRotationAngle = 180f;
    [SerializeField] private Transform particleParent;
    private List<Transform> particles;

    private Vector2 targetPos;
    private bool isDragging;
    private bool isRotating;

    private void Awake()
    {
        particles = new List<Transform>(particleParent.GetComponentsInChildren<Transform>());
    }

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

    private Tween rotateTween;
    [SerializeField] private float rotateDuration = 1f;
    [SerializeField] private float returnDuration = 1f;
    public void OnRotateHold()
    {
        if (isRotating) return;

        // particleParent.parent = null;
        
        isRotating = true;
        
        rotateTween?.Kill();
        
        rotateTween = transform.DORotate(new Vector3(0, 0, maxRotationAngle), rotateDuration)
            .SetEase(Ease.OutSine);
    }

    public void OnRotateRelease()
    {
        if (!isRotating) return;
        isRotating = false;
        // particleParent.parent = transform;
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

    }
    
}