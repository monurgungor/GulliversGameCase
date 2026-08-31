using System.Collections.Generic;

/// <summary>
/// Everything the game remembers between runs. Serialized by Newtonsoft rather
/// than by Unity, so a plain dictionary is all it needs.
/// </summary>
public class PlayerData
{
    /// <summary>Progress per level id. Level one starts unlocked.</summary>
    public Dictionary<int, PlayerLevelData> levels = new Dictionary<int, PlayerLevelData>
    {
        { 1, new PlayerLevelData(0, true) },
    };

    public int GetHighScore(int levelId)
    {
        PlayerLevelData level;
        return levels.TryGetValue(levelId, out level) ? level.highScore : 0;
    }

    public bool IsLevelUnlocked(int levelId)
    {
        PlayerLevelData level;
        return levels.TryGetValue(levelId, out level) && level.isUnlocked;
    }

    /// <summary>Copies the live level list back into the saved shape.</summary>
    public void Apply(IReadOnlyList<LevelData> levelList)
    {
        foreach (LevelData level in levelList)
        {
            levels[level.levelId] = new PlayerLevelData(level.highScore, level.isUnlocked);
        }
    }
}
