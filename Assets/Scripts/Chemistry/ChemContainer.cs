using UnityEngine;
using ChemSimDiploma.SceneObjectController;
using Unity.AppUI.UI;
using Draggable = ChemSimDiploma.SceneObjectController.Draggable;

namespace ChemSimDiploma.Chemistry
{
[RequireComponent(typeof(Draggable))]
public class ChemContainer : MonoBehaviour
{
    [SerializeField] private ContainerContents _contents = new();

    public ContainerContents Contents => _contents;
}
}