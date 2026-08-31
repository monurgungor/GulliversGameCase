using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TilePlacer : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject tilePrefab;
    
    [Header("Placement Settings")]
    [SerializeField] private Transform board;
    [SerializeField] private Transform tilesParent;
    [SerializeField] private float positionScaleFactor = 0.5f; // JSON pozisyonlarını küçültmek için

    [Inject] private TileManager tileManager;
    [Inject] private TileAnimationManager tileAnimationManager;
    
    /// <summary>
    /// Loads, spawns, places tiles and notifies TileManager
    /// </summary>
    /// <param name="tileDataArray">Array of tile data to load</param>
    public void LoadSpawnAndPlaceTiles(TileData[] tileDataArray)
    {
        if (tileDataArray == null || tileDataArray.Length == 0)
        {
            return;
        }
        
        // Spawn, place and collect tiles
        Dictionary<int, Tile> tiles = SpawnAndPlaceTiles(tileDataArray);
        
        // Notify TileManager
        if (tileManager != null)
        {
            tileManager.SetManagedTiles(tiles);
        }
    }
    
    /// <summary>
    /// Spawns and places tiles from TileData array
    /// </summary>
    /// <param name="tileDataArray">Array of tile data to load</param>
    /// <returns>Dictionary of placed tiles for TileManager</returns>
    private Dictionary<int, Tile> SpawnAndPlaceTiles(TileData[] tileDataArray)
    {
        Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();
        
        // Spawn and place all tiles
        foreach (var tileData in tileDataArray)
        {
            Tile tile = SpawnAndPlaceTile(tileData);
            if (tile != null)
            {
                tiles[tileData.Id] = tile;
            }
        }
        
        // Center the board
        CenterBoard(tiles);
        
        return tiles;
    }
    
    /// <summary>
    /// Spawns and places a single tile from TileData
    /// </summary>
    /// <param name="tileData">The tile data</param>
    /// <returns>The spawned and placed tile</returns>
    private Tile SpawnAndPlaceTile(TileData tileData)
    {
        if (tilePrefab == null)
        {
            return null;
        }
        
        // Spawn the tile at origin
        Tile spawnedTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity).GetComponent<Tile>();
        
        if (spawnedTile == null)
        {
            return null;
        }
        
        // Initialize the tile with data
        spawnedTile.Initialize(tileData);
        
        // Set the tile's name for easy identification
        spawnedTile.name = $"Tile_{tileData.Id}_{tileData.Character}";
        
        // Place the tile immediately
        PlaceTile(spawnedTile, tileData);
        
        return spawnedTile;
    }
    
    /// <summary>
    /// Places a tile at the specified position
    /// </summary>
    /// <param name="tile">The tile to place</param>
    /// <param name="tileData">The tile data containing position information</param>
    private void PlaceTile(Tile tile, TileData tileData)
    {
        if (tile == null)
        {
            return;
        }
        
        // Parent'a child yap
        if (tilesParent != null)
        {
            tile.transform.SetParent(tilesParent);
        }
        
        // Local position olarak ayarla
        tile.transform.localPosition = tileData.Position;
        
        // Tile'ın adını ayarla
        tile.name = $"Tile_{tileData.Id}_{tileData.Character}";
        
        // Store original position for animations
        if (tileAnimationManager != null)
        {
            tileAnimationManager.StoreOriginalPosition(tile);
        }
    }
    
    /// <summary>
    /// Board'u ortalar - board ve kamerayı tilesParent'ın merkezine hizalar
    /// </summary>
    private void CenterBoard(Dictionary<int, Tile> tiles)
    {
        if (tiles.Count == 0) return;
        
        // Tüm tile'ların min/max değerlerini bul
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        
        foreach (var tile in tiles.Values)
        {
            if (tile != null)
            {
                Vector3 pos = tile.transform.position; // World position kullan
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }
        }
        
        // tilesParent'ın merkezini hesapla
        Vector3 tilesCenter = (min + max) * 0.5f;
        
        // Board'u tilesParent'ın merkezine taşı
        if (board != null)
        {
            board.position = tilesCenter;
        }
        
        // Kamerayı da tilesParent'ın merkezine odakla
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            cameraPosition.x = tilesCenter.x;
            cameraPosition.y = tilesCenter.y;
            mainCamera.transform.position = cameraPosition;
        }
    }
    
    
    /// <summary>
    /// Gets the current position scale factor
    /// </summary>
    /// <returns>The current position scale factor</returns>
    public float GetPositionScaleFactor()
    {
        return positionScaleFactor;
    }
    
    /// <summary>
    /// Sets the position scale factor
    /// </summary>
    /// <param name="newScaleFactor">The new position scale factor</param>
    public void SetPositionScaleFactor(float newScaleFactor)
    {
        positionScaleFactor = newScaleFactor;
    }

    /// <summary>
    /// Get the position scale factor used for tile placement
    /// </summary>
    /// <returns>Position scale factor</returns>
    public float GetScaleFactor()
    {
        return positionScaleFactor;
    }
} 