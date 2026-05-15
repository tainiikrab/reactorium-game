using System;
using System.Collections.Generic;
using ChemSimDiploma.Chemistry.Data;
using UnityEngine;

namespace ChemSimDiploma.Chemistry
{
[Serializable]
public class ContainerContents
{
    [SerializeField] private ContainerType _containerType;
    [Range(0, 1)] [SerializeField] private float _currentFillLevel;
    [SerializeField] private Color _currentColor;
    [Range(0, 1)] [SerializeField] private float _maxFillLevel = 1;
    [SerializeField] private float _capacityMl = 1000;
    [SerializeField] private List<RuntimeSubstance> _substances = new();


    public List<RuntimeSubstance> Substances => _substances;

    public ContainerType ContainerType => _containerType;
    public float CurrentFillLevel => _currentFillLevel;
    public Color CurrentColor => _currentColor;
    public float MaxFillLevel => _maxFillLevel;
    public float CapacityMl => _capacityMl;

    public event Action<float> OnFillLevelChanged;
    public event Action<Color> OnColorChanged;

    public void RefreshState()
    {
        float totalLiquidVolumeMl = 0f;
        float mixedR = 0f;
        float mixedG = 0f;
        float mixedB = 0f;
        float mixedA = 0f;

        foreach (RuntimeSubstance substance in _substances)
        {
            if (substance == null || !substance.IsLiquid) continue;

            float volumeMl = substance.GetVolumeMl();
            if (volumeMl <= 0f) continue;

            totalLiquidVolumeMl += volumeMl;

            Color color = substance.SubstanceSO.Color;
            mixedR += color.r * volumeMl;
            mixedG += color.g * volumeMl;
            mixedB += color.b * volumeMl;
            mixedA += color.a * volumeMl;
        }

        float fillLevel = _capacityMl > 0f ? totalLiquidVolumeMl / _capacityMl : 0f;
        if (fillLevel > _maxFillLevel)
            UnityEngine.Debug.Log(
                $"[ContainerContents] Overfill: {totalLiquidVolumeMl:F1} ml in {_capacityMl:F1} ml capacity " +
                $"(fill {fillLevel:P0}, clamped to {_maxFillLevel:P0}).",
                null);

        SetFillLevel(fillLevel);

        Color mixedColor = totalLiquidVolumeMl > 0f
            ? new Color(
                mixedR / totalLiquidVolumeMl,
                mixedG / totalLiquidVolumeMl,
                mixedB / totalLiquidVolumeMl,
                mixedA / totalLiquidVolumeMl)
            : Color.clear;

        SetColor(mixedColor);

        OnFillLevelChanged?.Invoke(_currentFillLevel);
        OnColorChanged?.Invoke(_currentColor);
    }

    private void SetFillLevel(float value)
    {
        value = Mathf.Clamp(value, 0, _maxFillLevel);
        _currentFillLevel = value;
    }

    public void SetColor(Color value)
    {
        _currentColor = value;
    }

    public float GetVolumeMl()
    {
        return _currentFillLevel * _capacityMl;
    }

    public float GetLiquidVolumeMl()
    {
        float total = 0f;
        foreach (RuntimeSubstance substance in _substances)
        {
            if (substance == null) continue;
            total += substance.GetVolumeMl();
        }

        return total;
    }
}
}