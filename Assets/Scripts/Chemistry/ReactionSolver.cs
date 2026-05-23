using ChemSimDiploma.Chemistry.Data;
using ChemSimDiploma.Chemistry.ScriptableObjects;
using UnityEngine;

namespace ChemSimDiploma.Chemistry
{
public static class ReactionSolver
{
    private const float MoleEpsilon = 1e-6f;

    public static bool TryApply(ContainerContents contents, ReactionSO reaction, out float extentApplied)
    {
        extentApplied = 0f;
        if (contents == null || reaction == null) return false;

        ReactionTerm[] reactants = reaction.Reactants;
        ReactionTerm[] products = reaction.Products;
        if (reactants == null || reactants.Length == 0 || products == null || products.Length == 0)
            return false;

        float maxExtent = float.MaxValue;
        var matched = new (RuntimeSubstance runtime, int coefficient)[reactants.Length];

        for (int i = 0; i < reactants.Length; i++)
        {
            ReactionTerm term = reactants[i];
            if (term?.Substance == null || term.Coefficient < 1) return false;

            RuntimeSubstance runtime = FindSubstance(contents, term.Substance);
            if (runtime == null) return false;

            maxExtent = Mathf.Min(maxExtent, runtime.Moles / term.Coefficient);
            matched[i] = (runtime, term.Coefficient);
        }

        if (maxExtent < MoleEpsilon) return false;

        extentApplied = maxExtent;

        float tempNumerator = 0f;
        float tempDenom = 0f;

        foreach ((RuntimeSubstance runtime, int coefficient) in matched)
        {
            float consumed = coefficient * extentApplied;
            tempNumerator += consumed * runtime.Temperature;
            tempDenom += consumed;
            runtime.Moles -= consumed;
            if (runtime.Moles < MoleEpsilon)
                runtime.Moles = 0f;
        }

        float productTemperature = tempDenom > MoleEpsilon ? tempNumerator / tempDenom : 25f;

        foreach (ReactionTerm term in products)
        {
            if (term?.Substance == null || term.Coefficient < 1) continue;
            float productMoles = term.Coefficient * extentApplied;
            contents.AddOrMergeSubstance(term.Substance, productMoles, productTemperature);
        }

        contents.RemoveDepletedSubstances();
        return true;
    }

    private static RuntimeSubstance FindSubstance(ContainerContents contents, SubstanceSO substance)
    {
        foreach (RuntimeSubstance runtime in contents.Substances)
        {
            if (runtime?.SubstanceSO == substance)
                return runtime;
        }

        return null;
    }
}
}
