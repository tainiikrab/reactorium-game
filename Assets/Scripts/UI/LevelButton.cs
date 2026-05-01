using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    private TextMeshProUGUI levelNumberLebel;

    public bool IsAvailable { get; set; }

    private void Awake()
    {
        levelNumberLebel = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(bool isAvailable)
    {
        IsAvailable = isAvailable;
        levelNumberLebel.color = IsAvailable ? activeColor : inactiveColor;
    }
}