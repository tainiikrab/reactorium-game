using System;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour, IDraggable
{
    public ContainerType ContainerType => _containerType;
    [SerializeField] private ContainerType _containerType;
    public float CurrentFillLevel => _currentFillLevel;
    [Range(0, 1)] [SerializeField] private float _currentFillLevel;

    public float MaxFillLevel => _maxFillLevel;
    [Range(0, 1)] [SerializeField] private float _maxFillLevel = 1;

    public float CapacityMl => _capacityMl;
    [SerializeField] private float _capacityMl = 1000;
    [SerializeField] private GameObject _hoverSelection;
    [field: SerializeField] public Transform InteractPoint { get; private set; }
    public IDraggable InteractionTargetReceiver { get; set; }
    public IDraggable InteractionTargetSender { get; set; }
    public bool IsInteracting { get; set; }

    public event Action<float> OnFillLevelChangedEvent;
    public Transform Transform => transform;

    private List<ISubstance> _substances = new();
    private Collider2D _collider;


#if UNITY_EDITOR
    [Range(0, 1)] [SerializeField] private float _editorFillValue;

    [ContextMenu("Apply Editor Fill Value")]
    private void ApplyEditorFill()
    {
        SetFillLevel(_editorFillValue);
    }
#endif

    public void ToggleHover(bool toggle)
    {
        _hoverSelection.SetActive(toggle);
    }

    public void ToggleCollider(bool toggle)
    {
        _collider.enabled = toggle;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

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
}

public interface IDraggable
{
    void ToggleHover(bool toggle);
    void ToggleCollider(bool toggle);
    Transform Transform { get; }
    bool IsInteracting { get; set; }
    Transform InteractPoint { get; }
    IDraggable InteractionTargetReceiver { get; set; }
    IDraggable InteractionTargetSender { get; set; }
}

public enum ContainerType
{
    Flask,
    Beaker
}