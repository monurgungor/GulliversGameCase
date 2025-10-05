using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using System;
using Zenject;

public class HighscorePanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas highscoreCanvas;
    [SerializeField] private Image canvasBackground;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI noWordsLeftText;
    [SerializeField] private RectTransform[] highscoreStars;
    [SerializeField] private RectTransform starImage;
    [SerializeField] private RectTransform shineImage;
    [SerializeField] private RectTransform mainMenuButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private float delayBetweenAnimations = 0.3f;
    [SerializeField] private float backgroundFadeAlpha = 0.7f;
    [SerializeField] private float noWordsShakeAmount = 30f;
    [SerializeField] private float shineRotationSpeed = 360f;
    [SerializeField] private float punchScale = 1.2f;

    [Inject]private GameStateManager gameStateManager;
    private GameEndData currentGameEndData;


    private void OnEnable()
    {
        GameStateManager.OnGameEndProcessed += OnGameEndProcessed;
    }

    private void OnDisable()
    {
        GameStateManager.OnGameEndProcessed -= OnGameEndProcessed;
    }

    private void OnGameEndProcessed(GameEndData gameEndData)
    {
        currentGameEndData = gameEndData;
        StartGameEndSequence();
    }

    private void StartGameEndSequence()
    {
        SetInitialStates();
        
        highscoreCanvas.enabled = true;

        CreateGameEndAnimationSequence();
    }

    private void SetInitialStates()
    {
        canvasBackground.color = new Color(canvasBackground.color.r, canvasBackground.color.g, canvasBackground.color.b, 0f);

        noWordsLeftText.rectTransform.anchoredPosition = new Vector2(Screen.width * 1.5f, noWordsLeftText.rectTransform.anchoredPosition.y);

        starImage.localScale = Vector3.zero;
        shineImage.localScale = Vector3.zero;

        highscoreText.rectTransform.anchoredPosition = new Vector2(Screen.width * 1.5f, highscoreText.rectTransform.anchoredPosition.y);
        mainMenuButton.anchoredPosition = new Vector2(Screen.width * 1.5f, mainMenuButton.anchoredPosition.y);

        if (highscoreStars != null)
        {
            foreach (var star in highscoreStars)
            {
                if (star != null)
                    star.localScale = Vector3.zero;
            }
        }
        
        mainMenuButton.GetComponent<Button>().onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void CreateGameEndAnimationSequence()
    {
        Sequence mainSequence = Sequence.Create();

        if (currentGameEndData.isDeadlockEnd)
        {
            mainSequence.Group(CreateNoWordsLeftSequence());
        }

        mainSequence.Chain(CreateBackgroundAnimation());
    

        if (currentGameEndData.isNewHighScore)
        {
            mainSequence.Chain(CreateNewHighScoreStarsSequence());
        }
        else
        {
            mainSequence.Chain(CreateStarImageAnimation());
        }
        
        mainSequence.Chain(CreateShineImageAnimation());
        mainSequence.Chain(CreateHighscoreTextAnimation());
        mainSequence.Chain(CreateMainMenuButtonAnimation());
    }

    private Sequence CreateNoWordsLeftSequence()
    {
        Vector2 targetPos = Vector2.zero;
        Vector2 exitPos = new Vector2(-Screen.width * 1.5f, targetPos.y);
        Vector2 shakePos1 = targetPos + Vector2.right * noWordsShakeAmount;
        Vector2 shakePos2 = targetPos - Vector2.right * noWordsShakeAmount;

        Sequence noWordsSequence = Sequence.Create();
        
        noWordsSequence.Chain(Tween.UIAnchoredPosition(noWordsLeftText.rectTransform, targetPos, animationDuration, Ease.OutBack));
        
        Sequence shakeSequence = Sequence.Create();
        shakeSequence.Chain(Tween.UIAnchoredPosition(noWordsLeftText.rectTransform, shakePos1, 0.1f, Ease.InOutSine));
        shakeSequence.Chain(Tween.UIAnchoredPosition(noWordsLeftText.rectTransform, shakePos2, 0.1f, Ease.InOutSine));
        shakeSequence.Chain(Tween.UIAnchoredPosition(noWordsLeftText.rectTransform, shakePos1, 0.1f, Ease.InOutSine));
        shakeSequence.Chain(Tween.UIAnchoredPosition(noWordsLeftText.rectTransform, targetPos, 0.1f, Ease.InOutSine));
        
        noWordsSequence.Chain(shakeSequence);
        noWordsSequence.Chain(Tween.UIAnchoredPosition(noWordsLeftText.rectTransform, exitPos, animationDuration, Ease.InBack));
        noWordsSequence.ChainDelay(delayBetweenAnimations);

        return noWordsSequence;
    }

    private Tween CreateBackgroundAnimation()
    {
        Color targetColor = new Color(canvasBackground.color.r, canvasBackground.color.g, canvasBackground.color.b, backgroundFadeAlpha);
        return Tween.Color(canvasBackground, targetColor, animationDuration, Ease.OutQuad)
            .OnComplete(() => Tween.Delay(delayBetweenAnimations));
    }

    private Tween CreateStarImageAnimation()
    {
        return Tween.Scale(starImage, Vector3.one, animationDuration, Ease.OutBack)
            .OnComplete(() => Tween.Delay(delayBetweenAnimations));
    }

    private Tween CreateShineImageAnimation()
    {
        return Tween.Scale(shineImage, Vector3.one, animationDuration, Ease.OutBack)
            .OnComplete(() => 
            {
                StartShineRotation();
                Tween.Delay(delayBetweenAnimations);
            });
    }

    private void StartShineRotation()
    {
        float rotationDuration = 2f;
        Vector3 currentRotation = shineImage.transform.eulerAngles;
        Vector3 targetRotation = new Vector3(currentRotation.x, currentRotation.y, currentRotation.z + 360f);
        
        Tween.Rotation(shineImage, Quaternion.Euler(targetRotation), rotationDuration, Ease.Linear)
            .OnComplete(() => StartShineRotation());
    }

    private Sequence CreateNewHighScoreStarsSequence()
    {
        if (highscoreStars == null) return Sequence.Create();

        Sequence starsSequence = Sequence.Create();

        for (int i = 0; i < highscoreStars.Length; i++)
        {
            if (highscoreStars[i] != null)
            {
                starsSequence.Chain(Tween.Scale(highscoreStars[i], Vector3.one, animationDuration * 0.5f, Ease.OutBack));
                starsSequence.ChainDelay(0.1f);
            }
        }
        
        starsSequence.ChainDelay(delayBetweenAnimations);
        return starsSequence;
    }

    private Sequence CreateHighscoreTextAnimation()
    {
        Vector2 targetPos = Vector2.zero;
        
        Sequence textSequence = Sequence.Create();
        textSequence.Chain(Tween.UIAnchoredPosition(highscoreText.rectTransform, targetPos, animationDuration, Ease.OutBack));
        
        textSequence.ChainCallback(() =>
        {
            if (currentGameEndData.isNewHighScore)
            {
                highscoreText.text = $"NEW HIGH SCORE\n{currentGameEndData.currentScore}";
            }
            else
            {
                highscoreText.text = $"SCORE:{currentGameEndData.currentScore}\nHIGHSCORE:{currentGameEndData.previousHighScore}";
            }
        });
        
        if (currentGameEndData.isNewHighScore)
        {
            textSequence.Chain(Tween.Scale(highscoreText.rectTransform, Vector3.one * punchScale, 0.2f, Ease.OutQuad));
            textSequence.Chain(Tween.Scale(highscoreText.rectTransform, Vector3.one, 0.2f, Ease.OutQuad));
        }
        
        textSequence.ChainDelay(delayBetweenAnimations);
        return textSequence;
    }

    private Tween CreateMainMenuButtonAnimation()
    {
        Vector2 targetPos = Vector2.zero;
        return Tween.UIAnchoredPosition(mainMenuButton, targetPos, animationDuration, Ease.OutBack);
    }

    public void OnMainMenuButtonClicked()
    {
        if (gameStateManager != null)
        {
            gameStateManager.ReturnToMainMenu();
        }
    }
}
