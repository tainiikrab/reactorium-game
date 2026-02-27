using System;
using UnityEngine;

public class Container : MonoBehaviour
{
    public ContainerType ContainerType => _containerType;
    [SerializeField] private ContainerType _containerType;
    public float CurrentFillLevel => _currentFillLevel;
    [Range(0, 1)] [SerializeField] private float _currentFillLevel;

    public float MaxFillLevel => _maxFillLevel;
    [Range(0, 1)] [SerializeField] private float _maxFillLevel = 1;

    public float CapacityMl => _capacityMl;
    [SerializeField] private float _capacityMl = 1000;

    public event Action<float> OnFillLevelChangedEvent;

#if UNITY_EDITOR
    [Range(0, 1)] [SerializeField] private float _editorFillValue;

    [ContextMenu("Apply Editor Fill Value")]
    private void ApplyEditorFill()
    {
        SetFillLevel(_editorFillValue);
    }
#endif

    public void SetFillLevel(float fillLevel)
    {
        fillLevel = Mathf.Clamp(fillLevel, 0, MaxFillLevel);
        if (Mathf.Approximately(fillLevel, _currentFillLevel)) return;
        _currentFillLevel = fillLevel;
        OnFillLevelChangedEvent?.Invoke(CurrentFillLevel);
    }

    public float GetVolumeMl()
    {
        return _currentFillLevel * _capacityMl;
    }
}

public enum ContainerType
{
    Flask,
    Beaker
}