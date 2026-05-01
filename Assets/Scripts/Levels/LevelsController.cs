using System;
using UnityEngine;
using Zenject;

public class LevelsController : MonoBehaviour, ILevelsController
{
    [SerializeField] private Level[] _levels;

    public event Action<int> OnLevelUnlocked;

    [Inject]
    private void Initialize()
    {
        _levels[0].IsAvailable = true;
    }

    public void UnlockLevel(int levelNumber)
    {
        _levels[levelNumber - 1].IsAvailable = true;
        OnLevelUnlocked?.Invoke(levelNumber);
    }

    public Level[] Levels => _levels;


#if UNITY_EDITOR
    [ContextMenu("Unlock Second Level")]
    private void UnlockSecondLevel()
    {
        UnlockLevel(2);
    }
#endif
}