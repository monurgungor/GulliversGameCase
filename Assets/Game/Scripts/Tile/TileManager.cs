using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(TilePlacer))][RequireComponent(typeof(DeadlockDetector))][RequireComponent(typeof(TileClickHandler))]
public class TileManager : MonoBehaviour
{
    private TilePlacer tilePlacer;
    private DeadlockDetector deadlockDetector;

    [Inject] private WordChecker wordChecker;
    [Inject] private VisualSettings visualSettings;
    [Inject] private ScoreManager scoreManager;
    [Inject] private TileAnimationManager tileAnimationManager;

    private Dictionary<int, Tile> managedTiles = new Dictionary<int, Tile>();
    private HashSet<int> board = new HashSet<int>();
    private List<TileNode> selectedTiles = new List<TileNode>();
    private Dictionary<int, int> childBlockedByCount = new Dictionary<int, int>();

    private string currentWord = "";


    private void OnEnable()
    {
        TileClickHandler.OnTileClicked += HandleTileClicked;

        WordActions.OnWordUndo += HandleUndo;
        WordActions.OnWordUndoWithType += HandleUndoWithType;
    }

    private void OnDisable()
    {
        TileClickHandler.OnTileClicked -= HandleTileClicked;

        WordActions.OnWordUndo -= HandleUndo;
        WordActions.OnWordUndoWithType -= HandleUndoWithType;
    }

    private void Start()
    {
        tilePlacer = GetComponent<TilePlacer>();
        deadlockDetector = GetComponent<DeadlockDetector>();

        if (scoreManager == null)
        {
            Debug.LogError("TileManager: ScoreManager not found in scene!");
        }

        UpdateUndoAvailability();
    }


    /// <summary>
    /// Sets the tiles to be managed by TileManager (called by TilePlacer after spawning)
    /// </summary>
    /// <param name="tiles">Dictionary of tiles to manage</param>
    public void SetManagedTiles(Dictionary<int, Tile> tiles)
    {
        managedTiles = new Dictionary<int, Tile>(tiles);

        InitializeBoard();
    }

    /// <summary>
    /// Initialize board state and child dependency tracking
    /// </summary>
    private void InitializeBoard()
    {
        board.Clear();
        childBlockedByCount.Clear();
        selectedTiles.Clear();
        currentWord = "";

        foreach (var tile in managedTiles.Values)
        {
            board.Add(tile.TileData.Id);
        }

        foreach (var tile in managedTiles.Values)
        {
            foreach (int childId in tile.TileData.Children)
            {
                if (managedTiles.ContainsKey(childId))
                {
                    if (!childBlockedByCount.ContainsKey(childId))
                        childBlockedByCount[childId] = 0;
                    childBlockedByCount[childId]++;
                }
            }
        }

        UpdateAllTilesState();
    }

    /// <summary>
    /// Handles tile click events from TileClickHandler
    /// </summary>
    /// <param name="clickedTile">The clicked tile</param>
    private void HandleTileClicked(Tile clickedTile)
    {

        ProcessTileClick(clickedTile);
    }

    /// <summary>
    /// Handles undo events from WordActions (backwards compatibility)
    /// </summary>
    private void HandleUndo()
    {
        Undo(false);
    }

    /// <summary>
    /// Handles undo events with type from WordActions
    /// </summary>
    /// <param name="isHold">True for undo all, false for undo last</param>
    private void HandleUndoWithType(bool isHold)
    {
        Undo(isHold);
    }

    /// <summary>
    /// Process tile click with Mahjong-like logic
    /// </summary>
    /// <param name="clickedTile">The tile that was clicked</param>
    private void ProcessTileClick(Tile clickedTile)
    {
        if (!IsTileClickable(clickedTile))
        {
            return;
        }

        SelectTile(clickedTile);
    }

    /// <summary>
    /// Check if a tile is currently clickable
    /// </summary>
    /// <param name="tile">The tile to check</param>
    /// <returns>True if tile is clickable</returns>
    private bool IsTileClickable(Tile tile)
    {
        int tileId = tile.TileData.Id;

        if (!board.Contains(tileId))
            return false;

        if (childBlockedByCount.ContainsKey(tileId) && childBlockedByCount[tileId] > 0)
            return false;


        return true;
    }

    /// <summary>
    /// Select a tile and update game state
    /// </summary>
    /// <param name="tile">The tile to select</param>
    private void SelectTile(Tile tile)
    {
        if (wordChecker != null)
        {
            if (wordChecker.AreSlotsFull()) return;
            SlotInfo slotInfo = wordChecker.GetAvailableSlot();

            if (!slotInfo.IsAvailable)
            {
                return;
            }

            wordChecker.AddCharacter(tile.TileData.Character);

            if (tileAnimationManager != null)
            {
                tileAnimationManager.AnimateSelect(tile, slotInfo.Position);
            }
            else
            {
                tile.MovePosition(slotInfo.Position);
            }
        }

        int tileId = tile.TileData.Id;

        // Remove from board
        board.Remove(tileId);

        // Add to selected tiles
        TileNode tileNode = new TileNode(tile);
        selectedTiles.Add(tileNode);

        // Update current word
        currentWord += tile.TileData.Character;

        // Unlock children
        foreach (int childId in tile.TileData.Children)
        {
            if (childBlockedByCount.ContainsKey(childId))
            {
                childBlockedByCount[childId]--;
            }
        }

        // Update tile states
        UpdateAllTilesState();

        // Update undo availability
        UpdateUndoAvailability();

    }

