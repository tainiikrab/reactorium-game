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

    private float _lowY;
    private float _highY;
    private float _lowScale;
    private float _highScale;
    private bool _rangeValid;
    private bool _scalesValid;

    private readonly HashSet<Transform> _extraScaleTargets = new();

    private void Awake()
    {
        _grabber = GetComponent<IGrabber>();
        RebuildScaleCache();
    }

    private void OnValidate()
    {
        RebuildScaleCache();
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

    private void RebuildScaleCache()
    {
        _lowY = Mathf.Min(minY, maxY);
        _highY = Mathf.Max(minY, maxY);
        _rangeValid = !Mathf.Approximately(_lowY, _highY);

        bool yAscending = minY <= maxY;
        _lowScale = yAscending ? minScale : maxScale;
        _highScale = yAscending ? maxScale : minScale;
        _scalesValid = _lowScale > 0f || _highScale > 0f;
    }

    public void ApplyScaleForWorldY(Transform target)
    {
        if (target == null || !_rangeValid || !_scalesValid) return;

        float t = Mathf.InverseLerp(_lowY, _highY, target.position.y);
        float scale = Mathf.Max(Mathf.Lerp(_lowScale, _highScale, t), 0.01f);
        target.localScale = new Vector3(scale, scale, 1f);
    }
}
}
