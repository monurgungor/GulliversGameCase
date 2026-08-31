using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; private set; }

    public static Action<int> OnScoreUpdated;

    private void OnEnable()
    {
        WordActions.OnScoreAdded += AddScore;
    }

    private void Start()
    {
        OnScoreUpdated?.Invoke(CurrentScore);
    }

    private void OnDisable()
    {
        WordActions.OnScoreAdded -= AddScore;
    }

    /// <summary>
    /// Add score to current total
    /// </summary>
    /// <param name="score">Score to add</param>
    private void AddScore(int score)
    {
        CurrentScore += score;
        OnScoreUpdated?.Invoke(CurrentScore);
    }

    /// <summary>
    /// Subtract score from current total
    /// </summary>
    /// <param name="score">Score to subtract</param>
    public void SubtractScore(int score)
    {
        CurrentScore = Mathf.Max(0, CurrentScore - score);
        OnScoreUpdated?.Invoke(CurrentScore);
    }

    /// <summary>
    /// Reset score to zero
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = 0;
        OnScoreUpdated?.Invoke(CurrentScore);
    }
}
