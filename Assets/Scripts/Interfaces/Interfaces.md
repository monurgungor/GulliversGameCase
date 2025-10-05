# Interfaces

## Overview

The Interfaces system provides fundamental behavioral contracts that define how different components in the Word Tiles GO game interact with each other. It establishes standardized communication patterns for click handling and data persistence operations across the game architecture.

## Why Needed

The system is essential for maintaining clean architecture and loose coupling between components. It provides standardized contracts that allow different parts of the game to interact without tight dependencies, enabling better testability, maintainability, and extensibility. Without it, components would be tightly coupled and difficult to modify or extend.

## Folder Structure

```
Interfaces/
└── IClickable.cs                    # Interface for clickable game objects
```

Related interfaces in other systems:
```
Data/
└── ISaveable.cs                     # Interface for data persistence operations
```

## Abstraction

### **Interfaces**

#### **IClickable**

**Main Purpose**: Defines a contract for game objects that can respond to user click/touch interactions, providing a standardized way to handle user input across different UI and gameplay elements.

```csharp
public interface IClickable
{
    void OnClick();                           // Handles click/touch events
}
```

#### **ISaveable**

**Main Purpose**: Defines a contract for components that need to participate in the game's data persistence system, allowing them to save and load their state as part of the player data.

```csharp
public interface ISaveable
{
    void OnDataLoad(PlayerData playerData);           // Called when player data is loaded
    void OnDataSave(PlayerData playerData);           // Called when player data should be saved
    void OnPlayerDataRequested(PlayerData playerData); // Called when player data is requested
}
```

### **Usage Examples**

#### **Click Handling**
```csharp
// Implementation in a Tile component
public class Tile : MonoBehaviour, IClickable
{
    public void OnClick()
    {
        // Handle tile click logic
        TileClickHandler.OnTileClicked?.Invoke(this);
    }
}
```

#### **Click Detection**
```csharp
// Usage in click detection system
IClickable clickable = hit2D.collider.GetComponent<IClickable>();
if (clickable != null)
{
    clickable.OnClick();
}
```

#### **Data Persistence**
```csharp
// Implementation for saveable component
public class ScoreManager : MonoBehaviour, ISaveable
{
    public void OnDataLoad(PlayerData playerData)
    {
        // Load score data from player data
    }

    public void OnDataSave(PlayerData playerData)
    {
        // Save current score to player data
    }

    public void OnPlayerDataRequested(PlayerData playerData)
    {
        // Provide current score data when requested
    }
}
```

## System Architecture and Data Flow

### Click Interaction Flow
1. **Input Detection**: TileClickHandler detects touch/mouse input
2. **Raycast Check**: Performs raycast to find clicked object
3. **Interface Resolution**: Checks if clicked object implements IClickable
4. **Action Execution**: Calls OnClick() method on the clickable component

### Data Persistence Flow
1. **Registration**: Components implementing ISaveable register with SaveManager
2. **Load Phase**: OnDataLoad() called when game data is loaded
3. **Save Phase**: OnDataSave() called when game needs to persist data
4. **Request Phase**: OnPlayerDataRequested() called for real-time data updates

## Internal Dependencies

- **TileClickHandler** - Uses IClickable for handling tile interactions
- **Tile** - Implements IClickable for click responsiveness
- **SaveManager** - Works with ISaveable components for data persistence
- **PlayerData** - Data structure used by ISaveable implementations

## External Dependencies

None

---