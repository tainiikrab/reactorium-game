using ChemSimDiploma.Burner;
using ChemSimDiploma.Chemistry;
using UnityEngine;

namespace ChemSimDiploma.SceneObjectController
{
public class HeatInteractionController : MonoBehaviour
{
    public void OnContainersAttached(IDraggable source, IDraggable destination)
    {
        if (source == null || destination == null) return;

        if (!source.Transform.TryGetComponent(out ChemContainer container)) return;
        if (!destination.Transform.TryGetComponent(out BurnerController burner)) return;

        burner.AttachContainer(container);
    }
}
}
