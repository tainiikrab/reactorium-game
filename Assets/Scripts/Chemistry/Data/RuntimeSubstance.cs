using System;
using UnityEngine;

namespace ChemSimDiploma.Chemistry.Data
{
[Serializable]
public class RuntimeSubstance
{
    public float Moles;
    public float Temperature;
    public SubstanceSO SubstanceSO;

    public bool IsLiquid =>
        SubstanceSO != null && SubstanceSO.DefaultMatterPhase == MatterPhase.Liquid;

    public float GetMassGrams()
    {
        if (SubstanceSO == null || Moles <= 0f) return 0f;
        return Moles * SubstanceSO.MolarMass;
    }

    public float GetVolumeMl()
    {
        if (!IsLiquid || SubstanceSO.Density <= 0f) return 0f;
        return GetMassGrams() / SubstanceSO.Density;
    }
}
}