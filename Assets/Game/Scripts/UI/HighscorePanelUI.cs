using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// The end of level panel. It plays one sequence: the "no words left" banner
/// when the board died, then the dim, the star, the score, and the way back to
/// the menu.
/// </summary>
public class HighscorePanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private Canvas highscoreCanvas;
    [SerializeField] private Image dimBackground;

    [Header("Elements")]
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI noWordsLeftText;
    [SerializeField] private RectTransform starImage;
    [SerializeField] private RectTransform shineImage;
    [SerializeField] private RectTransform mainMenuButton;
    [Tooltip("Extra stars that only appear when the player beat their record.")]
    [SerializeField] private RectTransform[] highscoreStars;

    [Header("Animation")]
    [SerializeField] private float stepDuration = 0.8f;
    [SerializeField] private float pauseBetweenSteps = 0.3f;
    [SerializeField] private float dimAlpha = 0.7f;
    [SerializeField] private float bannerShakeDistance = 30f;
    [SerializeField] private float shineSpinDuration = 2f;
    [SerializeField] private float newRecordPunch = 1.2f;

    [Inject] private GameStateManager gameStateManager;

    private void Awake()
    {
        highscoreCanvas.enabled = false;
        mainMenuButton.GetComponent<Button>().onClick.AddListener(gameStateManager.ReturnToMainMenu);
    }

    private void OnEnable()
    {
        GameStateManager.GameEndProcessed += Show;
    }

    private void OnDisable()
    {
        GameStateManager.GameEndProcessed -= Show;
    }

    private void Show(GameEndData result)
    {
        HideEverything();
        highscoreCanvas.enabled = true;

        Sequence panel = Sequence.Create();

        if (result.Reason == GameEndReason.NoWordsLeft)
        {
            panel.Chain(NoWordsLeftBanner());
        }

        panel.Chain(Tween.Color(dimBackground, WithAlpha(dimBackground.color, dimAlpha), stepDuration, Ease.OutQuad));
        panel.ChainDelay(pauseBetweenSteps);

        panel.Chain(result.IsNewHighScore ? NewRecordStars() : Sequence.Create(Grow(starImage, stepDuration)));

        panel.Chain(Grow(shineImage, stepDuration));
        panel.ChainCallback(SpinShineForever);

        panel.Chain(ScoreText(result));
        panel.ChainDelay(pauseBetweenSteps);

        panel.Chain(Tween.UIAnchoredPosition(mainMenuButton, Vector2.zero, stepDuration, Ease.OutBack));
    }

    /// <summary>Puts everything off screen or at zero scale before the sequence runs.</summary>
    private void HideEverything()
    {
        dimBackground.color = WithAlpha(dimBackground.color, 0f);

        starImage.localScale = Vector3.zero;
        shineImage.localScale = Vector3.zero;

        foreach (RectTransform star in highscoreStars)
        {
            star.localScale = Vector3.zero;
        }

        ParkOffScreen(noWordsLeftText.rectTransform);
        ParkOffScreen(highscoreText.rectTransform);
        ParkOffScreen(mainMenuButton);
    }

    private static void ParkOffScreen(RectTransform target)
    {
        target.anchoredPosition = new Vector2(Screen.width * 1.5f, target.anchoredPosition.y);
    }

    /// <summary>Slides the banner in, shakes it, then throws it off the other side.</summary>
    private Sequence NoWordsLeftBanner()
    {
        RectTransform banner = noWordsLeftText.rectTransform;

        return Sequence.Create(Tween.UIAnchoredPosition(banner, Vector2.zero, stepDuration, Ease.OutBack))
            .Chain(Tween.ShakeLocalPosition(banner, Vector3.right * bannerShakeDistance, 0.4f))
            .Chain(Tween.UIAnchoredPosition(banner, new Vector2(-Screen.width * 1.5f, 0f), stepDuration, Ease.InBack))
            .ChainDelay(pauseBetweenSteps);
    }

    private Sequence NewRecordStars()
    {
        Sequence stars = Sequence.Create(Grow(starImage, stepDuration));

        foreach (RectTransform star in highscoreStars)
        {
            stars.Chain(Grow(star, stepDuration * 0.5f));
        }

        return stars;
    }

    private Sequence ScoreText(GameEndData result)
    {
        RectTransform text = highscoreText.rectTransform;

        Sequence sequence = Sequence.Create(Tween.UIAnchoredPosition(text, Vector2.zero, stepDuration, Ease.OutBack));

        sequence.ChainCallback(() =>
        {
            highscoreText.text = result.IsNewHighScore
                ? $"NEW HIGH SCORE\n{result.Score}"
                : $"SCORE: {result.Score}\nHIGH SCORE: {result.PreviousHighScore}";
        });

        if (result.IsNewHighScore)
        {
            sequence.Chain(Tween.PunchScale(text, Vector3.one * (newRecordPunch - 1f), 0.4f));
        }

        return sequence;
    }

    private void SpinShineForever()
    {
        Tween.LocalEulerAngles(shineImage, Vector3.zero, new Vector3(0f, 0f, 360f),
            shineSpinDuration, Ease.Linear, cycles: -1);
    }

    private static Tween Grow(RectTransform target, float duration)
    {
        return Tween.Scale(target, Vector3.one, duration, Ease.OutBack);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}
