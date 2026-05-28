using System;
using UnityEngine;
using Zenject;
using ChemSimDiploma.Transitions;
namespace ChemSimDiploma.Levels
{

public class LevelsController : MonoBehaviour, ILevelsController
{
    [SerializeField] private Level[] _levels;

    private ISceneTransitionService _sceneTransitions;
    private ILevelProgressService _progressService;

    public event Action<int> OnLevelUnlocked;

    [Inject]
    private void Initialize(ISceneTransitionService sceneTransitions, ILevelProgressService progressService)
    {
        _sceneTransitions = sceneTransitions;
        _progressService = progressService;
        ApplyProgress();
    }

    public void UnlockLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > _levels.Length)
        {
            UnityEngine.Debug.LogError(
                $"{nameof(LevelsController)}: номер уровня вне диапазона 1..{_levels.Length}: {levelNumber}");
            return;
        }

        int previousMax = _progressService.MaxUnlockedLevel;
        _progressService.CompleteLevel(levelNumber - 1);
        ApplyProgress();

        if (_progressService.MaxUnlockedLevel > previousMax)
            OnLevelUnlocked?.Invoke(levelNumber);
    }

    public Level[] Levels => _levels;

    /// <param name="levelNumber">Номер уровня от 1</param>
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > _levels.Length)
        {
            UnityEngine.Debug.LogError(
                $"{nameof(LevelsController)}: номер уровня вне диапазона 1..{_levels.Length}: {levelNumber}");
            return;
        }

        int slotIndex = levelNumber - 1;
        SceneReference scene = _levels[slotIndex].Scene;
        if (!scene.IsValid)
        {
            UnityEngine.Debug.LogError(
                $"{nameof(LevelsController)}: для уровня {levelNumber} не назначена сцена в массиве {nameof(_levels)}.");
            return;
        }

        _sceneTransitions.LoadScene(scene.SceneName);
    }

    private void ApplyProgress()
    {
        int maxUnlockedLevel = _progressService.MaxUnlockedLevel;
        for (int i = 0; i < _levels.Length; i++)
            _levels[i].IsAvailable = _levels[i].Number <= maxUnlockedLevel;
    }


#if UNITY_EDITOR
    [ContextMenu("Unlock Second Level")]
    private void UnlockSecondLevel()
    {
        UnlockLevel(2);
    }
#endif
}
}