    /// <summary>
    /// Undo tiles based on hold state
    /// </summary>
    /// <param name="isHold">If true, undo all tiles. If false, undo only last tile</param>
    public void Undo(bool isHold = false)
    {
        if (selectedTiles.Count == 0)
            return;

        int tilesToUndo;

        if (isHold)
        {
            tilesToUndo = selectedTiles.Count;
        }
        else
        {
            tilesToUndo = 1;
        }

        var tilesToUndoList = selectedTiles.Skip(selectedTiles.Count - tilesToUndo).ToList();

        selectedTiles.RemoveRange(selectedTiles.Count - tilesToUndo, tilesToUndo);

        if (wordChecker != null)
        {
            for (int i = 0; i < tilesToUndoList.Count; i++)
            {
                wordChecker.RemoveLastCharacter();
            }
        }

        foreach (var tileNode in tilesToUndoList)
        {
            Tile tile = tileNode.Tile;
            int tileId = tile.TileData.Id;

            board.Add(tileId);

            foreach (int childId in tile.TileData.Children)
            {
                if (!childBlockedByCount.ContainsKey(childId))
                    childBlockedByCount[childId] = 0;
                childBlockedByCount[childId]++;
            }

            // Animate tile back to original position
            if (tileAnimationManager != null)
            {
                tileAnimationManager.AnimateUndo(tile);
            }
            else
            {
                tile.MoveLocalPosition(tile.TileData.Position);
            }

        }

        // Rebuild current word
        currentWord = string.Concat(selectedTiles.Select(t => t.TileData.Character));

        // Update tile states
        UpdateAllTilesState();

        // Update undo availability
        UpdateUndoAvailability();
    }

    /// <summary>
    /// Update state for all tiles
    /// </summary>
    private void UpdateAllTilesState()
    {
        foreach (var tile in managedTiles.Values)
        {
            // Tile state hesapla
            TileState state = CalculateTileState(tile);

            // State'i tile'a uygula
            tile.SetState(state, visualSettings);
        }
    }

    /// <summary>
    /// Calculate visual state for a tile
    /// </summary>
    /// <param name="tile">The tile to calculate state for</param>
    /// <returns>Visual state of the tile</returns>
    private TileState CalculateTileState(Tile tile)
    {
        int tileId = tile.TileData.Id;

        if (IsSelectedTile(tileId))
            return TileState.Clickable;

        if (IsTileClickable(tile))
            return TileState.Clickable;

        if (IsTilePotentiallyClickable(tileId))
            return TileState.Potential;

        return TileState.Blocked;
    }

