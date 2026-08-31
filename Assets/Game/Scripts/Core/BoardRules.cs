/// <summary>
/// The fixed rules of the board. Gameplay, the deadlock search and the editor
/// tool that bakes the word list all read these, so they cannot drift apart.
/// </summary>
public static class BoardRules
{
    /// <summary>Letter slots the player can fill before submitting a word.</summary>
    public const int SlotCount = 7;

    /// <summary>Points removed for every tile left on the board when no word is left.</summary>
    public const int DeadlockPenaltyPerTile = 10;
}
