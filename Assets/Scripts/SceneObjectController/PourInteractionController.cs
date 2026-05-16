using ChemSimDiploma.Chemistry;
using ChemSimDiploma.UI;
using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.SceneObjectController
{
public class PourInteractionController : MonoBehaviour
{
    [Header("UI")] [SerializeField] private PourSliderView _sliderView;

    [Header("Placement")]
    [Tooltip("World-space horizontal offset from the rightmost container to the slider anchor.")]
    [SerializeField] private float _sliderWorldRightOffset = 1.5f;
    [SerializeField] private float _sliderWorldYOffset = 0f;

    [Header("Tilt")]
    [Tooltip("Additional Z rotation (degrees) applied to the source container at slider = 1.")]
    [SerializeField] private float _maxPourAngle = 60f;
    [SerializeField] private float _rotationSmoothTime = 0.08f;
    [SerializeField] private float _resetRotationDuration = 0.15f;

    private Camera _cam;

    private IDraggable _source;
    private IDraggable _destination;
    private ChemContainer _sourceContainer;
    private ChemContainer _destinationContainer;

    private Quaternion _baseSourceRotation;
    private float _tiltSign = 1f;
    private float _initialMaxPourMl;
    private float _pouredMl;
    private bool _active;

    private float _currentAngle;
    private float _angleVelocity;

    /// <summary>True between <see cref="OnContainersAttached"/> and <see cref="OnInteractionEnded"/>.</summary>
    public bool IsPourActive => _active;

    private void Awake()
    {
        _cam = Camera.main;

        if (_sliderView != null)
        {
            _sliderView.ValueChanged += OnSliderValueChanged;
            _sliderView.Hide();
        }
    }

    private void OnDestroy()
    {
        if (_sliderView != null)
            _sliderView.ValueChanged -= OnSliderValueChanged;
    }

    public void OnContainersAttached(IDraggable source, IDraggable destination)
    {
        if (source == null || destination == null) return;

        ChemContainer src = source.Transform.GetComponent<ChemContainer>();
        ChemContainer dst = destination.Transform.GetComponent<ChemContainer>();
        if (src == null || dst == null) return;

        float maxPour = ContainerContents.GetMaxPourVolumeMl(src.Contents, dst.Contents);

        _source = source;
        _destination = destination;
        _sourceContainer = src;
        _destinationContainer = dst;
        _initialMaxPourMl = maxPour;
        _pouredMl = 0f;
        _baseSourceRotation = source.Transform.rotation;
        _tiltSign = ComputeTiltSign(source.Transform.position, destination.Transform.position);
        _currentAngle = 0f;
        _angleVelocity = 0f;
        _active = true;

        if (_cam == null) _cam = Camera.main;

        if (_sliderView != null)
        {
            if (maxPour <= 0f)
            {
                _sliderView.Hide();
                return;
            }

            _sliderView.Show(_cam, ComputeSliderAnchor());
        }
    }

    public void OnInteractionEnded()
    {
        if (!_active) return;
        _active = false;

        if (_sliderView != null)
            _sliderView.Hide();

        if (_source != null)
            Tween.Rotation(_source.Transform, _baseSourceRotation, _resetRotationDuration);

        _source = null;
        _destination = null;
        _sourceContainer = null;
        _destinationContainer = null;
    }

    private void LateUpdate()
    {
        if (!_active || _source == null) return;

        if (_sliderView != null)
            _sliderView.UpdatePosition(ComputeSliderAnchor());

        float sliderValue = _sliderView != null ? _sliderView.Value : 0f;
        float targetAngle = sliderValue * _maxPourAngle * _tiltSign;

        _currentAngle = Mathf.SmoothDampAngle(
            _currentAngle,
            targetAngle,
            ref _angleVelocity,
            _rotationSmoothTime);

        _source.Transform.rotation = _baseSourceRotation * Quaternion.Euler(0f, 0f, _currentAngle);
    }

    private Vector3 ComputeSliderAnchor()
    {
        Vector3 a = _source.Transform.position;
        Vector3 b = _destination.Transform.position;
        float rightX = Mathf.Max(a.x, b.x);
        float midY = (a.y + b.y) * 0.5f;
        return new Vector3(rightX + _sliderWorldRightOffset, midY + _sliderWorldYOffset, 0f);
    }

    private static float ComputeTiltSign(Vector3 sourcePos, Vector3 destPos)
    {
        float dx = destPos.x - sourcePos.x;
        if (Mathf.Abs(dx) < 1e-4f) return 1f;
        return dx > 0f ? -1f : 1f;
    }

    private void OnSliderValueChanged(float value)
    {
        if (!_active || _sourceContainer == null || _destinationContainer == null) return;
        if (_initialMaxPourMl <= 0f) return;

        float desiredPouredMl = value * _initialMaxPourMl;
        float deltaMl = desiredPouredMl - _pouredMl;
        if (deltaMl <= 0f) return;

        float pouredMl = _sourceContainer.Contents.PourInto(
            _destinationContainer.Contents,
            deltaMl);

        _pouredMl += pouredMl;
    }
}
}
