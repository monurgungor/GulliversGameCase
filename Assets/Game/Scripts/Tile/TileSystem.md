# TileSystem

## Overview

TileSystem is a comprehensive tile management system that handles all aspects of tile behavior in the word puzzle game. It manages tile creation, placement, visual states, user interactions, animations, and game logic including Mahjong-like layered blocking mechanics where tiles can be stacked and only accessible tiles can be selected for word formation.

## Why Needed

The system is essential for the core gameplay mechanics of the word puzzle game. It provides centralized management of tile lifecycles, implements complex game rules for tile accessibility, and ensures consistent visual feedback. Without it, the game would lack the fundamental mechanics for tile-based word formation, visual state management, and user interaction handling that define the core gameplay experience.

## Folder Structure

```
Tile/
├── Tile.cs                       # Main tile component with state management
├── TileData.cs                   # Data structure for tile information
├── TileManager.cs                # Central tile orchestration and game logic
├── TileVisual.cs                 # Visual representation and state rendering
├── TileAnimationManager.cs       # Animation control and smoothing
├── TileClickHandler.cs           # Input handling and click detection
└── TilePlacer.cs                 # Tile spawning and positioning logic
```

## Abstraction

### **Interfaces**

#### **IClickable**
**Main Purpose**: Provides a standard interface for clickable game objects, ensuring consistent click handling across different interactive elements in the game.

```csharp
public interface IClickable
{
    void OnClick();        // Handles click events when the object is clicked
}
```

### **Singleton Services**

#### **TileManager**
**Main Purpose**: Central orchestrator for tile management, handling selection logic, game state tracking, and coordination with other game systems.

```csharp
public class TileManager : MonoBehaviour
{
    // Core Properties
    public Dictionary<int, Tile> GetManagedTiles();           // Access to all managed tiles
    public string GetCurrentWord();                          // Currently formed word from selected tiles
    public List<TileNode> GetSelectedTiles();                // Currently selected tile nodes

    // Core Methods
    public void SetManagedTiles(Dictionary<int, Tile> tiles); // Initialize tiles for management
    public void Undo(bool isHold = false);                   // Undo tile selection (last or all)
    public void ClearAllSlots();                             // Reset all tiles and word formation
    public void CheckForDeadlock();                          // Check for game deadlock conditions
}
```

#### **TileAnimationManager**
**Main Purpose**: Manages all tile animations including selection, undo, and submission animations with smooth transitions and event coordination.

```csharp
public class TileAnimationManager : MonoBehaviour
{
    // Core Properties
    public static event Action<Tile> OnSelectAnimationComplete;    // Animation completion events
    public static event Action<List<Tile>> OnSubmitAnimationComplete;

    // Core Methods
    public void AnimateSelect(Tile tile, Vector3 targetPosition, Action onComplete = null);  // Animate tile selection
    public void AnimateUndo(Tile tile, Action onComplete = null);                            // Animate tile return
    public void AnimateSubmit(List<Tile> tiles, Action onComplete = null);                   // Animate word submission
    public void CancelAllAnimations();                                                       // Stop all active animations
}
```

#### **TileClickHandler**
**Main Purpose**: Handles all user input for tile interactions, including touch and mouse input processing with proper raycast detection.

```csharp
public class TileClickHandler : MonoBehaviour
{
    // Core Properties
    public static System.Action<Tile> OnTileClicked;          // Event fired when a tile is clicked

    // Core Methods
    public void SetClickable(bool clickable);                 // Enable/disable click handling
    public void ResetState();                                 // Reset click handler state
}
```

#### **TilePlacer**
**Main Purpose**: Handles tile spawning, positioning, and initial setup including board centering and camera alignment for optimal gameplay view.

```csharp
public class TilePlacer : MonoBehaviour
{
    // Core Properties
    public float GetPositionScaleFactor();                    // Current position scaling factor

    // Core Methods
    public void LoadSpawnAndPlaceTiles(TileData[] tileDataArray);  // Load and place all tiles from data
    public void SetPositionScaleFactor(float newScaleFactor);     // Adjust position scaling
}
```

### **Data Structures**

#### **TileData**
Immutable data structure containing all essential information about a tile including its unique identifier, character, position, scoring value, and hierarchical relationships for the Mahjong-like blocking system.

```csharp
public struct TileData
{
    public int Id { get; private set; }           // Unique tile identifier
    public char Character { get; private set; }   // Letter displayed on tile
    public Vector3 Position { get; private set; } // Tile position in game space
    public int[] Children { get; private set; }   // IDs of tiles this tile blocks
    public int Score { get; private set; }        // Point value of the tile

    public TileData(int id, char character, Vector3 position, int[] children, int score);
}
```

#### **TileNode**
Wrapper class for selected tiles that preserves additional data during tile selection and word formation processes.

```csharp
public class TileNode
{
    public Tile Tile { get; private set; }        // Reference to the actual tile component
    public TileData TileData => Tile.TileData;    // Access to tile's data

    public TileNode(Tile tile);                   // Constructor with tile reference
}
```

#### **TileState**
Enumeration defining the possible visual and interactive states a tile can be in during gameplay.

```csharp
public enum TileState
{
    Clickable,    // Tile is accessible and can be selected
    Potential,    // Tile could become clickable when blocking tiles are removed
    Blocked       // Tile is completely blocked and inaccessible
}
```

### **Usage Examples**

#### **Tile Selection and Word Formation**
```csharp
// Get the tile manager and select a tile for word formation
var tileManager = container.Resolve<TileManager>();
string currentWord = tileManager.GetCurrentWord();
```

#### **Animation Control**
```csharp
// Animate tile selection with completion callback
var animationManager = container.Resolve<TileAnimationManager>();
animationManager.AnimateSelect(tile, targetPosition, () => Debug.Log("Animation complete"));
```

#### **Tile Placement**
```csharp
// Load and place tiles from data array
var tilePlacer = container.Resolve<TilePlacer>();
tilePlacer.LoadSpawnAndPlaceTiles(tileDataArray);
```

## System Architecture and Data Flow

### Tile Lifecycle Management
1. **Initialization**: TilePlacer spawns tiles from TileData array and positions them on the board
2. **State Management**: TileManager calculates and applies visual states based on blocking relationships
3. **User Interaction**: TileClickHandler processes input and notifies TileManager of tile selections
4. **Animation**: TileAnimationManager smoothly transitions tiles between positions and states
5. **Word Formation**: Selected tiles form words while maintaining game state consistency

### Blocking System Behavior
- **Hierarchical Blocking**: Tiles can block other tiles based on Children array relationships
- **Dynamic State Calculation**: Tile states are recalculated after each selection to reflect new accessibility
- **Potential State**: Shows players which tiles could become accessible when blocking tiles are removed

## Internal Dependencies

- **WordChecker** - Validates words and manages available slots for tile placement
- **ScoreManager** - Handles scoring when tiles are selected and submitted
- **VisualSettings** - Provides visual configuration for different tile states
- **WordActions** - Handles undo operations and UI state updates
- **GameEvents** - Triggers game completion and deadlock events
- **DeadlockDetector** - Checks for unwinnable game states

## External Dependencies

- **PrimeTween** - Animation library for smooth tile movement and transitions
- **Zenject** - Dependency injection for service resolution and component management
- **TMPro** - Text rendering for tile letters and scores

---