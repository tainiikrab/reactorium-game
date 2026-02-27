using System;
using UnityEngine;

public class ObjectPerspectiveScaler : MonoBehaviour
{
    private IGrabber grabber;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;

    private void Awake()
    {
        grabber = GetComponent<IGrabber>();
    }

    private void Update()
    {
        if (grabber.IsDragging)
        {
            var target = grabber.CurrentTarget;
            var scale = Mathf.Lerp(minScale, maxScale, (target.position.y - minY) / (maxY - minY));
            target.localScale = new Vector3(scale, scale, 1);
        }
    }
}