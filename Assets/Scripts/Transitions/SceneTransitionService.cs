using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Полноэкранный fade и асинхронная смена сцены. Живёт в DontDestroyOnLoad, чтобы пережить выгрузку меню.
/// </summary>
public sealed class SceneTransitionService : MonoBehaviour
{
    public static SceneTransitionService Instance;
    private static Sprite _whiteUiSprite;

    [Header("Fade")] [SerializeField] private float fadeOutDuration = 0.42f;
    [SerializeField] private float fadeInDuration = 0.48f;
    [SerializeField] private Ease fadeOutEase = Ease.InOutCubic;
    [SerializeField] private Ease fadeInEase = Ease.OutQuart;
    [SerializeField] private Color fadeColor = new(0.04f, 0.07f, 0.12f, 1f);

    private CanvasGroup _canvasGroup;
    private bool _busy;
    private bool _built;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureBuilt();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Запускает переход: затемнение, загрузка, проявление новой сцены.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (_busy)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"{nameof(SceneTransitionService)}: пустое имя сцены.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        _busy = true;
        _canvasGroup.blocksRaycasts = true;

        Tween.StopAll(_canvasGroup);
        yield return Tween.Alpha(_canvasGroup, 1f, fadeOutDuration, fadeOutEase).ToYieldInstruction();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)}: не удалось начать загрузку «{sceneName}».");
            ReleaseBusy();
            yield break;
        }

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        yield return null;

        Tween.StopAll(_canvasGroup);
        yield return Tween.Alpha(_canvasGroup, 0f, fadeInDuration, fadeInEase).ToYieldInstruction();

        ReleaseBusy();
    }

    private void ReleaseBusy()
    {
        _canvasGroup.blocksRaycasts = false;
        _busy = false;
    }

    private void EnsureBuilt()
    {
        if (_built)
            return;

        _built = true;

        var canvasGo = new GameObject("TransitionCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        _canvasGroup = canvasGo.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        var imgGo = new GameObject("Fade");
        imgGo.transform.SetParent(canvasGo.transform, false);

        var rect = imgGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imgGo.AddComponent<Image>();
        image.sprite = WhiteUiSprite();
        image.color = fadeColor;
        image.raycastTarget = true;
    }

    private static Sprite WhiteUiSprite()
    {
        if (_whiteUiSprite != null)
            return _whiteUiSprite;

        Texture2D tex = Texture2D.whiteTexture;
        _whiteUiSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);
        return _whiteUiSprite;
    }
}