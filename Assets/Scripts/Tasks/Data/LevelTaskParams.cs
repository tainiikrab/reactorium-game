using System;
using UnityEngine;

namespace ChemSimDiploma.Tasks.Data
{
public enum ContainerSubstanceMatchMode
{
    AllRequired,
    AnyOf,
    AcidAndBase
}

[Serializable]
public struct MixAcidBaseTaskParams
{
    public ContainerSubstanceMatchMode MatchMode;
    public SubstanceSO[] RequiredSubstances;
    public SubstanceSO[] AnyOfSubstances;
    public float AcidMaxPh;
    public float BaseMinPh;
    public float MinFillLevel;

    public static MixAcidBaseTaskParams Default => new()
    {
        MatchMode = ContainerSubstanceMatchMode.AcidAndBase,
        AcidMaxPh = 3f,
        BaseMinPh = 11f,
        MinFillLevel = 0.01f
    };
}

[Serializable]
public struct HasLiquidTaskParams
{
    public float MinFillLevel;

    public static HasLiquidTaskParams Default => new() { MinFillLevel = 0.05f };
}

[Serializable]
public struct IndicatorPhTaskParams
{
    public float MinPh;
    public float MaxPh;

    public static IndicatorPhTaskParams Default => new() { MinPh = 6f, MaxPh = 8f };
}
}
