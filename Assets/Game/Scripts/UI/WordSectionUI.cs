using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// The panel beside the board: the level title, the running score, the value of
/// the letters in the slots, and the list of words already played.
/// </summary>
public class WordSectionUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button returnToMenuButton;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [Tooltip("Bubble around the pending score, hidden while the letters spell nothing.")]
    [SerializeField] private RectTransform currentScoreBubble;

    [Header("Played Words")]
    [SerializeField] private ScrollRect wordScrollRect;
    [SerializeField] private RectTransform wordContainer;
    [SerializeField] private TextMeshProUGUI wordPrefab;

    [Header("Panels")]
    [SerializeField] private RectTransform buttonsPanel;
    [SerializeField] private RectTransform wordSectionPanel;

    [Header("Text")]
    [SerializeField] private string currentScoreSuffix = "pts";
    [SerializeField] private string totalScorePrefix = "Score: ";

    [Header("Animation")]
    [SerializeField] private float slideInDuration = 0.8f;
    [SerializeField] private Ease slideInEase = Ease.OutCubic;
    [SerializeField] private float scrollDuration = 0.5f;
    [SerializeField] private Ease scrollEase = Ease.OutQuad;

    [Inject] private LevelController levelController;

    private Vector2 buttonsRestPosition;
    private Vector2 wordSectionRestPosition;

    // Built once: TextMeshPro only substitutes numbers, so the words around the
    // number have to be baked into the format string to keep updates allocation free.
    private string pendingScoreFormat;
    private string totalScoreFormat;

    private void Awake()
    {
        pendingScoreFormat = "{0}" + currentScoreSuffix;
        totalScoreFormat = totalScorePrefix + "{0}";

        buttonsRestPosition = buttonsPanel.anchoredPosition;
        wordSectionRestPosition = wordSectionPanel.anchoredPosition;

        submitButton.onClick.AddListener(WordActions.RaiseSubmitRequested);
        returnToMenuButton.onClick.AddListener(OnReturnToMenu);

        SetSubmitEnabled(false);
        SetPendingScore(0);
        SetTotalScore(0);
    }

    private void OnEnable()
    {
        WordActions.WordValidityChanged += SetSubmitEnabled;
        WordActions.WordScoreChanged += SetPendingScore;
        WordActions.WordSubmitted += AddPlayedWord;
        ScoreManager.ScoreChanged += SetTotalScore;
        LevelLoader.LevelLoaded += ShowLevelTitle;
    }

    private void OnDisable()
    {
        WordActions.WordValidityChanged -= SetSubmitEnabled;
        WordActions.WordScoreChanged -= SetPendingScore;
        WordActions.WordSubmitted -= AddPlayedWord;
        ScoreManager.ScoreChanged -= SetTotalScore;
        LevelLoader.LevelLoaded -= ShowLevelTitle;
    }

    private void Start()
    {
        SlideIn(buttonsPanel, buttonsRestPosition);
        SlideIn(wordSectionPanel, wordSectionRestPosition);
    }

    private void OnDestroy()
    {
        submitButton.onClick.RemoveListener(WordActions.RaiseSubmitRequested);
        returnToMenuButton.onClick.RemoveListener(OnReturnToMenu);
    }

    private void OnReturnToMenu()
    {
        levelController.ReturnToMainMenu();
    }

    private void ShowLevelTitle(int levelId)
    {
        levelTitleText.text = levelController.GetTitle(levelId);
    }

    private void SetSubmitEnabled(bool canSubmit)
    {
        submitButton.interactable = canSubmit;
    }

    private void SetPendingScore(int score)
    {
        currentScoreBubble.gameObject.SetActive(score > 0);

        if (score > 0)
        {
            currentScoreText.SetText(pendingScoreFormat, score);
        }
    }

    private void SetTotalScore(int total)
    {
        totalScoreText.SetText(totalScoreFormat, total);
    }

    private void AddPlayedWord(string word)
    {
        TextMeshProUGUI entry = Instantiate(wordPrefab, wordContainer);
        entry.SetText(word);

        LayoutRebuilder.ForceRebuildLayoutImmediate(wordContainer);
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Tween.Custom(
            wordScrollRect.verticalNormalizedPosition,
            0f,
            scrollDuration,
            onValueChange: value => wordScrollRect.verticalNormalizedPosition = value,
            ease: scrollEase);
    }

    /// <summary>Slides a panel up from just off screen into its authored position.</summary>
    private void SlideIn(RectTransform panel, Vector2 restPosition)
    {
        Vector2 start = new Vector2(restPosition.x, -restPosition.y);
        panel.anchoredPosition = start;

        Tween.UIAnchoredPosition(panel, restPosition, slideInDuration, slideInEase);
    }
}
