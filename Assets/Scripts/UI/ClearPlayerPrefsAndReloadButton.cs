using ChemSimDiploma.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace ChemSimDiploma.UI
{
    /// <summary>
    /// Вешается на UI-кнопку: очищает PlayerPrefs и перезагружает текущую сцену.
    /// </summary>
    public sealed class ClearPlayerPrefsAndReloadButton : MonoBehaviour
    {
        private Button _button;
        private ILevelProgressService _progressService;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        [Inject]
        private void Construct(ILevelProgressService progressService)
        {
            _progressService = progressService;
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(ClearAndReload);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(ClearAndReload);
        }

        public void ClearAndReload()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            _progressService?.ReloadFromStorage();

            Scene active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex);
        }
    }
}
