using System;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

/// <summary>
/// Релэй для UI-кнопок в сценах уровней: получает сервис переходов через Zenject
/// и предоставляет публичные методы для Button.onClick.
/// Требует SceneContext в сцене (как минимум пустой).
/// </summary>
public sealed class SceneTransitionButton : MonoBehaviour
{
    [Tooltip("Сцена, на которую нужно перейти. Если не задана — используется LoadSceneByName из onClick.")]
    [SerializeField]
    private SceneReference scene;

    private Button _button;
    private ISceneTransitionService _sceneTransitions;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(LoadConfiguredScene);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveAllListeners();
    }


    [Inject]
    private void Construct(ISceneTransitionService sceneTransitions)
    {
        _sceneTransitions = sceneTransitions;
    }

    public void LoadConfiguredScene()
    {
        if (!scene.IsValid)
        {
            Debug.LogError($"{nameof(SceneTransitionButton)}: поле {nameof(scene)} не назначено на {name}.");
            return;
        }

        LoadSceneByName(scene.SceneName);
    }

    public void LoadSceneByName(string sceneName)
    {
        if (_sceneTransitions == null)
        {
            Debug.LogError(
                $"{nameof(SceneTransitionButton)}: сервис не внедрён. Убедитесь, что в сцене есть SceneContext, " +
                "а ProjectContext.prefab лежит в Resources с биндингом SceneTransitionService.");
            return;
        }

        _sceneTransitions.LoadScene(sceneName);
    }
}