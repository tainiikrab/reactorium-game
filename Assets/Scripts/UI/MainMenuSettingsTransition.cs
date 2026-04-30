using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Переключение экранов главного меню и настроек на одном canvas.
/// </summary>
public class MainMenuSettingsTransition : MonoBehaviour
{
    [Header("Panels")] [SerializeField] private RectTransform mainMenuPanel;
    [SerializeField] private RectTransform settingsPanel;

    [Header("Canvas groups")] [SerializeField]
    private CanvasGroup mainMenuCanvasGroup;

    [SerializeField] private CanvasGroup settingsCanvasGroup;

    [Header("Navigation")] [SerializeField]
    private Button openSettingsButton;

    [SerializeField] private Button closeSettingsButton;

    [Header("Motion")] [SerializeField] private float duration = 0.42f;
    [SerializeField] private float slideDistance = 160f;
    [SerializeField] private float incomingScaleFrom = 0.94f;
    [SerializeField] private Ease moveEase = Ease.OutQuint;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private Vector2 _mainHome;
    private Vector2 _settingsHome;

    private bool _showingSettings;
    private Sequence _transition;

    private void Awake()
    {
        _mainHome = mainMenuPanel.anchoredPosition;
        _settingsHome = settingsPanel.anchoredPosition;

        openSettingsButton.onClick.AddListener(GoToSettings);
        closeSettingsButton.onClick.AddListener(GoToMainMenu);

        GameObject mainRoot = mainMenuPanel.gameObject;
        GameObject settingsRoot = settingsPanel.gameObject;
        bool mainOn = mainRoot.activeSelf;
        bool settingsOn = settingsRoot.activeSelf;
        _showingSettings = mainOn != settingsOn && settingsOn;

        mainRoot.SetActive(true);
        settingsRoot.SetActive(true);
    }

    private void Start()
    {
        ApplySettledLayout(_showingSettings);
    }

    private void OnDestroy()
    {
        if (_transition.isAlive)
            _transition.Stop();

        openSettingsButton.onClick.RemoveListener(GoToSettings);
        closeSettingsButton.onClick.RemoveListener(GoToMainMenu);
    }

    public void GoToSettings()
    {
        CrossFade(true);
    }

    public void GoToMainMenu()
    {
        CrossFade(false);
    }

    private void CrossFade(bool toSettings)
    {
        if (toSettings == _showingSettings)
            return;

        if (_transition.isAlive)
            _transition.Stop();

        _showingSettings = toSettings;

        RectTransform outgoing = toSettings ? mainMenuPanel : settingsPanel;
        RectTransform incoming = toSettings ? settingsPanel : mainMenuPanel;
        CanvasGroup outgoingCg = toSettings ? mainMenuCanvasGroup : settingsCanvasGroup;
        CanvasGroup incomingCg = toSettings ? settingsCanvasGroup : mainMenuCanvasGroup;

        Vector2 outEnd;
        Vector2 inStart;
        if (toSettings)
        {
            outEnd = _mainHome + Vector2.left * slideDistance;
            inStart = _settingsHome + Vector2.right * slideDistance;
        }
        else
        {
            outEnd = _settingsHome + Vector2.right * slideDistance;
            inStart = _mainHome + Vector2.left * slideDistance;
        }

        incoming.anchoredPosition = inStart;
        incoming.localScale = Vector3.one * incomingScaleFrom;
        incomingCg.alpha = 0f;

        outgoing.anchoredPosition = toSettings ? _mainHome : _settingsHome;
        outgoing.localScale = Vector3.one;
        outgoingCg.alpha = 1f;

        SetRaycastsForTransition(true);

        Vector2 targetHome = toSettings ? _settingsHome : _mainHome;

        Tween outMove = Tween.UIAnchoredPosition(outgoing, outEnd, duration, moveEase);
        outMove.OnComplete(() =>
        {
            outgoing.anchoredPosition = toSettings ? _mainHome : _settingsHome;
            outgoing.localScale = Vector3.one;
            outgoingCg.alpha = 0f;
            SetRaycastsForTransition(false);
        });

        _transition = Sequence.Create();
        _transition.Group(outMove);
        _transition.Group(Tween.Alpha(outgoingCg, 0f, duration, moveEase));
        _transition.Group(Tween.UIAnchoredPosition(incoming, targetHome, duration, moveEase));
        _transition.Group(Tween.Alpha(incomingCg, 1f, duration, moveEase));
        _transition.Group(Tween.Scale(incoming, Vector3.one, duration, scaleEase));
    }

    private void ApplySettledLayout(bool settingsOn)
    {
        _showingSettings = settingsOn;

        mainMenuPanel.anchoredPosition = _mainHome;
        mainMenuPanel.localScale = Vector3.one;
        settingsPanel.anchoredPosition = _settingsHome;
        settingsPanel.localScale = Vector3.one;

        if (settingsOn)
        {
            mainMenuCanvasGroup.alpha = 0f;
            settingsCanvasGroup.alpha = 1f;
        }
        else
        {
            settingsCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.alpha = 1f;
        }

        SetRaycastsForTransition(false);
    }

    private void SetRaycastsForTransition(bool transitionRunning)
    {
        if (transitionRunning)
        {
            mainMenuCanvasGroup.blocksRaycasts = false;
            settingsCanvasGroup.blocksRaycasts = false;
        }
        else if (_showingSettings)
        {
            mainMenuCanvasGroup.blocksRaycasts = false;
            settingsCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            settingsCanvasGroup.blocksRaycasts = false;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }
    }
}