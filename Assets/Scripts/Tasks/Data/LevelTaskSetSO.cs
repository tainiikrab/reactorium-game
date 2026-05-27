using UnityEngine;

namespace ChemSimDiploma.Tasks.Data
{
[CreateAssetMenu(fileName = "LevelTaskSet", menuName = "Tasks/Level Task Set", order = 0)]
public class LevelTaskSetSO : ScriptableObject
{
    [SerializeField] private LevelTaskEntry[] _tasks;
    [SerializeField] private float _finishPopupDelaySeconds = 2.5f;

    public LevelTaskEntry[] Tasks => _tasks;
    public float FinishPopupDelaySeconds => _finishPopupDelaySeconds;
}
}
