using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Chemistry.Data;
using UnityEngine;

namespace ChemSimDiploma.Tasks
{
public static class ContainerContentsHelper
{
    private const float MoleEpsilon = 1e-6f;

    public static bool HasLiquid(ContainerContents contents, float minFillLevel)
    {
        return contents != null && contents.CurrentFillLevel >= minFillLevel;
    }

    public static bool HasSubstanceWithMoles(ContainerContents contents, SubstanceSO substance, float minMoles = MoleEpsilon)
    {
        if (contents == null || substance == null) return false;

        foreach (RuntimeSubstance runtime in contents.Substances)
        {
            if (runtime?.SubstanceSO != substance) continue;
            if (runtime.Moles >= minMoles) return true;
        }

        return false;
    }

    public static bool HasAllSubstances(ContainerContents contents, SubstanceSO[] required, float minMoles = MoleEpsilon)
    {
        if (contents == null || required == null || required.Length == 0) return false;

        foreach (SubstanceSO substance in required)
        {
            if (substance == null) return false;
            if (!HasSubstanceWithMoles(contents, substance, minMoles)) return false;
        }

        return true;
    }

    public static bool HasAnySubstance(ContainerContents contents, SubstanceSO[] candidates, float minMoles = MoleEpsilon)
    {
        if (contents == null || candidates == null) return false;

        foreach (SubstanceSO substance in candidates)
        {
            if (substance == null) continue;
            if (HasSubstanceWithMoles(contents, substance, minMoles)) return true;
        }

        return false;
    }

    public static bool HasAcidAndBase(ContainerContents contents, float acidMaxPh, float baseMinPh, float minMoles = MoleEpsilon)
    {
        if (contents == null) return false;

        bool hasAcid = false;
        bool hasBase = false;

        foreach (RuntimeSubstance runtime in contents.Substances)
        {
            if (runtime?.SubstanceSO == null || runtime.Moles < minMoles) continue;
            if (!runtime.IsLiquid) continue;

            float ph = runtime.SubstanceSO.pH;
            if (ph <= acidMaxPh) hasAcid = true;
            if (ph >= baseMinPh) hasBase = true;
        }

        return hasAcid && hasBase;
    }
}
}
