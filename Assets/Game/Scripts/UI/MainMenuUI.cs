using PrimeTween;
using UnityEngine;

/// <summary>
/// Swaps between the title screen and the level list. Both live in the scene at
/// once and are slid off screen rather than toggled, so the transition reads as
/// one movement.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private RectTransform logo;
    [SerializeField] private RectTransform playButton;
    [SerializeField] private RectTransform levelPanel;
    [SerializeField] private LevelPageUI levelPage;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 1.2f;
    [SerializeField] private float stagger = 0.3f;
    [SerializeField] private Ease enterEase = Ease.OutBack;
    [SerializeField] private Ease exitEase = Ease.InBack;

    private bool showingTitle = true;

    private void Start()
    {
        logo.anchoredPosition = AboveScreen();
        playButton.anchoredPosition = BelowScreen();
        levelPanel.anchoredPosition = RightOfScreen();

        ShowTitle();
    }

    /// <summary>Wired to the play button in the scene.</summary>
    public void ShowLevelPanel()
    {
        if (!showingTitle)
        {
            return;
        }

        showingTitle = false;

        Sequence.Create(Tween.UIAnchoredPosition(logo, AboveScreen(), slideDuration, exitEase))
            .Group(Tween.UIAnchoredPosition(playButton, BelowScreen(), slideDuration, exitEase, startDelay: stagger))
            .Insert(stagger * 0.5f, Tween.UIAnchoredPosition(levelPanel, Vector2.zero, slideDuration, enterEase))
            .OnComplete(levelPage.CelebrateNewlyUnlockedLevel);
    }

    /// <summary>Wired to the back button in the level panel.</summary>
    public void ShowMainMenu()
    {
        if (showingTitle)
        {
            return;
        }

        showingTitle = true;

        Sequence.Create(Tween.UIAnchoredPosition(levelPanel, RightOfScreen(), slideDuration, exitEase))
            .Insert(stagger * 0.5f, Tween.UIAnchoredPosition(logo, Vector2.zero, slideDuration, enterEase))
            .Insert(stagger, Tween.UIAnchoredPosition(playButton, Vector2.zero, slideDuration, enterEase));
    }

    private void ShowTitle()
    {
        Sequence.Create(Tween.UIAnchoredPosition(logo, Vector2.zero, slideDuration, enterEase))
            .Insert(stagger, Tween.UIAnchoredPosition(playButton, Vector2.zero, slideDuration, enterEase));
    }

    private static Vector2 AboveScreen() => new Vector2(0f, Screen.height * 1.5f);

    private static Vector2 BelowScreen() => new Vector2(0f, -Screen.height * 1.5f);

    private static Vector2 RightOfScreen() => new Vector2(Screen.width * 1.5f, 0f);
}
