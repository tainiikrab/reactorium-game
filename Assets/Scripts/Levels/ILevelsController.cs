using System;
namespace ChemSimDiploma.Levels
{

public interface ILevelsController
{
    event Action<int> OnLevelUnlocked;

    public Level[] Levels { get; }

    /// <summary>
    /// Загружает сцену уровня.
    /// </summary>
    /// <param name="levelNumber">Номер уровня от 1.</param>
    public void LoadLevel(int levelNumber);
}
}
