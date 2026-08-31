using System;

/// <summary>
/// Events raised while the player builds a word. They are static because the
/// panels that listen sit in a different part of the scene than the board that
/// raises them, and neither should have to hold a reference to the other.
/// </summary>
public static class WordActions
{
    /// <summary>The letters in the slots became submittable, or stopped being so.</summary>
    public static event Action<bool> WordValidityChanged;

    /// <summary>Points the letters in the slots are currently worth.</summary>
    public static event Action<int> WordScoreChanged;

    /// <summary>The player pressed submit.</summary>
    public static event Action SubmitRequested;

    /// <summary>A word was accepted and cleared from the slots.</summary>
    public static event Action<string> WordSubmitted;

    /// <summary>Points to add to the level total.</summary>
    public static event Action<int> ScoreAdded;

    /// <summary>The player asked to take letters back. True returns every letter.</summary>
    public static event Action<bool> UndoRequested;

    /// <summary>Whether there is anything to undo.</summary>
    public static event Action<bool> UndoAvailabilityChanged;

    public static void RaiseWordValidityChanged(bool isValid) => WordValidityChanged?.Invoke(isValid);

    public static void RaiseWordScoreChanged(int score) => WordScoreChanged?.Invoke(score);

    public static void RaiseSubmitRequested() => SubmitRequested?.Invoke();

    public static void RaiseWordSubmitted(string word) => WordSubmitted?.Invoke(word);

    public static void RaiseScoreAdded(int score) => ScoreAdded?.Invoke(score);

    public static void RaiseUndoRequested(bool undoAll) => UndoRequested?.Invoke(undoAll);

    public static void RaiseUndoAvailabilityChanged(bool canUndo) => UndoAvailabilityChanged?.Invoke(canUndo);
}
