/// <summary>A level plus the player's progress on it, as the menu shows it.</summary>
public class LevelData
{
    public readonly int levelId;
    public readonly string levelTitle;

    public int highScore;
    public bool isUnlocked;

    public LevelData(int levelId, string levelTitle, int highScore, bool isUnlocked)
    {
        this.levelId = levelId;
        this.levelTitle = levelTitle;
        this.highScore = highScore;
        this.isUnlocked = isUnlocked;
    }
}

/// <summary>The saved half of a level's progress.</summary>
public class PlayerLevelData
{
    public int highScore;
    public bool isUnlocked;

    public PlayerLevelData()
    {
    }

    public PlayerLevelData(int highScore, bool isUnlocked)
    {
        this.highScore = highScore;
        this.isUnlocked = isUnlocked;
    }
}
