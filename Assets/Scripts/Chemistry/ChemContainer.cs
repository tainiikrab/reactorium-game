using ChemSimDiploma.Chemistry.Visuals;
using UnityEngine;
using ChemSimDiploma.SceneObjectController;
using Draggable = ChemSimDiploma.SceneObjectController.Draggable;

namespace ChemSimDiploma.Chemistry
{
[RequireComponent(typeof(Draggable))]
public class ChemContainer : MonoBehaviour
{
    [SerializeField] private ContainerContents _contents = new();
    public ContainerContents Contents => _contents;

    private void OnValidate()
    {
        _contents.RefreshState();

        if (Application.isPlaying) return;

        foreach (FillLevelAnimator animator in GetComponentsInChildren<FillLevelAnimator>(true))
            animator.ApplyImmediateState();
    }
}
}