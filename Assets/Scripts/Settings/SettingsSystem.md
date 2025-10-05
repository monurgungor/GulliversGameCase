# SettingsSystem

## Overview

SettingsSystem is a Unity ScriptableObject-based configuration system that manages game data and visual configurations through asset files. It provides centralized storage for game parameters including letter scoring, visual themes, and UI component settings that can be easily modified in the Unity editor without code changes.

## Why Needed

The system is essential for maintaining configurable game data and visual settings that designers and developers can modify without rebuilding the game. It provides efficient lookup mechanisms for letter scoring in word games and centralizes visual configuration data for UI components. Without it, game parameters would be hardcoded, making balance changes and visual updates difficult and requiring programmer intervention for every adjustment.

## Folder Structure

```
Settings/
├── LetterSettings.cs                # Letter scoring configuration ScriptableObject
├── LetterSettings.asset            # Letter scoring data asset
├── VisualSettings.cs               # Visual configuration ScriptableObject
└── VisualSettings.asset            # Visual configuration data asset
```

## Abstraction

### **Data Structures**

#### **LetterSettings**
A ScriptableObject that manages letter scoring for word puzzle gameplay, providing fast O(1) lookup for letter scores.

```csharp
[CreateAssetMenu(fileName = "LetterSettings", menuName = "Settings/LetterSettings")]
public class LetterSettings : SerializedScriptableObject
{
    [SerializeField] private LetterScoreCategory[] letterCategories;    // Categories of letters with their scores
    private Dictionary<char, int> letterScoreLookup;                    // Fast lookup dictionary

    // Core Methods
    public int GetLetterScore(char letter);           // Get score for a letter (O(1) lookup)
    public bool HasLetterScore(char letter);          // Check if a letter exists in the score system
    public Dictionary<char, int> GetAllLetterScores(); // Get all letters and their scores (for debugging)
}
```

#### **LetterScoreCategory**
A data structure that groups letters by their point values for easier configuration in the Unity inspector.

```csharp
[System.Serializable]
public class LetterScoreCategory
{
    [SerializeField] private string letters;    // String of letters in this category
    [SerializeField] private int score;         // Point value for letters in this category

    public string Letters => letters;
    public int Score => score;
}
```

#### **VisualSettings**
A ScriptableObject that stores visual configuration data for tile-based UI components including sprites and colors.

```csharp
[CreateAssetMenu(fileName = "VisualSettings", menuName = "Settings/VisualSettings")]
public class VisualSettings : SerializedScriptableObject
{
    [field:SerializeField] public Sprite openedSprite { get; private set; }     // Sprite for clickable tiles
    [field:SerializeField] public Color openedColor { get; private set; }      // Color for clickable tiles
    [field:SerializeField] public Sprite closedSprite { get; private set; }    // Sprite for potentially clickable tiles
    [field:SerializeField] public Color closedColor { get; private set; }      // Color for blocked tiles
}
```

### **Usage Examples**

#### **Getting Letter Score**
```csharp
// Get score for a specific letter
var letterSettings = /* reference to LetterSettings asset */;
int score = letterSettings.GetLetterScore('A'); // Returns 1 based on default configuration
```

#### **Checking Letter Validity**
```csharp
// Check if a letter has a score assigned
var letterSettings = /* reference to LetterSettings asset */;
bool isValid = letterSettings.HasLetterScore('Q'); // Returns true if 'Q' has a score
```

#### **Accessing Visual Configuration**
```csharp
// Access visual settings for tile components
var visualSettings = /* reference to VisualSettings asset */;
tileImage.sprite = visualSettings.openedSprite;
tileImage.color = visualSettings.openedColor;
```

## System Architecture and Data Flow

### Configuration Loading
1. **Asset Loading**: ScriptableObject assets are loaded at runtime through Unity's asset system
2. **Initialization**: LetterSettings initializes lookup dictionary on first access for O(1) performance
3. **Access**: Components reference settings assets directly for configuration data

### Letter Score Lookup Process
1. **Dictionary Initialization**: LetterScoreCategory arrays are processed into a fast lookup dictionary
2. **Lazy Loading**: Dictionary is initialized on first access or when asset is enabled
3. **Fast Retrieval**: O(1) lookup provides efficient score retrieval during gameplay

## Internal Dependencies

- **CoreGameplay.Rack** - Uses visual settings for rack slot appearance configuration
- **CoreGameplay.Components** - TileSlotComponent uses visual settings for tile rendering
- **DataSO.RackDataSO** - Related visual settings systems that extend the base pattern

## External Dependencies

- **Sirenix.OdinInspector** - Provides enhanced inspector attributes for better editor experience
- **Unity.Serialization** - Unity's serialization system for ScriptableObject persistence