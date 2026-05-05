using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _inactiveColor;
    private TextMeshProUGUI _levelNumberLable;
    private Button _button;
    private UnityAction _boundClick;

    public bool IsAvailable { get; private set; }

    private void Awake()
    {
        _levelNumberLable = GetComponentInChildren<TextMeshProUGUI>();
        _button = GetComponentInChildren<Button>();
    }

    /// <summary>
    /// Связывает кнопку с номером уровня и колбэком выбора.
    /// </summary>
    public void Bind(int levelNumber, Action<int> onLevelSelected)
    {
        if (_button == null)
            Awake();

        if (_boundClick != null)
            _button.onClick.RemoveListener(_boundClick);

        _boundClick = () => onLevelSelected?.Invoke(levelNumber);
        _button.onClick.AddListener(_boundClick);
    }

    private void OnDestroy()
    {
        if (_button != null && _boundClick != null)
            _button.onClick.RemoveListener(_boundClick);
    }

    public void Initialize(bool isAvailable)
    {
        IsAvailable = isAvailable;
        _levelNumberLable.color = IsAvailable ? _activeColor : _inactiveColor;
        _button.interactable = IsAvailable;
    }
}