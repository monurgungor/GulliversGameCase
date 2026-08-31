# LevelsSystem

## Overview

LevelsSystem is a comprehensive level management system that handles all level-related operations including loading, discovery, progression tracking, and gameplay coordination. It manages level data from JSON files, tracks player progress and scores, handles level unlocking mechanics, and coordinates with save management and scene loading systems to provide a seamless level-based gameplay experience.

## Why Needed

The system is essential for providing structured gameplay progression and content management. It enables players to progress through predefined levels, tracks their achievements and high scores, manages level unlocking mechanics, and provides a foundation for level-based game modes. Without it, the game would lack organized content progression and player achievement tracking.

## Folder Structure

```
Levels/
├── LevelController.cs                   # Main controller managing level operations, progression, and player data
├── LevelData.cs                        # Data structures for level information and player progress
└── LevelLoader.cs                      # Handles level loading from JSON files and tile placement coordination
```

## Abstraction

### **Services**

#### **LevelController**

**Main Purpose**: Central service that manages all level-related operations including discovery, loading, progression tracking, and player data coordination.

```csharp
public class LevelController : MonoBehaviour
{
    // Core Properties
    public static Action<int> OnLevelRequested;     // Event for level loading requests

    // Core Methods
    public void LoadAllLevels();                    // Discovers and loads all available levels
    public List<LevelData> GetAvailableLevels();    // Returns list of all discovered levels
    public bool LevelExists(int levelId);           // Checks if a specific level exists
    public bool IsLevelUnlocked(int levelId);       // Checks if level is unlocked for player
    public int GetHighScoreForLevel(int levelId);   // Returns player's high score for level
    public string GetLevelTitle(int levelId);       // Returns display title for level
    public void CompleteLevel(int levelId, int score); // Handles level completion and progression
    public void LoadGameScene(int levelId);         // Loads game scene for specific level
    public void RefreshLevelSlots();               // Refreshes level data from disk
}
```

#### **LevelLoader**

**Main Purpose**: Handles the technical aspects of loading level data from JSON files and coordinating with tile placement systems for gameplay setup.

```csharp
public class LevelLoader : MonoBehaviour
{
    // Core Properties
    public static Action<int> OnLevelLoaded;        // Event fired when level is successfully loaded

    // Core Methods
    public async void LoadLevelAsync(int levelId);  // Asynchronously loads level data and places tiles
    public void LoadLevel(int levelId);             // Synchronous level loading (legacy)
    public void ReloadCurrentLevel();               // Reloads the currently active level
}
```

### **Data Structures**

#### **LevelData**

Primary data structure representing a game level with player progress information.

```csharp
public class LevelData
{
    public int levelId;         // Unique identifier for the level
    public string levelTitle;   // Display name for the level
    public int highScore;       // Player's best score for this level
    public bool isUnlocked;     // Whether player has access to this level

    public LevelData(int levelId, string levelTitle, int highScore = 0, bool isUnlocked = false);
}
```

#### **PlayerLevelData**

Simplified data structure for player-specific level progress information.

```csharp
public class PlayerLevelData
{
    public int highScore;       // Player's best score
    public bool isUnlocked;     // Level unlock status

    public PlayerLevelData(int highScore = 0, bool isUnlocked = false);
}
```

#### **LevelJsonData**

Data structure for parsing level configuration from JSON files.

```csharp
public class LevelJsonData
{
    public string title;        // Level display title from JSON
    public TileData[] tiles;    // Array of tile placement data
}
```

### **Usage Examples**

#### **Loading and Displaying Levels**

```csharp
// Get level controller and load all available levels
var levelController = FindObjectOfType<LevelController>();
levelController.LoadAllLevels();

// Get available levels for UI display
List<LevelData> availableLevels = levelController.GetAvailableLevels();
foreach (var level in availableLevels)
{
    Debug.Log($"Level {level.levelId}: {level.levelTitle} - Unlocked: {level.isUnlocked}");
}
```

#### **Level Progression Management**

```csharp
// Check if player can access a level
if (levelController.IsLevelUnlocked(5))
{
    // Load the level
    LevelController.OnLevelRequested?.Invoke(5);
}

// Complete a level and update progress
levelController.CompleteLevel(3, 1500); // Level 3 completed with score 1500
```

#### **Level Loading Coordination**

```csharp
// Subscribe to level loading events
LevelLoader.OnLevelLoaded += OnLevelLoadComplete;

// Load a specific level
var levelLoader = FindObjectOfType<LevelLoader>();
levelLoader.LoadLevelAsync(2);

void OnLevelLoadComplete(int levelId)
{
    Debug.Log($"Level {levelId} loaded successfully");
}
```

## System Architecture and Data Flow

### Level Discovery and Loading Flow

1. **Level Discovery**: LevelController scans Resources/levels folder for JSON files with level_ prefix
2. **Data Parsing**: JSON files are parsed to extract level metadata (title, tile data)
3. **Progress Integration**: Player progress data is merged with level information
4. **Level Registration**: Complete level data is stored in availableLevels list
5. **UI Coordination**: Level data is provided to UI systems for display

### Level Gameplay Flow

1. **Level Request**: Player selects level through UI, triggering OnLevelRequested event
2. **Validation**: LevelController validates level exists and is unlocked
3. **Scene Loading**: SceneController loads game scene with level ID stored in PlayerPrefs
4. **Level Loading**: LevelLoader reads level ID from PlayerPrefs and loads corresponding JSON
5. **Tile Placement**: TileData is processed and passed to TilePlacer for game setup
6. **Game Start**: OnLevelLoaded event notifies systems that level is ready for gameplay

### Progress Management

1. **Level Completion**: CompleteLevel method updates high scores and unlocks next level
2. **Data Persistence**: Player progress is synchronized with SaveManager
3. **Unlock Logic**: Sequential level unlocking (completing level N unlocks N+1)
4. **Score Tracking**: High scores are maintained per level and persisted

## Internal Dependencies

- **SaveManager** - Manages persistent player data including level progress and scores
- **SceneController** - Handles scene transitions for level loading and main menu navigation
- **TilePlacer** - Receives processed tile data for game board setup
- **PlayerData** - Stores and manages player-specific level progress information
- **LetterSettings** - Provides letter scoring information for tile data processing

## External Dependencies

- **Newtonsoft.Json** - JSON serialization and deserialization for level data files
- **Zenject** - Dependency injection for service resolution and lifecycle management

---