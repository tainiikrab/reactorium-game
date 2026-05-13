using System;
using UnityEngine;
using Zenject;

public class LevelsController : MonoBehaviour, ILevelsController
{
    [SerializeField] private Level[] _levels;

    private ISceneTransitionService _sceneTransitions;

    public event Action<int> OnLevelUnlocked;

    [Inject]
    private void Initialize(ISceneTransitionService sceneTransitions)
    {
        _sceneTransitions = sceneTransitions;
        _levels[0].IsAvailable = true;
    }

    public void UnlockLevel(int levelNumber)
    {
        _levels[levelNumber - 1].IsAvailable = true;
        OnLevelUnlocked?.Invoke(levelNumber);
    }

    public Level[] Levels => _levels;

    /// <param name="levelNumber">Номер уровня от 1</param>
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > _levels.Length)
        {
            Debug.LogError(
                $"{nameof(LevelsController)}: номер уровня вне диапазона 1..{_levels.Length}: {levelNumber}");
            return;
        }

        int slotIndex = levelNumber - 1;
        SceneReference scene = _levels[slotIndex].Scene;
        if (!scene.IsValid)
        {
            Debug.LogError(
                $"{nameof(LevelsController)}: для уровня {levelNumber} не назначена сцена в массиве {nameof(_levels)}.");
            return;
        }

        _sceneTransitions.LoadScene(scene.SceneName);
    }


#if UNITY_EDITOR
    [ContextMenu("Unlock Second Level")]
    private void UnlockSecondLevel()
    {
        UnlockLevel(2);
    }
#endif
}