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
        _capacityMl = Mathf.Max(1, _capacityMl);

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
            if (substance == null || !substance.IsLiquid) continue;
            total += substance.GetVolumeMl();
        }

        return total;
    }

    public float GetFreeLiquidCapacityMl()
    {
        float maxVolumeMl = _capacityMl * _maxFillLevel;
        return Mathf.Max(0f, maxVolumeMl - GetLiquidVolumeMl());
    }

    public static float GetMaxPourVolumeMl(ContainerContents from, ContainerContents to)
    {
        if (from == null || to == null) return 0f;
        return Mathf.Min(from.GetLiquidVolumeMl(), to.GetFreeLiquidCapacityMl());
    }

    public float PourInto(ContainerContents destination, float volumeMl)
    {
        if (destination == null || volumeMl <= 0f) return 0f;

        float sourceVolumeMl = GetLiquidVolumeMl();
        if (sourceVolumeMl <= 1e-6f) return 0f;

        volumeMl = Mathf.Min(volumeMl, GetMaxPourVolumeMl(this, destination));
        if (volumeMl <= 0f) return 0f;

        float fraction = volumeMl / sourceVolumeMl;
        var transfers = new List<(RuntimeSubstance source, float moles)>();

        foreach (RuntimeSubstance substance in _substances)
        {
            if (substance == null || !substance.IsLiquid) continue;
            if (substance.GetVolumeMl() <= 0f) continue;

            float molesToTransfer = substance.Moles * fraction;
            if (molesToTransfer <= 1e-6f) continue;

            transfers.Add((substance, molesToTransfer));
        }

        foreach ((RuntimeSubstance source, float moles) in transfers)
        {
            source.Moles -= moles;
            if (source.Moles < 1e-6f)
                source.Moles = 0f;

            destination.AddOrMergeSubstance(source.SubstanceSO, moles, source.Temperature);
        }

        _substances.RemoveAll(s => s == null || s.Moles <= 1e-6f);

        RefreshState();
        destination.RefreshState();
        return volumeMl;
    }

    private void AddOrMergeSubstance(SubstanceSO substanceSo, float moles, float temperature)
    {
        if (substanceSo == null || moles <= 0f) return;

        foreach (RuntimeSubstance existing in _substances)
        {
            if (existing?.SubstanceSO != substanceSo) continue;

            float totalMoles = existing.Moles + moles;
            existing.Temperature = (existing.Temperature * existing.Moles + temperature * moles) / totalMoles;
            existing.Moles = totalMoles;
            return;
        }

        _substances.Add(new RuntimeSubstance
        {
            SubstanceSO = substanceSo,
            Moles = moles,
            Temperature = temperature
        });
    }
}
}