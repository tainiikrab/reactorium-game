using ChemSimDiploma.Chemistry.Data;
using UnityEngine;

namespace ChemSimDiploma.Chemistry.ScriptableObjects
{
[CreateAssetMenu(fileName = "NewReaction", menuName = "Chemistry/Reaction", order = 1)]
public class ReactionSO : ScriptableObject
{
    public ReactionTerm[] Reactants;
    public ReactionTerm[] Products;
    public ReactionConditions Conditions;
}
}
