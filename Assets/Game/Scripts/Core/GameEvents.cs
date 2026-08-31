using System;

/// <summary>Why a level finished.</summary>
public enum GameEndReason
{
    /// <summary>Every tile was played into a word.</summary>
    BoardCleared,

    /// <summary>Tiles remain but no unplayed word can be built from them.</summary>
    NoWordsLeft,
}

/// <summary>
/// The single place a level's end is announced. TileManager owns that decision
/// and applies the deadlock penalty before raising it, so every listener reads
/// the same final score.
/// </summary>
public static class GameEvents
{
    public static event Action<GameEndReason> GameEnded;

    public static void RaiseGameEnded(GameEndReason reason)
    {
        GameEnded?.Invoke(reason);
    }
}
