# DataSystem

## Overview

DataSystem is a comprehensive data management and persistence system for Word Tiles GO that handles player progress data, save/load operations, and word dictionary management. It provides a robust event-driven architecture for saving player data automatically on application lifecycle events and manages a DAWG (Directed Acyclic Word Graph) based word dictionary for efficient word validation and lookup operations.

## Why Needed

The system is essential for maintaining game progression and providing core word validation functionality. It ensures player progress is never lost through automatic save mechanisms triggered by application focus changes, pause events, and quit operations. Without it, players would lose their level progression data, and the game would lack the word validation capabilities necessary for word puzzle gameplay mechanics.

## Folder Structure

```
Data/
├── ISaveable.cs                         # Interface for data-aware components
├── SaveManager.cs                       # Core save/load service with event system
├── PlayerData.cs                        # Player progression data structure
├── WordDictionary.cs                    # DAWG-based word validation system
└── GameInstaller.cs                     # Dependency injection configuration
```

## Abstraction

### **Interfaces**

#### **ISaveable**
Main Purpose: Defines a contract for components that need to participate in the save/load data lifecycle, allowing them to receive notifications when player data is loaded, saved, or requested.

```csharp
public interface ISaveable
{
    void OnDataLoad(PlayerData playerData);           // Called when player data is loaded
    void OnDataSave(PlayerData playerData);           // Called when player data should be saved
    void OnPlayerDataRequested(PlayerData playerData); // Called when player data is requested
}
```

### **Singleton Services**

#### **SaveManager**
Main Purpose: Manages all save and load operations for player data using JSON serialization with automatic lifecycle-based saving and event-driven architecture for notifying data-aware components.

```csharp
public class SaveManager : MonoBehaviour
{
    // Core Properties
    public bool IsDataLoaded { get; }                           // Indicates if data loading is complete
    public static event Action<PlayerData> OnDataLoaded;        // Event fired when data is loaded
    public static event Action<PlayerData> OnDataSaved;         // Event fired when data is saved

    // Core Methods
    public async Task InitializeAsync();                        // Initialize save manager asynchronously
    public async Task LoadPlayerDataAsync();                    // Load player data from file
    public void SavePlayerData();                               // Save current player data to file
    public PlayerData GetCurrentPlayerData();                   // Get current player data instance
    public bool SaveFileExists();                               // Check if save file exists
    public bool DeleteSaveFile();                               // Delete existing save file
}
```

#### **WordDictionary**
Main Purpose: Provides high-performance word validation and prefix checking using a DAWG (Directed Acyclic Word Graph) trie structure for efficient memory usage and fast lookup operations.

```csharp
public class WordDictionary
{
    // Core Properties
    public DAWGNode GetRootNode();                              // Access to trie root for traversal

    // Core Methods
    public bool IsValidWord(string word);                       // Check if word exists in dictionary
    public bool HasValidPrefix(string prefix);                  // Check if prefix has valid continuations
    public List<string> GetWordsWithPrefix(string prefix);      // Get all words starting with prefix
    public int GetWordCount();                                  // Get total number of words
    public bool IsInitialized();                               // Check if dictionary is ready
    public List<char> GetPossibleNextCharacters(string prefix); // Get valid next characters
}
```

### **Data Structures**

#### **PlayerData**
Represents the complete player progression state including level completion data, high scores, and unlock status. Uses a dictionary structure for efficient level data lookup and supports loading from external level data sources.

```csharp
public class PlayerData
{
    public PlayerLevelDataDictionary levelData;                 // Dictionary of level progression data

    // Key methods
    public int GetLevelScore(int levelId);                      // Get high score for specific level
    public void SetLevelData(int id, PlayerLevelData levelData); // Set level data for specific level
    public List<PlayerLevelData> GetAllLevelData();             // Get all level progression data
    public void CompleteLevel(int levelNumber, int score);       // Mark level as completed with score
    public bool IsLevelUnlocked(int levelNumber);               // Check if level is unlocked
    public void LoadToPlayerData(List<LevelData> datas);        // Load data from external source
}
```

#### **PlayerLevelData**
Contains progression information for a single level including completion status and highest achieved score.

```csharp
public class PlayerLevelData
{
    public int highScore;                                       // Highest score achieved on this level
    public bool isUnlocked;                                     // Whether level is accessible to player
}
```

#### **DAWGNode**
Represents a single node in the Directed Acyclic Word Graph structure used for efficient word storage and lookup with suffix compression capabilities.

```csharp
public class DAWGNode
{
    public Dictionary<char, DAWGNode> children;                 // Child nodes for each character
    public bool isEndOfWord;                                    // Indicates if this node completes a word
    public int nodeId;                                          // Unique identifier for compression

    public string GetSignature();                               // Generate signature for node comparison
}
```

### **Usage Examples**

#### **Saving Player Progress**
```csharp
// Get save manager and update player progress
var saveManager = container.Resolve<SaveManager>();
var playerData = saveManager.GetCurrentPlayerData();
playerData.CompleteLevel(5, 1250);
saveManager.SavePlayerData();
```

#### **Word Validation**
```csharp
// Validate a word using the dictionary
var wordDictionary = container.Resolve<WordDictionary>();
bool isValid = wordDictionary.IsValidWord("HELLO");
bool hasPrefix = wordDictionary.HasValidPrefix("HEL");
```

#### **Data Loading Notification**
```csharp
// Component implementing ISaveable to receive data updates
public class GameComponent : MonoBehaviour, ISaveable
{
    public void OnDataLoad(PlayerData playerData)
    {
        // Update UI or game state with loaded data
        UpdateLevelProgress(playerData.GetAllLevelData());
    }
}
```

## System Architecture and Data Flow

### Save/Load Lifecycle
1. **Initialization**: SaveManager loads existing data or creates new PlayerData instance
2. **Runtime Updates**: Game components modify PlayerData through SaveManager
3. **Automatic Saving**: System saves data on application pause, focus loss, or quit events
4. **Event Notification**: ISaveable components receive notifications about data changes

### Word Dictionary Architecture
- **DAWG Structure**: Uses trie with suffix compression for memory efficiency
- **Initialization**: Builds dictionary from TextAsset containing word list
- **Validation**: Provides O(m) lookup time where m is word length
- **Prefix Support**: Enables autocomplete and word prediction features

## Internal Dependencies

- **LevelSystem** - Provides LevelData and PlayerLevelData structures for progression tracking
- **GameStateManager** - Coordinates with save system for game state persistence

## External Dependencies

- **Newtonsoft.Json** - JSON serialization for player data persistence
- **Sirenix.Serialization** - Enhanced serialization support for complex data structures

---