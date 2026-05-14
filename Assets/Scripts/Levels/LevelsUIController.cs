using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using ChemSimDiploma.UI;
namespace ChemSimDiploma.Levels
{

public class LevelsUIController : MonoBehaviour, IDisposable
{
    [SerializeField] private LevelButton[] _levelButtons;
    [SerializeField] private Image[] _levelSegments;

    private ILevelsController _levelsController;

    [Inject]
    private void Construct(ILevelsController levelsController)
    {
        _levelsController = levelsController;
        _levelsController.OnLevelUnlocked += OnLevelUnlockedHandler;
    }

    private void Start()
    {
        BindLevelButtons();
        RefreshLevelsVisuals();
    }

    private void BindLevelButtons()
    {
        for (int i = 0; i < _levelButtons.Length; i++)
            _levelButtons[i].Bind(i + 1, _levelsController.LoadLevel);
    }

    private void OnDestroy()
    {
        if (_levelsController != null)
            _levelsController.OnLevelUnlocked -= OnLevelUnlockedHandler;
    }

    public void Dispose()
    {
        if (_levelsController != null)
            _levelsController.OnLevelUnlocked -= OnLevelUnlockedHandler;
    }

    private void OnLevelUnlockedHandler(int _)
    {
        RefreshLevelsVisuals();
    }

    private void RefreshLevelsVisuals()
    {
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            bool isAvailable = _levelsController.Levels[i].IsAvailable;
            _levelButtons[i].Initialize(isAvailable);
            _levelSegments[i].enabled = isAvailable;
        }

        _levelSegments.Last().enabled = _levelsController.Levels.Last().isFinished;
    }
}
}
