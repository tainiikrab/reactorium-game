namespace ChemSimDiploma.Chemistry.ScriptableObjects
{
using UnityEngine;

[CreateAssetMenu(fileName = "NewReaction", menuName = "Chemistry/Reaction", order = 1)]
public class ReactionSO : ScriptableObject
{
    public SubstanceSO Substance1;
    public SubstanceSO Substance2;
    public SubstanceSO Product;
}
}