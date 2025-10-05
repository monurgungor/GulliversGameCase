using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Zenject;
using System.Threading.Tasks;

/// <summary>
/// Handles saving and loading player data using JSON serialization with event-based system
/// </summary>
public class SaveManager : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "playerdata.json";
    
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
    
    // Events
    public static event Action<PlayerData> OnDataLoaded;
    public static event Action<PlayerData> OnDataSaved;
    
    // Current player data
    private PlayerData currentPlayerData;
    
    // Loading state
    private bool isDataLoaded = false;
    public bool IsDataLoaded => isDataLoaded;
    
    // Zenject injection
    [Inject] private DiContainer container;
    
    /// <summary>
    /// Initialize the save manager asynchronously
    /// </summary>
    public async Task InitializeAsync()
    {
        Debug.Log("SaveManager: Starting async initialization...");
        
        await LoadPlayerDataAsync();
        
        RegisterApplicationEvents();
        
        Debug.Log("SaveManager: Initialization completed");
    }
    
    /// <summary>
    /// Register for application focus and quit events
    /// </summary>
    private void RegisterApplicationEvents()
    {
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isDataLoaded)
        {
            SavePlayerData();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isDataLoaded)
        {
            SavePlayerData();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (isDataLoaded)
        {
            SavePlayerData();
        }
    }
    
    /// <summary>
    /// Load player data asynchronously
    /// </summary>
    public async Task LoadPlayerDataAsync()
    {
        try
        {

            currentPlayerData = await LoadPlayerDataFromFile();
            isDataLoaded = true;
            OnDataLoaded?.Invoke(currentPlayerData);
            
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: Error loading player data: {e.Message}");
            currentPlayerData = new PlayerData();
            isDataLoaded = true;
            OnDataLoaded?.Invoke(currentPlayerData);
        }
    }
    
    /// <summary>
    /// Save player data
    /// </summary>
    public void SavePlayerData()
    {
        if (!isDataLoaded || currentPlayerData == null)
        {
            Debug.LogWarning("SaveManager: Cannot save - data not loaded");
            return;
        }

        SavePlayerDataToFile(currentPlayerData);
        
        OnDataSaved?.Invoke(currentPlayerData);
        
    }
    
    /// <summary>
    /// Get current player data
    /// </summary>
    /// <returns>Current player data</returns>
    public PlayerData GetCurrentPlayerData()
    {
        if (!isDataLoaded)
        {
            Debug.LogWarning("SaveManager: Data not loaded yet");
            return null;
        }
        return currentPlayerData;
    }
    
    /// <summary>
    /// Save player data to JSON file
    /// </summary>
    /// <param name="playerData">Player data to save</param>
    /// <returns>True if save was successful</returns>
    private bool SavePlayerDataToFile(PlayerData playerData)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(playerData, Formatting.Indented);
            File.WriteAllText(SaveFilePath, jsonData);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: Error saving player data: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Load player data from JSON file
    /// </summary>
    /// <returns>Loaded player data or new instance if file doesn't exist</returns>
    private async Task<PlayerData> LoadPlayerDataFromFile()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.Log("SaveManager: No save file found, creating new player data");
                return new PlayerData();
            }
            
            string jsonData = await File.ReadAllTextAsync(SaveFilePath);
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);
            return playerData;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: Error loading player data: {e.Message}");
            Debug.Log("SaveManager: Creating new player data due to error");
            return new PlayerData();
        }
    }
    
    /// <summary>
    /// Check if save file exists
    /// </summary>
    /// <returns>True if save file exists</returns>
    public bool SaveFileExists()
    {
        return File.Exists(SaveFilePath);
    }
    
    /// <summary>
    /// Delete save file
    /// </summary>
    /// <returns>True if deletion was successful</returns>
    public bool DeleteSaveFile()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("SaveManager: Save file deleted successfully");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: Error deleting save file: {e.Message}");
            return false;
        }
    }
} 