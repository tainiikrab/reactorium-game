/// <summary>
/// Сервис плавного перехода между сценами. Живёт в ProjectContext, доступен через Zenject.
/// </summary>
public interface ISceneTransitionService
{
    /// <summary>
    /// Запускает переход: затемнение, асинхронная загрузка, проявление новой сцены.
    /// </summary>
    void LoadScene(string sceneName);
}
