/// <summary>What the end of level panel needs to know.</summary>
public readonly struct GameEndData
{
    public readonly int Score;
    public readonly int PreviousHighScore;
    public readonly bool IsNewHighScore;
    public readonly GameEndReason Reason;

    public GameEndData(int score, int previousHighScore, bool isNewHighScore, GameEndReason reason)
    {
        Score = score;
        PreviousHighScore = previousHighScore;
        IsNewHighScore = isNewHighScore;
        Reason = reason;
    }
}
