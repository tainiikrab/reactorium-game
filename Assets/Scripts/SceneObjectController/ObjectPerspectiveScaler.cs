using System.Collections.Generic;
using UnityEngine;
namespace ChemSimDiploma.SceneObjectController
{

public class ObjectPerspectiveScaler : MonoBehaviour
{
    private IGrabber _grabber;

    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;

    private readonly HashSet<Transform> _extraScaleTargets = new();

    private void Awake()
    {
        _grabber = GetComponent<IGrabber>();
    }

    private void Update()
    {
        if (_grabber.IsDragging && _grabber.CurrentTarget != null)
            ApplyScaleForWorldY(_grabber.CurrentTarget.Transform);

        if (_extraScaleTargets.Count == 0)
            return;

        _reusableExtraBuffer.Clear();
        foreach (var t in _extraScaleTargets)
        {
            if (t != null)
                _reusableExtraBuffer.Add(t);
        }

        for (var i = 0; i < _reusableExtraBuffer.Count; i++)
            ApplyScaleForWorldY(_reusableExtraBuffer[i]);
    }

    private readonly List<Transform> _reusableExtraBuffer = new();

    public void RegisterExtraScaleTarget(Transform target)
    {
        if (target == null) return;
        _extraScaleTargets.Add(target);
    }

    public void UnregisterExtraScaleTarget(Transform target)
    {
        if (target == null) return;
        _extraScaleTargets.Remove(target);
    }

    public void ApplyScaleForWorldY(Transform target)
    {
        if (target == null) return;
        float t = Mathf.InverseLerp(minY, maxY, target.position.y);
        var scale = Mathf.Lerp(minScale, maxScale, t);
        target.localScale = new Vector3(scale, scale, 1);
    }
}
}
