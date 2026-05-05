using System;
using UnityEngine;

/// <summary>
/// Ссылка на сцену для инспектора: хранится GUID ассета и кэш имени файла.
/// В рантайме используется только закэшированное имя.
/// </summary>
[Serializable]
public class SceneReference
{
    [HideInInspector] [SerializeField] private string sceneAssetGuid;
    [SerializeField] private string sceneName;

    public string SceneName => sceneName;

    public bool IsValid => !string.IsNullOrEmpty(sceneName);
}