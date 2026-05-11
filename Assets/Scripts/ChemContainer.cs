using System;
using System.Collections.Generic;
using UnityEngine;

public class ChemContainer : MonoBehaviour, IDraggable, IFallsToRestWhenFree
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private ContainerType _containerType;
    [Range(0, 1)] [SerializeField] private float _currentFillLevel;
    [Range(0, 1)] [SerializeField] private float _maxFillLevel = 1;
    [SerializeField] private float _capacityMl = 1000;
    [SerializeField] private GameObject _hoverSelection;

    [Header("Table / drop height")] [SerializeField]
    private bool _enableFallToRestHeight = true;

    [SerializeField] private float _minFallHeight = 1.4f;

    private Collider2D _collider;

    private List<ISubstance> _substances = new();
    public ContainerType ContainerType => _containerType;
    public float CurrentFillLevel => _currentFillLevel;

    public float MaxFillLevel => _maxFillLevel;

    public float CapacityMl => _capacityMl;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    [field: SerializeField] public Transform InteractPoint { get; private set; }
    public IDraggable Receiver { get; set; }
    public IDraggable Sender { get; set; }
    public bool IsInteracting { get; set; }
    public Transform Transform => transform;

    public bool EnableFallToRest => _enableFallToRestHeight;
    public float MinFallHeight => _minFallHeight;
    public SpriteRenderer Sprite => _sprite;

    public void ToggleHover(bool toggle)
    {
        _hoverSelection.SetActive(toggle);
    }

    public void ToggleCollider(bool toggle)
    {
        _collider.enabled = toggle;
    }

    public event Action<float> OnFillLevelChangedEvent;

    public void SetFillLevel(float fillLevel)
    {
        fillLevel = Mathf.Clamp(fillLevel, 0, MaxFillLevel);
        if (Mathf.Approximately(fillLevel, _currentFillLevel)) return;
        _currentFillLevel = fillLevel;
        OnFillLevelChangedEvent?.Invoke(CurrentFillLevel);
    }

    public float GetVolumeMl()
    {
        return _currentFillLevel * _capacityMl;
    }


#if UNITY_EDITOR
    [Range(0, 1)] [SerializeField] private float _editorFillValue;

    [ContextMenu("Apply Editor Fill Value")]
    private void ApplyEditorFill()
    {
        SetFillLevel(_editorFillValue);
    }
#endif
}

public interface IDraggable
{
    Transform Transform { get; }
    bool IsInteracting { get; set; }
    Transform InteractPoint { get; }
    IDraggable Receiver { get; set; }
    IDraggable Sender { get; set; }
    void ToggleHover(bool toggle);
    void ToggleCollider(bool toggle);
    SpriteRenderer Sprite { get; }
}

public enum ContainerType
{
    Flask,
    Beaker
}