using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChemSimDiploma.DebugTools
{
public sealed class DebugTools : MonoBehaviour
{
    private static DebugTools _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs cleared");

            return;
        }

        if (!Input.GetKeyDown(KeyCode.R))
            return;

        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
        Debug.Log($"Reloaded scene {active.name}");
    }
}
}