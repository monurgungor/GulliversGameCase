using System;

/// <summary>
/// Interface for classes that need to receive player data updates
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// Called when player data is loaded
    /// </summary>
    /// <param name="playerData">Loaded player data</param>
    void OnDataLoad(PlayerData playerData);
    
    /// <summary>
    /// Called when player data should be saved
    /// </summary>
    /// <param name="playerData">Player data to update</param>
    void OnDataSave(PlayerData playerData);
    
    /// <summary>
    /// Called when player data is requested
    /// </summary>
    /// <param name="playerData">Current player data</param>
    void OnPlayerDataRequested(PlayerData playerData);
} 