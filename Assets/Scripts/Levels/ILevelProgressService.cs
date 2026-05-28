namespace ChemSimDiploma.Levels
{
public interface ILevelProgressService
{
    int MaxUnlockedLevel { get; }

    void CompleteLevel(int levelNumber);

    void ResetProgress();

    void ReloadFromStorage();
}
}
