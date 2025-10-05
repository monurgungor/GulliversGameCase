using System;

[System.Serializable]
public struct GameEndData
{
    public int currentScore;
    public int previousHighScore;
    public bool isNewHighScore;
    public bool isDeadlockEnd;
    public string levelName;
}