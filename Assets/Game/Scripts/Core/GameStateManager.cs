using System;
using UnityEngine;
using Zenject;

public class GameStateManager : MonoBehaviour
{
    [Inject] private LevelController levelController;
    [Inject] private ScoreManager scoreManager;

    private int currentLevelId;
    private int currentScore;
    private int previousHighScore;
    private bool isNewHighScore;
    private bool isDeadlockEnd;

    public static event Action<GameEndData> OnGameEndProcessed;

    private void OnEnable()
    {
        GameEvents.OnGameCompleted += OnGameCompleted;
        GameEvents.OnDeadlockDetected += OnDeadlockDetected;
        LevelLoader.OnLevelLoaded += OnLevelLoaded;
    }

    private void OnDisable()
    {
        GameEvents.OnGameCompleted -= OnGameCompleted;
        GameEvents.OnDeadlockDetected -= OnDeadlockDetected;
        LevelLoader.OnLevelLoaded -= OnLevelLoaded;
    }

    private void OnLevelLoaded(int levelId)
    {
        currentLevelId = levelId;
        ResetGameState();
        
        if (levelController != null && currentLevelId > 0)
        {
            previousHighScore = levelController.GetHighScoreForLevel(currentLevelId);
        }
    }

    private void OnDeadlockDetected(DeadlockResult result)
    {
        isDeadlockEnd = result.isDeadlocked;
    }

    private void OnGameCompleted()
    {
        ProcessGameCompletion();
    }

    private void ProcessGameCompletion()
    {
        if (scoreManager != null)
        {
            currentScore = scoreManager.CurrentScore;
        }

        if (levelController != null && currentLevelId > 0)
        {
            isNewHighScore = currentScore > 0 && currentScore > previousHighScore;
            
            if (isNewHighScore)
            {
                levelController.CompleteLevel(currentLevelId, currentScore);
            }
        }

        var gameEndData = new GameEndData
        {
            currentScore = currentScore,
            previousHighScore = previousHighScore,
            isNewHighScore = isNewHighScore,
            isDeadlockEnd = isDeadlockEnd,
            levelName = $"level_{currentLevelId}"
        };

        OnGameEndProcessed?.Invoke(gameEndData);
    }

    private void ResetGameState()
    {
        currentScore = 0;
        previousHighScore = 0;
        isNewHighScore = false;
        isDeadlockEnd = false;
    }

    public void ReturnToMainMenu()
    {
        if (levelController != null)
        {
            levelController.ReturnToMainMenu();
        }
    }


}