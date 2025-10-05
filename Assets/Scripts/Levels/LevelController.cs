using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zenject;
using Newtonsoft.Json;

/// <summary>
/// Handles all level-related operations including loading, discovery, and management
/// </summary>
public class LevelController : MonoBehaviour
{
    private const string levelsFolderPath = "levels";

    public static Action<int> OnLevelRequested;
    
    private List<LevelData> availableLevels = new List<LevelData>();
    private PlayerData playerData;
        
    [Inject] private SaveManager saveManager;
    [Inject] private SceneController sceneController;
    

    private void Start()
    {
        SaveManager.OnDataLoaded += OnPlayerDataLoaded;
        OnLevelRequested += LoadGameScene;
        
        if (saveManager != null && saveManager.IsDataLoaded)
        {
            OnPlayerDataLoaded(saveManager.GetCurrentPlayerData());
        }
    }

    private void OnDestroy()
    {
        SaveManager.OnDataLoaded -= OnPlayerDataLoaded;
        OnLevelRequested -= LoadGameScene;
    }
    
    /// <summary>
    /// Called when player data is loaded from SaveManager
    /// </summary>
    /// <param name="playerData">Loaded player data</param>
    private void OnPlayerDataLoaded(PlayerData playerData)
    {
        this.playerData = playerData;
        
        RefreshLevelSlots();
    }
    
    /// <summary>
    /// Load all levels from Resources folder and create LevelData list
    /// </summary>
    public void LoadAllLevels()
    {
        availableLevels = GetAllLevelData();
        
        availableLevels = availableLevels.OrderBy(level => level.levelId).ToList();
        
    }

    /// <summary>
    /// Get all LevelData from Resources/levels folder
    /// </summary>
    /// <returns>List of LevelData</returns>
    private List<LevelData> GetAllLevelData()
    {
        List<LevelData> levelDataList = new List<LevelData>();
        
        try
        {
            TextAsset[] levelFiles = Resources.LoadAll<TextAsset>(levelsFolderPath);
            
            foreach (TextAsset levelFile in levelFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(levelFile.name);
                
                int levelId = 0;
                if (fileName.StartsWith("level_"))
                {
                    string numberPart = fileName.Substring(6);
                    int.TryParse(numberPart, out levelId);
                }
                
                if (levelId <= 0)
                {
                    continue;
                }
                
                string levelTitle = fileName;
                try
                {
                    LevelJsonData jsonData = JsonConvert.DeserializeObject<LevelJsonData>(levelFile.text);
                    if (!string.IsNullOrEmpty(jsonData.title))
                    {
                        levelTitle = jsonData.title;
                    }
                }
                catch (System.Exception jsonException)
                {
                    Debug.LogWarning($"LevelController: Could not parse JSON for {levelFile.name}: {jsonException.Message}. Using filename as title.");
                }
                
                bool isUnlocked = (levelId == 1);
                int highScore = 0;
                
                if(playerData != null)
                {
                    isUnlocked = playerData.IsLevelUnlocked(levelId);
                    highScore = playerData.GetLevelScore(levelId);
                }

                LevelData levelData = new LevelData(levelId, levelTitle, highScore, isUnlocked);
                levelDataList.Add(levelData);
            }
            
            levelDataList = levelDataList.OrderBy(level => level.levelId).ToList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LevelController: Error loading levels: {e.Message}");
        }
        
        return levelDataList;
    }

    
    /// <summary>
    /// Get high score for a specific level
    /// </summary>
    /// <param name="levelId">Level ID</param>
    /// <returns>High score for the level</returns>
    public int GetHighScoreForLevel(int levelId)
    {
        return GetLevelData(levelId).highScore;
    }

    public bool IsLevelUnlocked(int levelId)
    {
        return GetLevelData(levelId).isUnlocked;
    }

    public string GetLevelTitle(int levelId)
    {
        return GetLevelData(levelId).levelTitle;
    }
    


    /// <summary>
    /// Get all available LevelData
    /// </summary>
    /// <returns>List of available LevelData</returns>
    public List<LevelData> GetAvailableLevels()
    {
        return new List<LevelData>(availableLevels);
    }

    /// <summary>
    /// Check if a level exists
    /// </summary>
    /// <param name="levelId">Level ID to check</param>
    /// <returns>True if level exists</returns>
    public bool LevelExists(int levelId)
    {
        return availableLevels.Any(level => level.levelId == levelId);
    }


    /// <summary>
    /// Refresh level slots (useful when new levels are added)
    /// </summary>
    public void RefreshLevelSlots()
    {
        LoadAllLevels();
    }

    /// <summary>
    /// Complete a level and save progress
    /// </summary>
    /// <param name="levelId">Level ID to complete</param>
    /// <param name="score">Score achieved</param>
    public void CompleteLevel(int levelId, int score)
    {
        GetLevelData(levelId).isUnlocked = true;
        if (GetLevelData(levelId).highScore < score)
        {
            GetLevelData(levelId).highScore = score;
        }
        UnlockLevel(levelId+1);
        UpdatePlayerData();
    }

    private void UnlockLevel(int levelId)
    {
        if (LevelExists(levelId))
        {
            GetLevelData(levelId).isUnlocked = true;
            PlayerPrefs.SetInt("UnlockedLevel", levelId);
            PlayerPrefs.Save();
        }
        
    }

    private void UpdatePlayerData()
    {
        playerData.LoadToPlayerData(availableLevels);
        saveManager.SavePlayerData();
    }

    private LevelData GetLevelData(int levelId)
    {
        return availableLevels[levelId - 1];
    }
    

    
    /// <summary>
    /// Get player data
    /// </summary>
    /// <returns>Current player data</returns>
    public PlayerData GetPlayerData()
    {
        return playerData;
    }
    
    /// <summary>
    /// Load game scene for selected level
    /// </summary>
    /// <param name="levelId">Level ID to load</param>
    public async void LoadGameScene(int levelId)
    {
        if (sceneController == null)
        {
            Debug.LogError("LevelController: SceneController not found!");
            return;
        }
        
        if (!LevelExists(levelId))
        {
            return;
        }
        
        if (!GetLevelData(levelId).isUnlocked)
        {
            return;
        }
        
        PlayerPrefs.SetInt("LevelToLoad", levelId);
        PlayerPrefs.Save();
        
        try
        {
            await sceneController.LoadGameSceneAsync(levelId);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LevelController: Error loading game scene: {e.Message}");
        }
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public async void ReturnToMainMenu()
    {
        if (sceneController == null)
        {
            Debug.LogError("LevelController: SceneController not found!");
            return;
        }
        
        Debug.Log("LevelController: Returning to main menu");
        
        try
        {
            await sceneController.ReturnToMainMenuAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LevelController: Error returning to main menu: {e.Message}");
        }
    }
} 