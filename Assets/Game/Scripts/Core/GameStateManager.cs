using System;
using UnityEngine;
using Zenject;

/// <summary>
/// Closes out a level: records the result, saves it, and tells the end of level
/// panel what to show. TileManager has already applied any penalty by the time
/// this runs, so the score read here is the final one.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    [Inject] private LevelController levelController;
    [Inject] private ScoreManager scoreManager;

    private int levelId;
    private int previousHighScore;
    private bool levelIsOver;

    public static event Action<GameEndData> GameEndProcessed;

    private void OnEnable()
    {
        GameEvents.GameEnded += OnGameEnded;
        LevelLoader.LevelLoaded += OnLevelLoaded;
    }

    private void OnDisable()
    {
        GameEvents.GameEnded -= OnGameEnded;
        LevelLoader.LevelLoaded -= OnLevelLoaded;
    }

    private void OnLevelLoaded(int loadedLevelId)
    {
        levelId = loadedLevelId;
        previousHighScore = levelController.GetHighScore(loadedLevelId);
        levelIsOver = false;
    }

    private void OnGameEnded(GameEndReason reason)
    {
        if (levelIsOver || levelId <= 0)
        {
            return;
        }

        levelIsOver = true;

        int score = scoreManager.CurrentScore;
        bool isNewHighScore = score > previousHighScore;

        levelController.CompleteLevel(levelId, score);

        GameEndProcessed?.Invoke(new GameEndData(score, previousHighScore, isNewHighScore, reason));
    }

    public void ReturnToMainMenu()
    {
        levelController.ReturnToMainMenu();
    }
}
