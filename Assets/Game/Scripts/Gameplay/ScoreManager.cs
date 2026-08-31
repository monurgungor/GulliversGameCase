using System;
using UnityEngine;

/// <summary>Keeps the running score for the current level.</summary>
public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; private set; }

    public static event Action<int> ScoreChanged;

    private void OnEnable()
    {
        WordActions.ScoreAdded += AddScore;
    }

    private void OnDisable()
    {
        WordActions.ScoreAdded -= AddScore;
    }

    private void Start()
    {
        ScoreChanged?.Invoke(CurrentScore);
    }

    private void AddScore(int points)
    {
        CurrentScore += points;
        ScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Applies the penalty for tiles left on a dead board. Never goes below zero.</summary>
    public void SubtractScore(int points)
    {
        CurrentScore = Mathf.Max(0, CurrentScore - points);
        ScoreChanged?.Invoke(CurrentScore);
    }
}
