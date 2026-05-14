using UnityEngine;

namespace ChemSimDiploma.SceneObjectController
{
public interface IDraggable
{
    Transform Transform { get; }
    bool IsInteracting { get; set; }
    Transform InteractPoint { get; }
    IDraggable Receiver { get; set; }
    IDraggable Sender { get; set; }
    void ToggleHover(bool toggle);
    void ToggleCollider(bool toggle);
}
}