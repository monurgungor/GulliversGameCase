using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

/// <summary>
/// Game state information for deadlock detection
/// </summary>
public struct GameStateInfo
{
    public HashSet<int> availableTileIds;
    public Dictionary<int, int> childBlockedCounts;
    public int remainingSlots;
    public string currentWord;
    
    public GameStateInfo(HashSet<int> availableTileIds, Dictionary<int, int> childBlockedCounts, 
                        int remainingSlots, string currentWord)
    {
        this.availableTileIds = new HashSet<int>(availableTileIds);
        this.childBlockedCounts = new Dictionary<int, int>(childBlockedCounts);
        this.remainingSlots = remainingSlots;
        this.currentWord = currentWord;
    }
}

/// <summary>
/// Deadlock detection result information
/// </summary>
public struct DeadlockResult
{
    public bool isDeadlocked;
    public bool allTilesUsed;
    public int possibleWordsCount;
    public List<string> sampleValidWords;
    
    public DeadlockResult(bool isDeadlocked, bool allTilesUsed, int possibleWordsCount, List<string> sampleValidWords = null)
    {
        this.isDeadlocked = isDeadlocked;
        this.allTilesUsed = allTilesUsed;
        this.possibleWordsCount = possibleWordsCount;
        this.sampleValidWords = sampleValidWords ?? new List<string>();
    }
}

