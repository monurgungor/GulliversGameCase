using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;
using PrimeTween;

public class WordSectionUI : MonoBehaviour
{
    [SerializeField] private Button undoButton;
    [SerializeField] private Button submitButton;

    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI totalScoreText;

    [SerializeField] private TextMeshProUGUI levelTitleText;

    [SerializeField] private ScrollRect wordSectionScrollRect;
    [SerializeField] private RectTransform wordContainer;
    [SerializeField] private TextMeshProUGUI wordPrefab;

    [SerializeField] private RectTransform buttonsPanel;
    [SerializeField] private RectTransform wordSectionPanel;
    
    [Header("UI Settings")]
    [SerializeField] private string currentScorePrefix = "pts";
    [SerializeField] private string totalScorePrefix = "Score: ";
    
    [Header("Animation Settings")]
    [SerializeField] private float initialAnimationDuration = 0.8f;
    [SerializeField] private Ease initialAnimationEase = Ease.OutCubic;
    [SerializeField] private float scrollAnimationDuration = 0.5f;
    [SerializeField] private Ease scrollEase = Ease.OutQuad;

    private RectTransform textBox;
    
    private Vector2 buttonsInitialPos;
    private Vector2 wordSectionInitialPos;

    [Inject] private LevelController levelController;

    private void Awake()
    {
        buttonsInitialPos = buttonsPanel.anchoredPosition;
        wordSectionInitialPos = wordSectionPanel.anchoredPosition;
    }

    private void Initialize()
    {
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
        returnToMenuButton.onClick.AddListener(delegate { levelController.ReturnToMainMenu(); });

        textBox = currentScoreText.transform.parent.GetComponent<RectTransform>();

        UpdateCurrentScoreDisplay(0);
        UpdateTotalScoreDisplay(0);
    }

    private void OnEnable()
    {   
        WordActions.OnWordValidityChanged += OnWordValidityChanged;
        WordActions.OnWordScoreChanged += OnWordScoreChanged;
        WordActions.OnWordSubmitted += OnWordSubmitted;
        LevelLoader.OnLevelLoaded += OnLevelLoaded;
    }

    private void Start()
    {
        ScoreManager.OnScoreUpdated += UpdateTotalScoreDisplay;
        
        Vector2 buttonsStartPos = new Vector2(buttonsInitialPos.x, -buttonsInitialPos.y);
        Vector2 wordSectionStartPos = new Vector2(wordSectionInitialPos.x, -wordSectionInitialPos.y);
        
        buttonsPanel.anchoredPosition = buttonsStartPos;
        wordSectionPanel.anchoredPosition = wordSectionStartPos;

        Tween.Custom(
            startValue: buttonsStartPos, 
            endValue: buttonsInitialPos, 
            duration: initialAnimationDuration, 
            ease: initialAnimationEase,
            onValueChange: val => buttonsPanel.anchoredPosition = val);
            
        Tween.Custom(
            startValue: wordSectionStartPos,
            endValue: wordSectionInitialPos,
            duration: initialAnimationDuration,
            ease: initialAnimationEase,
            onValueChange: val => wordSectionPanel.anchoredPosition = val);
    }

    private void OnDisable()
    {
        submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
        
        WordActions.OnWordValidityChanged -= OnWordValidityChanged;
        WordActions.OnWordScoreChanged -= OnWordScoreChanged;
        WordActions.OnWordSubmitted -= OnWordSubmitted;
        ScoreManager.OnScoreUpdated -= UpdateTotalScoreDisplay;
        LevelLoader.OnLevelLoaded -= OnLevelLoaded;
    }

    private void OnSubmitButtonClicked()
    {
        WordActions.OnWordSubmitRequested?.Invoke();
    }


    private void OnWordValidityChanged(bool isValid)
    {
        submitButton.interactable = isValid;
        
        ColorBlock colors = submitButton.colors;
        colors.normalColor = isValid ? Color.green : Color.gray;
        submitButton.colors = colors;
    }

    private void OnWordScoreChanged(int score)
    {
        UpdateCurrentScoreDisplay(score);
    }

    private void UpdateCurrentScoreDisplay(int score)
    {
        if (currentScoreText != null)
        {
            if (score > 0)
            {
                currentScoreText.text = $"{score}{currentScorePrefix}";
                textBox.gameObject.SetActive(true);
            }
            else
            {
                textBox.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateTotalScoreDisplay(int totalScore)
    {
        if (totalScoreText != null)
        {
            totalScoreText.text = $"{totalScorePrefix}{totalScore}";
        }
    }

    private void OnWordSubmitted(string word)
    {
        TextMeshProUGUI wordText = Instantiate(wordPrefab, wordContainer);
        wordText.text = word;
        
        AnimateScrollToBottom();
    }
    
    private void AnimateScrollToBottom()
    {
        if (wordSectionScrollRect == null) return;
        
        Sequence scrollSequence = Sequence.Create();
        
        scrollSequence.Chain(Tween.Delay(0.02f));
        
        scrollSequence.ChainCallback(() => {
            LayoutRebuilder.ForceRebuildLayoutImmediate(wordContainer);
        });
        
        scrollSequence.Chain(
            Tween.Custom(wordSectionScrollRect.verticalNormalizedPosition, 0f, scrollAnimationDuration, 
                onValueChange: value => wordSectionScrollRect.verticalNormalizedPosition = value, 
                ease: scrollEase)
        );
    }

    private void OnLevelLoaded(int levelId)
    {
        levelTitleText.text = levelController.GetLevelTitle(levelId);
        Initialize();
    }
}
