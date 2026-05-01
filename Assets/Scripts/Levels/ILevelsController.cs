using System;

public interface ILevelsController
{
    event Action<int> OnLevelUnlocked;

    public Level[] Levels { get; }
}