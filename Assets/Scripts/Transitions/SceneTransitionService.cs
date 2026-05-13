using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Полноэкранный fade и асинхронная смена сцены. Живёт на ProjectContext, чтобы пережить выгрузку любой сцены.
/// </summary>
public sealed class SceneTransitionService : MonoBehaviour, ISceneTransitionService
{
    private static Sprite _whiteUiSprite;

    [Header("Fade")] [SerializeField] private float fadeOutDuration = 0.42f;
    [SerializeField] private float fadeInDuration = 0.48f;
    [SerializeField] private Ease fadeOutEase = Ease.InOutCubic;
    [SerializeField] private Ease fadeInEase = Ease.OutQuart;
    [SerializeField] private Color fadeColor = new(0.04f, 0.07f, 0.12f, 1f);

    [Header("Overlay (pattern material)")]
    [Tooltip("Например PatternScroller. Прозрачные области паттерна показывают Fade Color под ним. Если null — только Fade Color.")]
    [SerializeField]
    private Material fadePatternMaterial;

    [Tooltip("Непрозрачность слоя паттерна (0 — только подложка Fade Color, 1 — паттерн без дополнительного ослабления). Умножается с альфой материала/шейдера.")]
    [SerializeField] [Range(0f, 1f)]
    private float patternOpacity = 1f;

    private CanvasGroup _canvasGroup;
    private Material _fadeMaterialInstance;
    private bool _busy;
    private bool _built;

    private void Awake()
    {
        EnsureBuilt();
    }

    private void OnDestroy()
    {
        if (_fadeMaterialInstance != null)
            Destroy(_fadeMaterialInstance);
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

        RectTransform backdropRt = CreateStretchUiChild(canvasGo.transform, "FadeBackdrop");
        var backdropImage = backdropRt.gameObject.AddComponent<Image>();
        backdropImage.sprite = WhiteUiSprite();
        backdropImage.color = fadeColor;
        backdropImage.raycastTarget = false;

        Material template = fadePatternMaterial;
        if (template != null)
        {
            RectTransform patternRt = CreateStretchUiChild(canvasGo.transform, "FadePattern");
            var patternImage = patternRt.gameObject.AddComponent<Image>();
            _fadeMaterialInstance = new Material(template);
            patternImage.material = _fadeMaterialInstance;
            patternImage.sprite = null;
            Color patternTint = Color.white;
            patternTint.a = Mathf.Clamp01(patternOpacity);
            patternImage.color = patternTint;
            patternImage.raycastTarget = true;

            var aspect = patternRt.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            aspect.aspectRatio = 1f;
        }
        else
        {
            backdropImage.raycastTarget = true;
        }
    }

    private static RectTransform CreateStretchUiChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
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