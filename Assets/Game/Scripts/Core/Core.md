# Core

## Overview

Core is the fundamental system that manages the central game state, scene navigation, and core game flow for Word Tiles GO. It provides essential services for game initialization, scene transitions, game completion handling, and centralized event communication between different game systems.

## Why Needed

The system is essential for providing a stable foundation for all game operations. It centralizes game state management, handles critical game flow events like completion and deadlock detection, and provides unified scene navigation capabilities. Without it, the game would lack proper state coordination, scene management, and reliable communication between different subsystems.

## Folder Structure

```
Core/
├── GameStateManager.cs              # Manages game state and completion logic
├── SceneController.cs               # Handles scene loading and transitions
├── GameEvents.cs                    # Centralized event system for game communication
├── GameEndData.cs                   # Data structure for game completion information
├── GamePlayInstaller.cs             # Dependency injection configuration for gameplay
└── UnitySerializedDictionary.cs     # Unity-compatible serializable dictionary utility
```

## Abstraction

### **Singleton Services**

#### **GameStateManager**
**Main Purpose**: Manages the overall game state, tracks scores, handles game completion events, and coordinates between level progression and score management systems.

```csharp
public class GameStateManager : MonoBehaviour
{
    // Core Properties
    private int currentLevelId;           // Currently active level identifier
    private int currentScore;             // Current game session score
    private bool isNewHighScore;          // Whether current score is a new high score

    // Core Methods
    public static event Action<GameEndData> OnGameEndProcessed;  // Event fired when game completion is processed
    public void ReturnToMainMenu();                             // Returns to main menu through level controller
    private void ProcessGameCompletion();                       // Processes game completion and creates GameEndData
}
```

#### **SceneController**
**Main Purpose**: Handles all scene loading operations, manages async scene transitions, and provides initialization logic for the game startup sequence.

```csharp
public class SceneController : MonoBehaviour
{
    // Core Properties
    public bool IsInitialized { get; }                          // Whether game initialization is complete

    // Core Methods
    public async Task InitializeGameAsync();                    // Initialize the game asynchronously
    public async Task LoadGameSceneAsync(int levelId);          // Load game scene for specific level
    public async Task ReturnToMainMenuAsync();                  // Return to main menu scene
    public async Task RestartCurrentSceneAsync();               // Restart current scene
    public string GetCurrentSceneName();                        // Get current scene name
    public bool IsMainMenuScene();                              // Check if current scene is main menu
    public bool IsGameScene();                                  // Check if current scene is game scene
}
```

#### **GameEvents**
**Main Purpose**: Provides a centralized, static event system for decoupled communication between game systems, particularly for game completion and deadlock detection events.

```csharp
public static class GameEvents
{
    // Core Events
    public static event Action OnGameCompleted;                 // Event fired when game is completed
    public static event Action<DeadlockResult> OnDeadlockDetected;  // Event fired when deadlock is detected

    // Core Methods
    public static void TriggerGameCompleted();                  // Trigger game completion event
    public static void TriggerDeadlockDetected(DeadlockResult result);  // Trigger deadlock detection event
}
```

#### **GamePlayInstaller**
**Main Purpose**: Configures dependency injection bindings for core gameplay components using Zenject container during scene initialization.

```csharp
public class GamePlayInstaller : MonoInstaller
{
    // Core Methods
    public override void InstallBindings();                     // Configure DI bindings for gameplay components
}
```

### **Data Structures**

#### **GameEndData**
Data structure that encapsulates all information about game completion, including scoring details, high score status, and completion conditions.

```csharp
public struct GameEndData
{
    public int currentScore;         // Score achieved in current session
    public int previousHighScore;    // Previous high score for comparison
    public bool isNewHighScore;      // Whether current score is a new record
    public bool isDeadlockEnd;       // Whether game ended due to deadlock
    public string levelName;         // Name/identifier of completed level
}
```

#### **UnitySerializedDictionary**
Abstract base class that provides Unity Inspector serialization support for Dictionary collections, allowing key-value pairs to be edited in Unity Editor.

```csharp
public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    // Core Methods
    void ISerializationCallbackReceiver.OnAfterDeserialize();   // Deserialize from Unity Inspector data
    void ISerializationCallbackReceiver.OnBeforeSerialize();    // Serialize to Unity Inspector data
}
```

### **Usage Examples**

#### **Game State Management**
```csharp
// Listen for game completion events
GameStateManager.OnGameEndProcessed += (gameEndData) => {
    if (gameEndData.isNewHighScore) {
        ShowHighScoreUI(gameEndData.currentScore);
    }
};

// Trigger game completion through events
GameEvents.TriggerGameCompleted();
```

#### **Scene Navigation**
```csharp
// Load a specific level
var sceneController = FindObjectOfType<SceneController>();
await sceneController.LoadGameSceneAsync(levelId: 5);

// Return to main menu
await sceneController.ReturnToMainMenuAsync();
```

#### **Event Communication**
```csharp
// Subscribe to game events
GameEvents.OnGameCompleted += OnGameCompleted;
GameEvents.OnDeadlockDetected += OnDeadlockDetected;

// Trigger deadlock detection
var deadlockResult = new DeadlockResult(isDeadlocked: true, allTilesUsed: false, possibleWordsCount: 0);
GameEvents.TriggerDeadlockDetected(deadlockResult);
```

## System Architecture and Data Flow

### Game Initialization Flow
1. **Scene Controller Start**: SceneController initializes game asynchronously on Start()
2. **Save Manager Init**: SaveManager is initialized and validated
3. **Main Menu Load**: Main menu scene is loaded with progress tracking
4. **Initialization Complete**: OnGameInitialized event is fired

### Game Completion Flow
1. **Event Trigger**: Game completion or deadlock events are triggered through GameEvents
2. **State Processing**: GameStateManager processes the completion, calculating scores and high score status
3. **Data Creation**: GameEndData structure is created with all completion information
4. **Event Broadcast**: OnGameEndProcessed event is fired with GameEndData for UI and other systems

### Scene Management
- **Async Loading**: All scene transitions use async operations with progress tracking
- **Error Handling**: Comprehensive error handling with event notifications
- **State Persistence**: Level data is persisted through PlayerPrefs during transitions

## Internal Dependencies

- **LevelController** - Used for level progression, high score tracking, and main menu navigation
- **ScoreManager** - Provides current score information for game completion processing
- **SaveManager** - Handles save data initialization during game startup
- **TileManager** - Registered in DI container for tile management
- **WordChecker** - Registered in DI container for word validation
- **TileAnimationManager** - Registered in DI container for tile animations

## External Dependencies

- **Zenject** - Dependency injection framework for service registration and resolution
- **Unity SceneManagement** - Unity's scene loading and management API
- **Unity PlayerPrefs** - Unity's persistent data storage for level information

---