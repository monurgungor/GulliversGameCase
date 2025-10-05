using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Zenject;
using System;

[System.Serializable]
public class LevelJsonData
{
    public string title;
    public TileData[] tiles;
}


public class LevelLoader : MonoBehaviour
{
    private TilePlacer tilePlacer;
    private int currentLevelId;

    [Inject] LetterSettings letterSettings;
    
    
    public static Action<int> OnLevelLoaded;

    private void Start()
    {
        tilePlacer = FindObjectOfType<TilePlacer>();
        
        int levelToLoad = PlayerPrefs.GetInt("LevelToLoad");
        if (levelToLoad != 0)
        {
            LoadLevelAsync(levelToLoad);
            PlayerPrefs.DeleteKey("LevelToLoad");
        }
        else
        {
            LoadLevelAsync(1);
        }
    }

    /// <summary>
    /// Load level asynchronously
    /// </summary>
    /// <param name="levelName">Level name to load</param>
    public async void LoadLevelAsync(int levelId)
    {
        currentLevelId = levelId;
        
        string jsonContent = await LoadJsonFileAsync(levelId);
            
            LevelJsonData levelData = JsonConvert.DeserializeObject<LevelJsonData>(jsonContent);
            
            TileData[] tileDataArray = ConvertToTileDataArray(levelData.tiles);
            
            if (tilePlacer != null)
            {
                tilePlacer.LoadSpawnAndPlaceTiles(tileDataArray);
            }
            
            OnLevelLoaded?.Invoke(levelId);
    }

    /// <summary>
    /// Load level synchronously (legacy method)
    /// </summary>
    /// <param name="levelName">Level name to load</param>
    public void LoadLevel(int levelId)
    {
        LoadLevelAsync(levelId);
    }
    
    /// <summary>
    /// Load JSON file asynchronously
    /// </summary>
    /// <param name="levelName">Level name to load</param>
    /// <returns>JSON content as string</returns>
    private async Task<string> LoadJsonFileAsync(int levelId)
    {
        string jsonPath = Path.Combine(Application.dataPath, "Resources", "levels", $"level_{levelId}.json");
        
        if (!File.Exists(jsonPath))
        {
            throw new System.IO.FileNotFoundException($"Level file not found: {jsonPath}");
        }
        
        return await Task.Run(() => File.ReadAllText(jsonPath));
    }


    /// <summary>
    /// Convert tile data array synchronously (legacy method)
    /// </summary>
    /// <param name="tiles">Original tile data</param>
    /// <returns>Converted tile data array</returns>
    private TileData[] ConvertToTileDataArray(TileData[] tiles)
    {
        TileData[] tileDataArray = new TileData[tiles.Length];
        
        float scaleFactor = tilePlacer != null ? tilePlacer.GetScaleFactor() : 0.5f;
        
        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = tiles[i];
            
            Vector3 scaledPosition = new Vector3(
                tile.Position.x * scaleFactor, 
                tile.Position.y * scaleFactor, 
                tile.Position.z * scaleFactor
            );
            
            char character = tile.Character;
            
            tileDataArray[i] = new TileData(tile.Id, character, scaledPosition, tile.Children, letterSettings.GetLetterScore(character));
        }
        
        return tileDataArray;
    }
    
    
    public void ReloadCurrentLevel()
    {
        if (currentLevelId != 0)
        {
            LoadLevelAsync(currentLevelId);
        }
    }
} 