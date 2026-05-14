using System;
using System.Collections.Generic;
using UnityEngine;
namespace ChemSimDiploma.Chemistry
{

[Serializable]
public class ContainerContents
{
    [SerializeField] private ContainerType _containerType;
    [Range(0, 1)] [SerializeField] private float _currentFillLevel;
    [Range(0, 1)] [SerializeField] private float _maxFillLevel = 1;
    [SerializeField] private float _capacityMl = 1000;

    private List<ISubstance> _substances = new();

    public ContainerType ContainerType => _containerType;
    public float CurrentFillLevel => _currentFillLevel;
    public float MaxFillLevel => _maxFillLevel;
    public float CapacityMl => _capacityMl;

    public event Action<float> OnFillLevelChanged;

    public void SetFillLevel(float value)
    {
        value = Mathf.Clamp(value, 0, _maxFillLevel);
        if (Mathf.Approximately(value, _currentFillLevel)) return;
        _currentFillLevel = value;
        OnFillLevelChanged?.Invoke(_currentFillLevel);
    }

    public float GetVolumeMl() => _currentFillLevel * _capacityMl;
}
}
