using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>One row in the level list.</summary>
public class LevelSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private Button playButton;
    [SerializeField] private Image lockImage;

    [Header("Unlock Animation")]
    [SerializeField] private float lockShrinkDuration = 0.5f;
    [SerializeField] private float buttonUnlockDelay = 1.5f;

    private int levelId;

    /// <summary>Which level this row opens.</summary>
    public int LevelId => levelId;

    public void Initialize(LevelData level)
    {
        levelId = level.levelId;

        levelNameText.text = $"Level {level.levelId} - {level.levelTitle}";

        highScoreText.gameObject.SetActive(level.highScore > 0);
        if (level.highScore > 0)
        {
            highScoreText.SetText("High Score: {0}", level.highScore);
        }

        playButton.onClick.AddListener(OnPlayClicked);

        // A level that is about to play its unlock animation starts locked, even
        // though the save file already counts it as unlocked.
        bool playable = level.isUnlocked && LevelProgress.PeekJustUnlocked() != level.levelId;
        SetLocked(!playable);
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        LevelController.RequestLevel(levelId);
    }

    /// <summary>Shrinks the padlock away and then hands the button to the player.</summary>
    public void PlayUnlockAnimation()
    {
        Vector3 lockScale = lockImage.transform.localScale;

        Sequence.Create(Tween.Scale(lockImage.transform, Vector3.zero, lockShrinkDuration, Ease.InBack))
            .ChainCallback(() =>
            {
                lockImage.gameObject.SetActive(false);
                lockImage.transform.localScale = lockScale;
            })
            .ChainDelay(buttonUnlockDelay)
            .ChainCallback(() => SetLocked(false))
            .Chain(Tween.PunchScale(playButton.transform, Vector3.one * 0.2f, 0.3f));
    }

    private void SetLocked(bool locked)
    {
        lockImage.gameObject.SetActive(locked);
        playButton.interactable = !locked;
        playButtonText.SetText(locked ? string.Empty : "Play");
    }
}
