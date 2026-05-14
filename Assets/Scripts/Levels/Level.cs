using System;
namespace ChemSimDiploma.Levels
{

[Serializable]
public class Level
{
    public int Number;
    public bool IsAvailable;
    public bool isFinished;
    public SceneReference Scene;
}
}
