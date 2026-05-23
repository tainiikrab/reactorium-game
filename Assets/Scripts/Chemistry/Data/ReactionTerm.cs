using System;
using UnityEngine;

namespace ChemSimDiploma.Chemistry.Data
{
[Serializable]
public class ReactionTerm
{
    public SubstanceSO Substance;
    [Min(1)] public int Coefficient = 1;
}
}