/// <summary>
/// Decoupled deadlock detection system for tile-based word games
/// Uses dependency injection to access game systems without tight coupling
/// </summary>
public class DeadlockDetector : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private int maxTraversalDepth = 7;
    [SerializeField] private int maxCacheSize = 1000;
    [SerializeField] private bool enableMemoization = true;
    [SerializeField] private bool enableDebugLogging = false;
    
    [Inject] private WordChecker wordChecker;
    private WordDictionary wordDictionary;
    
    private Dictionary<string, DeadlockResult> deadlockCache = new Dictionary<string, DeadlockResult>();
    private Queue<string> cacheKeys = new Queue<string>();


    private void Start()
    {
        if (enableDebugLogging)
        {
            Debug.Log("DeadlockDetector: Initialized with memoization=" + enableMemoization);
        }
        
        wordDictionary = wordChecker.GetWordDictionary();
    }
    
    /// <summary>
    /// Main entry point for deadlock detection
    /// </summary>
    /// <param name="gameState">Current game state information</param>
    /// <param name="tileData">Dictionary of all tile data for dependency resolution</param>
    /// <param name="checkWinCondition">Whether to check for win condition (default: true for backward compatibility)</param>
    /// <returns>Deadlock detection result</returns>
    public DeadlockResult CheckForDeadlock(GameStateInfo gameState, Dictionary<int, TileData> tileData, bool checkWinCondition = true)
    {
        if (wordDictionary == null)
        {
            Debug.LogError("DeadlockDetector: WordDictionary not injected!");
            return new DeadlockResult(true, false, 0);
        }
        
        if (checkWinCondition)
        {
            bool allTilesUsed = gameState.availableTileIds.Count == 0;
            if (allTilesUsed)
            {
                var completionResult = new DeadlockResult(false, true, 0);
                return completionResult;
            }
        }
        
        string cacheKey = GenerateCacheKey(gameState);
        
        if (enableMemoization && deadlockCache.ContainsKey(cacheKey))
        {
            if (enableDebugLogging)
            {
                Debug.Log($"DeadlockDetector: Cache hit for state {cacheKey}");
            }
            return deadlockCache[cacheKey];
        }
        
        DeadlockResult result = PerformDeadlockDetection(gameState, tileData);
        
        if (enableMemoization)
        {
            CacheResult(cacheKey, result);
        }
        
        GameEvents.TriggerDeadlockDetected(result);
        
        if (enableDebugLogging)
        {
            string sampleWordsText = result.sampleValidWords != null && result.sampleValidWords.Count > 0 
                ? $"Sample words: [{string.Join(", ", result.sampleValidWords)}]" 
                : "No sample words found";
            
            Debug.Log($"DeadlockDetector: Result - Deadlocked: {result.isDeadlocked}, " +
                     $"Possible words count: {result.possibleWordsCount}, {sampleWordsText}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Core deadlock detection algorithm
    /// </summary>
    private DeadlockResult PerformDeadlockDetection(GameStateInfo gameState, Dictionary<int, TileData> tileData)
    {
        List<string> validWords = new List<string>();
        HashSet<int> clickableTiles = GetClickableTiles(gameState, tileData);
        
        if (clickableTiles.Count == 0)
        {
            return new DeadlockResult(true, false, 0);
        }
        
        bool foundValidWord = TraverseAllPossiblePaths(
            gameState, 
            tileData, 
            clickableTiles, 
            gameState.currentWord, 
            0, 
            validWords
        );
        
        return new DeadlockResult(!foundValidWord, false, validWords.Count, validWords.Take(5).ToList());
    }
    
    /// <summary>
    /// Get all currently clickable tiles based on game state
    /// </summary>
    private HashSet<int> GetClickableTiles(GameStateInfo gameState, Dictionary<int, TileData> tileData)
    {
        HashSet<int> clickableTiles = new HashSet<int>();
        
        foreach (int tileId in gameState.availableTileIds)
        {
            bool isBlocked = gameState.childBlockedCounts.ContainsKey(tileId) && 
                           gameState.childBlockedCounts[tileId] > 0;
            
            if (!isBlocked)
            {
                clickableTiles.Add(tileId);
            }
        }
        
        return clickableTiles;
    }
    
    /// <summary>
    /// Recursive traversal of all possible word formation paths
    /// </summary>
    private bool TraverseAllPossiblePaths(
        GameStateInfo gameState, 
        Dictionary<int, TileData> tileData,
        HashSet<int> clickableTiles, 
        string currentWord, 
        int depth, 
        List<string> foundWords)
    {
        if (depth >= maxTraversalDepth)
        {
            return false;
        }
        
        if (!string.IsNullOrEmpty(currentWord) && wordChecker.GetWordDictionary().IsValidWord(currentWord))
        {
            foundWords.Add(currentWord);
            return true;
        }
        
        if (gameState.remainingSlots <= 0 || 
            (!string.IsNullOrEmpty(currentWord) && !wordDictionary.HasValidPrefix(currentWord)))
        {
            return false;
        }
        
        foreach (int tileId in clickableTiles)
        {
            if (!tileData.ContainsKey(tileId)) continue;
            
            TileData tile = tileData[tileId];
            string newWord = currentWord + tile.Character;
            
            if (!wordDictionary.HasValidPrefix(newWord))
            {
                continue;
            }
            
            GameStateInfo newGameState = SimulateTileSelection(gameState, tile, tileData);
            HashSet<int> newClickableTiles = GetClickableTiles(newGameState, tileData);
            
            bool foundValidPath = TraverseAllPossiblePaths(
                newGameState, 
                tileData, 
                newClickableTiles, 
                newWord, 
                depth + 1, 
                foundWords
            );
            
            if (foundValidPath)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Simulate tile selection and return new game state
    /// </summary>
    private GameStateInfo SimulateTileSelection(GameStateInfo currentState, TileData selectedTile, Dictionary<int, TileData> tileData)
    {
        HashSet<int> newAvailableTiles = new HashSet<int>(currentState.availableTileIds);
        Dictionary<int, int> newChildBlockedCounts = new Dictionary<int, int>(currentState.childBlockedCounts);
        
        newAvailableTiles.Remove(selectedTile.Id);
        
        foreach (int childId in selectedTile.Children)
        {
            if (newChildBlockedCounts.ContainsKey(childId))
            {
                newChildBlockedCounts[childId]--;
                if (newChildBlockedCounts[childId] <= 0)
                {
                    newChildBlockedCounts.Remove(childId);
                }
            }
        }
        
        return new GameStateInfo(
            newAvailableTiles,
            newChildBlockedCounts,
            currentState.remainingSlots - 1,
            currentState.currentWord + selectedTile.Character
        );
    }
    
    /// <summary>
    /// Generate cache key for memoization
    /// </summary>
    private string GenerateCacheKey(GameStateInfo gameState)
    {
        var sortedTiles = gameState.availableTileIds.OrderBy(x => x).ToArray();
        var sortedBlocked = gameState.childBlockedCounts.OrderBy(x => x.Key)
            .Select(x => $"{x.Key}:{x.Value}").ToArray();
        
        return $"{string.Join(",", sortedTiles)}|{string.Join(",", sortedBlocked)}|{gameState.remainingSlots}|{gameState.currentWord}";
    }
    
    /// <summary>
    /// Cache result with LRU eviction
    /// </summary>
    private void CacheResult(string key, DeadlockResult result)
    {
        if (deadlockCache.Count >= maxCacheSize)
        {
            string oldestKey = cacheKeys.Dequeue();
            deadlockCache.Remove(oldestKey);
        }
        
        deadlockCache[key] = result;
        cacheKeys.Enqueue(key);
    }
    
    /// <summary>
    /// Clear memoization cache
    /// </summary>
    public void ClearCache()
    {
        deadlockCache.Clear();
        cacheKeys.Clear();
        
        if (enableDebugLogging)
        {
            Debug.Log("DeadlockDetector: Cache cleared");
        }
    }
    
    /// <summary>
    /// Get cache statistics for debugging
    /// </summary>
    public (int size, int maxSize, float hitRate) GetCacheStats()
    {
        return (deadlockCache.Count, maxCacheSize, 0f); // Hit rate calculation would need additional tracking
    }
    
}