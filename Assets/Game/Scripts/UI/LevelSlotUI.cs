using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class LevelSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private Button playButton;
    [SerializeField] private Image lockImage;

    [Header("Animation Settings")]
    [SerializeField] private float lockShrinkDuration = 0.5f;
    [SerializeField] private float buttonUnlockDelay = 1.5f;


    public void Initialize(LevelData levelData)
    {
        levelNameText.text = $"Level {levelData.levelId} - {levelData.levelTitle}";


        if (highScoreText != null)
        {
            if (levelData.highScore > 0)
            {
                highScoreText.text = $"High Score: {levelData.highScore}";
                highScoreText.gameObject.SetActive(true);
            }
            else
            {
                highScoreText.gameObject.SetActive(false);
            }
        }

        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!levelData.isUnlocked);
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => OnPlayButtonClicked(levelData.levelId));
            playButton.interactable = levelData.isUnlocked;
            playButtonText.text = levelData.isUnlocked ? "Play" : "";
        }

        if(PlayerPrefs.GetInt("UnlockedLevel") == levelData.levelId)
        {
            LockButton();
        }
    }

    /// <summary>
    /// Called when play button is clicked
    /// </summary>
    /// <param name="levelId">Level name to load</param>
    private void OnPlayButtonClicked(int levelId)
    {
        Debug.Log($"LevelSlot_UI: Play button clicked for level: {levelId}");

        LevelController.OnLevelRequested?.Invoke(levelId);
    }

    /// <summary>
    /// Update level slot data
    /// </summary>
    /// <param name="highScore">New high score</param>
    /// <param name="isUnlocked">New unlock status</param>
    public void UpdateData(LevelData levelData)
    {

        if (highScoreText != null)
        {
            if (levelData.highScore > 0)
            {
                highScoreText.text = $"High Score: {levelData.highScore}";
                highScoreText.gameObject.SetActive(true);
            }
            else
            {
                highScoreText.gameObject.SetActive(false);
            }
        }

        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!levelData.isUnlocked);
            playButton.interactable = levelData.isUnlocked;
            playButtonText.text = levelData.isUnlocked ? "Play" : "";
        }

    }

    /// <summary>
    /// Play unlock animation sequence
    /// </summary>
    public void PlayUnlockAnimation()
    {
        if (lockImage == null || playButton == null) return;

        LockButton();

        Vector3 originalScale = lockImage.transform.localScale;
        Tween.Scale(lockImage.transform, Vector3.zero, lockShrinkDuration, Ease.InBack)
            .OnComplete(() =>
            {
                lockImage.gameObject.SetActive(false);

                Tween.Delay(buttonUnlockDelay).OnComplete(() =>
                {
                    UnlockButton();
                    lockImage.transform.localScale = originalScale;
                });
            });
    }



    /// <summary>
    /// Unlock button and show play text
    /// </summary>
    private void UnlockButton()
    {
        playButton.interactable = true;
        playButtonText.text = "Play";

        Tween.PunchScale(playButton.transform, Vector3.one * 0.2f, 0.3f);
    }


    private void LockButton()
    {
        playButton.interactable = false;
        playButtonText.text = "";
        lockImage.gameObject.SetActive(true);
    }
}
