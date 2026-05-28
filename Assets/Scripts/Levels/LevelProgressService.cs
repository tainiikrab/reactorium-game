using UnityEngine;

namespace ChemSimDiploma.Levels
{
public sealed class LevelProgressService : ILevelProgressService
{
    private const string MaxUnlockedLevelKey = "ChemSimDiploma.MaxUnlockedLevel";

    public int MaxUnlockedLevel { get; private set; }

    public LevelProgressService()
    {
        ReloadFromStorage();
    }

    public void ReloadFromStorage()
    {
        MaxUnlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt(MaxUnlockedLevelKey, 1));
    }

    public void CompleteLevel(int levelNumber)
    {
        int nextLevel = Mathf.Max(1, levelNumber + 1);
        if (nextLevel <= MaxUnlockedLevel)
            return;

        MaxUnlockedLevel = nextLevel;
        PlayerPrefs.SetInt(MaxUnlockedLevelKey, MaxUnlockedLevel);
        PlayerPrefs.Save();
    }

    public void ResetProgress()
    {
        MaxUnlockedLevel = 1;
        PlayerPrefs.SetInt(MaxUnlockedLevelKey, MaxUnlockedLevel);
        PlayerPrefs.Save();
    }
}
}