    /// <summary>
    /// Check if a tile is currently selected
    /// </summary>
    /// <param name="tileId">ID of the tile to check</param>
    /// <returns>True if tile is selected</returns>
    private bool IsSelectedTile(int tileId)
    {
        foreach (var tileNode in selectedTiles)
        {
            if (tileNode.Tile.TileData.Id == tileId)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check if a tile can potentially be clickable (can be opened by current clickable tiles)
    /// </summary>
    /// <param name="tileId">ID of the tile to check</param>
    /// <returns>True if tile can be opened by clickable tiles</returns>
    private bool IsTilePotentiallyClickable(int tileId)
    {
        // Bir karonun potansiyel olması için, onu üstten bloklayan
        // ebeveynlerinden en az BİR tanesinin tıklanabilir olması yeterlidir.
        foreach (var tile in managedTiles.Values)
        {
            if (tile.TileData.Children.Contains(tileId))
            {
                // Bu tile, aradığımız tile'ın ebeveyni.
                // Eğer bu ebeveyn tıklanabilir ise, aradığımız tile potansiyeldir.
                if (IsTileClickable(tile))
                {
                    return true; // Tıklanabilir bir ebeveyn bulundu, daha fazla aramaya gerek yok.
                }
            }
        }

        // Tıklanabilir hiçbir ebeveyn bulunamadı.
        return false;
    }




    /// <summary>
    /// Gets a specific managed tile by ID
    /// </summary>
    /// <param name="id">The tile ID</param>
    /// <returns>The tile if found, null otherwise</returns>
    public Tile GetTileById(int id)
    {
        return managedTiles.TryGetValue(id, out Tile tile) ? tile : null;
    }

    /// <summary>
    /// Gets all managed tiles
    /// </summary>
    /// <returns>Dictionary of all managed tiles</returns>
    public Dictionary<int, Tile> GetManagedTiles()
    {
        return managedTiles;
    }

    /// <summary>
    /// Gets the TilePlacer component
    /// </summary>
    /// <returns>The TilePlacer component</returns>
    public TilePlacer GetTilePlacer()
    {
        return tilePlacer;
    }

    /// <summary>
    /// Gets the current word being formed
    /// </summary>
    /// <returns>Current word string</returns>
    public string GetCurrentWord()
    {
        return currentWord;
    }

    /// <summary>
    /// Gets the selected tiles
    /// </summary>
    /// <returns>List of selected tile nodes</returns>
    public List<TileNode> GetSelectedTiles()
    {
        return new List<TileNode>(selectedTiles);
    }


    /// <summary>
    /// Clear all slots and reset word
    /// </summary>
    public void ClearAllSlots()
    {
        if (wordChecker != null)
        {
            wordChecker.ClearAllSlots();
        }

        selectedTiles.Clear();
        currentWord = "";

        board.Clear();
        foreach (var tile in managedTiles.Values)
        {
            board.Add(tile.TileData.Id);
            tile.MovePosition(tile.TileData.Position);
        }

        InitializeBoard();

        UpdateUndoAvailability();
    }

    /// <summary>
    /// Clear selected tiles from managed tiles and board
    /// </summary>
    public void ClearSelectedTiles()
    {
        foreach (var tileNode in selectedTiles)
        {
            if (tileNode.Tile != null)
            {
                int tileId = tileNode.Tile.TileData.Id;
                managedTiles.Remove(tileId);
                board.Remove(tileId);
            }
        }

        selectedTiles.Clear();
        currentWord = "";

        UpdateAllTilesState();

        UpdateUndoAvailability();
    }

    /// <summary>
    /// Check for deadlock after word submission
    /// </summary>
    public void CheckForDeadlock()
    {
        if (deadlockDetector == null)
        {
            Debug.LogError("TileManager: DeadlockDetector not injected!");
            return;
        }

        if (AreAllTilesConsumed())
        {
            Debug.Log("TileManager: All tiles consumed - triggering game completion!");
            GameEvents.TriggerGameCompleted();
            return;
        }

        GameStateInfo gameState = CreateCurrentGameState();

        Dictionary<int, TileData> tileDataDict = CreateTileDataDictionary();

        DeadlockResult result = deadlockDetector.CheckForDeadlock(gameState, tileDataDict, false);

        Debug.Log($"TileManager: Deadlock check result - Deadlocked: {result.isDeadlocked}, " +
                 $"Board tiles: {board.Count}, Selected tiles: {selectedTiles.Count}, " +
                 $"Managed tiles: {managedTiles.Count}, Possible words: {result.possibleWordsCount}");

        if (result.isDeadlocked)
        {
            HandleDeadlockPenalty();
        }
    }

    /// <summary>
    /// Create current game state for deadlock detection
    /// </summary>
    private GameStateInfo CreateCurrentGameState()
    {
        int remainingSlots = wordChecker != null ?
            (7 - wordChecker.GetCurrentSlotIndex()) : 7;

        return new GameStateInfo(
            new HashSet<int>(board),
            new Dictionary<int, int>(childBlockedByCount),
            remainingSlots,
            currentWord
        );
    }

    /// <summary>
    /// Get total remaining tiles (board + selected but not submitted)
    /// </summary>
    public int GetTotalRemainingTiles()
    {
        return board.Count + selectedTiles.Count;
    }

    /// <summary>
    /// Check if all tiles are actually consumed (submitted, not just selected)
    /// </summary>
    public bool AreAllTilesConsumed()
    {   
        return managedTiles.Count == 0;
    }

    /// <summary>
    /// Create tile data dictionary for deadlock detection
    /// </summary>
    private Dictionary<int, TileData> CreateTileDataDictionary()
    {
        Dictionary<int, TileData> tileDataDict = new Dictionary<int, TileData>();

        foreach (var kvp in managedTiles)
        {
            tileDataDict[kvp.Key] = kvp.Value.TileData;
        }

        return tileDataDict;
    }

    /// <summary>
    /// Update undo availability and notify UI
    /// </summary>
    private void UpdateUndoAvailability()
    {
        bool canUndo = selectedTiles.Count > 0;
        WordActions.OnUndoAvailabilityChanged?.Invoke(canUndo);
    }

    /// <summary>
    /// Handle deadlock penalty - subtract 10 points per remaining tile
    /// </summary>
    private void HandleDeadlockPenalty()
    {
        int remainingTiles = board.Count;
        int penalty = remainingTiles * 10;

        if (scoreManager != null && penalty > 0)
        {
            scoreManager.SubtractScore(penalty);
            Debug.Log($"TileManager: Deadlock penalty applied - {remainingTiles} tiles × 10 = {penalty} points subtracted");
        }
        else if (penalty == 0)
        {
            Debug.Log("TileManager: No penalty applied - no remaining tiles");
        }
        else
        {
            Debug.LogError("TileManager: Cannot apply penalty - ScoreManager not found");
        }
    }
}

/// <summary>
/// Wrapper class for selected tiles to preserve additional data
/// </summary>
public class TileNode
{
    public Tile Tile { get; private set; }
    public TileData TileData => Tile.TileData;
    
    public TileNode(Tile tile)
    {
        Tile = tile;
    }
}
