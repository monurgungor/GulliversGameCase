using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
 
    public PlayerLevelDataDictionary levelData = new PlayerLevelDataDictionary();

    /// <summary>
    /// Default constructor
    /// </summary>
    public PlayerData()
    {
        levelData = new PlayerLevelDataDictionary()
        {
            { 1, new PlayerLevelData(0, true) },
        };
    }


    public void LoadToPlayerData(List<LevelData> datas)
    {
        foreach (var level in datas)
        {
            if(!levelData.ContainsKey(level.levelId))
            {
                levelData.Add(level.levelId, new PlayerLevelData(level.highScore, level.isUnlocked));
            }
            else
            {
                this.levelData[level.levelId] = new PlayerLevelData(level.highScore, level.isUnlocked);
            }
        }
    }


    /// <summary>
    /// Get score for a specific level
    /// </summary>
    /// <param name="levelId">Level name</param>
    /// <returns>Score for the level, 0 if not found</returns>
    public int GetLevelScore(int levelId)
    {
        if(!levelData.ContainsKey(levelId))
        {
            return 0; 
        }
        return levelData[levelId].highScore;
    }


    /// <summary>
    /// Set LevelData for a specific level
    /// </summary>
    /// <param name="levelData">LevelData to set</param>
    public void SetLevelData(int id, PlayerLevelData levelData)
    {
        this.levelData[id] = levelData;
    }

    /// <summary>
    /// Get all LevelData
    /// </summary>
    /// <returns>List of all LevelData</returns>
    public List<PlayerLevelData> GetAllLevelData()
    {
        return new List<PlayerLevelData>(levelData.Values);
    }

    /// <summary>
    /// Mark a level as completed
    /// </summary>
    /// <param name="levelNumber">Level number to mark as completed</param>
    public void CompleteLevel(int levelNumber, int score)
    {
        if (!levelData.ContainsKey(levelNumber))
        {
            levelData.Add(levelNumber, new PlayerLevelData(score, true));
            return;
        }
        levelData[levelNumber].isUnlocked = true;
        levelData[levelNumber].highScore = Mathf.Max(levelData[levelNumber].highScore, score);
    }


    /// <summary>
    /// Check if a level is unlocked (previous level is completed)
    /// </summary>
    /// <param name="levelNumber">Level number to check</param>
    /// <returns>True if level is unlocked</returns>
    public bool IsLevelUnlocked(int levelNumber)
    {
        if(!levelData.ContainsKey(levelNumber))
        {
            return false; 
        }
        return levelData[levelNumber].isUnlocked;
    }

} 

[Serializable]
public class PlayerLevelDataDictionary :  UnitySerializedDictionary <int, PlayerLevelData> { }