using UnityEngine;

/// <summary>
/// The two things that have to survive a scene load: which level to open, and
/// which level was just unlocked so the menu can play its animation. Keeping the
/// keys here stops them being spelled out at four different call sites.
/// </summary>
public static class LevelProgress
{
    private const string LevelToLoadKey = "LevelToLoad";
    private const string JustUnlockedKey = "JustUnlockedLevel";

    public static void RequestLevel(int levelId)
    {
        PlayerPrefs.SetInt(LevelToLoadKey, levelId);
        PlayerPrefs.Save();
    }

    /// <summary>Reads and clears the requested level, falling back to the first one.</summary>
    public static int ConsumeRequestedLevel()
    {
        int levelId = PlayerPrefs.GetInt(LevelToLoadKey, 1);
        PlayerPrefs.DeleteKey(LevelToLoadKey);
        PlayerPrefs.Save();
        return levelId > 0 ? levelId : 1;
    }

    public static void MarkJustUnlocked(int levelId)
    {
        PlayerPrefs.SetInt(JustUnlockedKey, levelId);
        PlayerPrefs.Save();
    }

    /// <summary>Reads and clears the level to celebrate, or 0 when there is none.</summary>
    public static int ConsumeJustUnlocked()
    {
        int levelId = PlayerPrefs.GetInt(JustUnlockedKey, 0);
        PlayerPrefs.DeleteKey(JustUnlockedKey);
        PlayerPrefs.Save();
        return levelId;
    }

    public static int PeekJustUnlocked()
    {
        return PlayerPrefs.GetInt(JustUnlockedKey, 0);
    }
}
