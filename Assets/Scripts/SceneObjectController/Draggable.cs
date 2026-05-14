using UnityEngine;

namespace ChemSimDiploma.SceneObjectController
{
public class Draggable : MonoBehaviour, IDraggable, IFallsToRestWhenFree
{
    [SerializeField] private GameObject _hoverSelection;

    [Header("Table / drop height")] [SerializeField]
    private bool _enableFallToRest = true;

    [SerializeField] private float _minFallHeight = 1.4f;

    [field: SerializeField] public Transform InteractPoint { get; private set; }

    private Collider2D _collider;

    public IDraggable Receiver { get; set; }
    public IDraggable Sender { get; set; }
    public bool IsInteracting { get; set; }
    public Transform Transform => transform;

    public bool EnableFallToRest => _enableFallToRest;
    public float MinFallHeight => _minFallHeight;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    public void ToggleHover(bool toggle)
    {
        _hoverSelection.SetActive(toggle);
    }

    public void ToggleCollider(bool toggle)
    {
        _collider.enabled = toggle;
    }
}
}