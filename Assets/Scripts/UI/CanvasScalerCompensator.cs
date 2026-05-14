using UnityEngine;
using UnityEngine.UI;
namespace ChemSimDiploma.UI
{
    
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class WideAspectCompensation : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;

    [Header("Compensation")]
    [SerializeField] private bool affectWidth = true;
    [SerializeField] private bool affectHeight = true;
    [SerializeField] private bool affectScale;

    private RectTransform _rectTransform;

    private Vector2 _initialSizeDelta;
    private Vector3 _initialScale;

    private bool _initialized;

    private void Awake()
    {
        Initialize();
        Apply();
    }

    private void OnEnable()
    {
        Initialize();
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            Apply();
        }
    }
#endif

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _rectTransform = GetComponent<RectTransform>();

        if (canvasScaler == null)
        {
            canvasScaler = GetComponentInParent<CanvasScaler>();
        }

        _initialSizeDelta = _rectTransform.sizeDelta;
        _initialScale = _rectTransform.localScale;

        _initialized = true;
    }

    private void Apply()
    {
        if (_rectTransform == null || canvasScaler == null)
        {
            return;
        }

        float compensation = GetCompensationFactor();

        Vector2 compensatedSize = _initialSizeDelta;

        if (affectWidth)
        {
            compensatedSize.x = _initialSizeDelta.x * compensation;
        }

        if (affectHeight)
        {
            compensatedSize.y = _initialSizeDelta.y * compensation;
        }

        _rectTransform.sizeDelta = compensatedSize;

        if (affectScale)
        {
            _rectTransform.localScale = _initialScale * compensation;
        }
    }

    private float GetCompensationFactor()
    {
        Vector2 referenceResolution = canvasScaler.referenceResolution;

        float referenceAspect = referenceResolution.x / referenceResolution.y;
        float currentAspect = (float)Screen.width / Screen.height;

        // Narrower or equal -> no compensation
        if (currentAspect <= referenceAspect)
        {
            return 1f;
        }

        // Wider -> compensate
        return referenceAspect / currentAspect;
    }
}
}