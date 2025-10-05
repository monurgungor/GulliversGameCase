using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelId;
    public string levelTitle;
    public int highScore;
    public bool isUnlocked;

    public LevelData(int levelId, string levelTitle, int highScore = 0, bool isUnlocked = false)
    {
        this.levelId = levelId;
        this.levelTitle = levelTitle;
        this.highScore = highScore;
        this.isUnlocked = isUnlocked;

    }

}

[System.Serializable]
public class PlayerLevelData
{
    public int highScore;
    public bool isUnlocked;

    public PlayerLevelData(int highScore = 0, bool isUnlocked = false)
    {
        this.highScore = highScore;
        this.isUnlocked = isUnlocked;

    }
    
}
