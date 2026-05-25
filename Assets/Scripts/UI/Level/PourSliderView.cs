using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChemSimDiploma.UI
{
public class PourSliderView : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private RectTransform _root;

    private RectTransform _canvasRect;
    private Camera _worldCamera;
    private Camera _uiCamera;

    public event Action<float> ValueChanged;

    public float Value => _slider != null ? _slider.value : 0f;

    private void Awake()
    {
        if (_root == null) _root = transform as RectTransform;
        if (_slider == null) _slider = GetComponent<Slider>();
        if (_slider == null) _slider = GetComponentInChildren<Slider>(true);

        if (_slider != null)
            _slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        if (_slider != null)
            _slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    public void Show(Camera worldCamera, Vector3 worldAnchor)
    {
        _worldCamera = worldCamera;
        ResolveCanvas();

        if (_slider != null)
            _slider.SetValueWithoutNotify(0f);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        UpdatePosition(worldAnchor);
    }

    public void UpdatePosition(Vector3 worldAnchor)
    {
        if (_worldCamera == null || _root == null) return;
        if (_canvasRect == null) ResolveCanvas();
        if (_canvasRect == null) return;

        Vector2 screen = _worldCamera.WorldToScreenPoint(worldAnchor);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screen, _uiCamera, out Vector2 local))
        {
            _root.anchoredPosition = local;
        }
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);

        if (_slider != null)
            _slider.SetValueWithoutNotify(0f);
    }

    private void ResolveCanvas()
    {
        if (_root == null) _root = transform as RectTransform;
        if (_root == null) return;

        Canvas canvas = _root.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Canvas rootCanvas = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
        _canvasRect = rootCanvas.transform as RectTransform;
        _uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;
    }

    private void OnSliderChanged(float value)
    {
        ValueChanged?.Invoke(value);
    }
}
}
