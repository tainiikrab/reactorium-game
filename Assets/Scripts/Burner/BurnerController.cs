using ChemSimDiploma.Chemistry;
using ChemSimDiploma.SceneObjectController;
using ChemSimDiploma.Tasks.Signals;
using UnityEngine;
using Zenject;

namespace ChemSimDiploma.Burner
{
[RequireComponent(typeof(Draggable))]
public class BurnerController : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject _burnerFire;

    [Header("Heating")]
    [SerializeField] private float _heatingRatePerSecond = 20f;
    [SerializeField] private float _coolingRatePerSecond = 5f;
    [SerializeField] private float _maxTemperature = 80f;
    [SerializeField] private float _minTemperature = 20f;
    [SerializeField] private float _signalDeltaTemperature = 0.5f;

    private Draggable _draggable;
    private SignalBus _signalBus;
    private float _lastSignaledTemperature;
    private BurnerFireAnimator _fireAnimator;

    public bool IsLit { get; private set; }
    public ChemContainer AttachedContainer { get; private set; }

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    private void Awake()
    {
        _draggable = GetComponent<Draggable>();

        if (_burnerFire == null)
        {
            Transform fireTransform = transform.Find("BurnerFire");
            if (fireTransform != null)
                _burnerFire = fireTransform.gameObject;
        }

        if (_burnerFire != null)
        {
            _fireAnimator = _burnerFire.GetComponent<BurnerFireAnimator>();
            if (_fireAnimator == null)
                _fireAnimator = _burnerFire.AddComponent<BurnerFireAnimator>();
        }

        UpdateFireVisual();
    }

    private void Update()
    {
        AttachedContainer = ResolveAttachedContainer();
        if (AttachedContainer == null)
            return;

        if (IsLit)
            ApplyTemperatureDelta(_heatingRatePerSecond * Time.deltaTime);
        else
            ApplyTemperatureDelta(-_coolingRatePerSecond * Time.deltaTime);
    }

    public void ToggleFlame()
    {
        SetLit(!IsLit);
    }

    public void SetLit(bool lit)
    {
        IsLit = lit;
        UpdateFireVisual();
    }

    public void AttachContainer(ChemContainer container)
    {
        AttachedContainer = container;
        _lastSignaledTemperature = container != null ? container.Contents.GetAverageLiquidTemperature() : 0f;
        if (container == null) return;

        _signalBus?.Fire(new ContainerPlacedOnBurnerSignal
        {
            Container = container,
            Burner = this
        });
    }

    private ChemContainer ResolveAttachedContainer()
    {
        if (_draggable.Sender == null)
            return null;

        return _draggable.Sender.Transform.GetComponent<ChemContainer>();
    }

    private void UpdateFireVisual()
    {
        if (_fireAnimator != null)
        {
            _fireAnimator.SetVisible(IsLit);
            return;
        }

        if (_burnerFire != null)
            _burnerFire.SetActive(IsLit);
    }

    private void ApplyTemperatureDelta(float delta)
    {
        ContainerContents contents = AttachedContainer.Contents;
        bool changed = false;

        foreach (var substance in contents.Substances)
        {
            if (substance == null || !substance.IsLiquid) continue;

            float next = Mathf.Clamp(substance.Temperature + delta, _minTemperature, _maxTemperature);
            if (Mathf.Abs(next - substance.Temperature) < 1e-4f) continue;

            substance.Temperature = next;
            changed = true;
        }

        if (!changed) return;

        float avgTemperature = contents.GetAverageLiquidTemperature();
        if (Mathf.Abs(avgTemperature - _lastSignaledTemperature) < _signalDeltaTemperature)
            return;

        _lastSignaledTemperature = avgTemperature;
        _signalBus?.Fire(new ContainerHeatedSignal
        {
            Container = AttachedContainer,
            Temperature = avgTemperature
        });
    }
}
}
