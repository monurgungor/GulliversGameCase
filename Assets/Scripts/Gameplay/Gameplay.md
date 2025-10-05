# Gameplay

## Overview

Gameplay is the core system that manages word formation, validation, scoring, and deadlock detection in Word Tiles GO. It handles the fundamental gameplay mechanics including character placement in slots, word dictionary validation, score calculation based on letter values, and advanced deadlock detection to ensure playable game states.

## Why Needed

The system is essential for implementing the core word puzzle gameplay mechanics. It provides centralized word validation, scoring logic, and game state analysis that ensures players can form valid words while preventing unwinnable game states. Without it, the game would lack fundamental word puzzle mechanics and could result in frustrating deadlock situations where no valid moves are possible.

## Folder Structure

```
Gameplay/
├── DeadlockDetector.cs             # Advanced deadlock detection system
├── ScoreManager.cs                 # Score tracking and management
├── WordActions.cs                  # Static event actions for word-related events
└── WordChecker.cs                  # Word validation and slot management
```

## Abstraction

### **Singleton Services**

#### **DeadlockDetector**
**Main Purpose**: Analyzes game states to detect when no valid word combinations are possible, preventing unwinnable scenarios in tile-based word games.

```csharp
public class DeadlockDetector : MonoBehaviour
{
    // Core Properties
    public int maxTraversalDepth { get; }           // Maximum search depth for performance
    public bool enableMemoization { get; }          // Cache results for optimization

    // Core Methods
    DeadlockResult CheckForDeadlock(GameStateInfo gameState, Dictionary<int, TileData> tileData, bool checkWinCondition = true);  // Main deadlock detection entry point
    void ClearCache();                              // Clear memoization cache
    (int size, int maxSize, float hitRate) GetCacheStats();  // Get cache statistics
}
```

#### **ScoreManager**
**Main Purpose**: Manages the player's current score by tracking score additions and providing score update notifications throughout the game.

```csharp
public class ScoreManager : MonoBehaviour
{
    // Core Properties
    public int CurrentScore { get; private set; }   // Current total score

    // Core Methods
    void SubtractScore(int score);                  // Subtract points from current score
    void ResetScore();                              // Reset score to zero
}
```

#### **WordChecker**
**Main Purpose**: Manages word formation through character slots, validates words against dictionary, calculates scores, and handles word submission with tile removal animations.

```csharp
public class WordChecker : MonoBehaviour
{
    // Core Properties
    public string GetCurrentWord();                 // Current word being formed
    public int GetCurrentSlotIndex();               // Current slot position

    // Core Methods
    SlotInfo GetAvailableSlot();                    // Get next available slot info
    void AddCharacter(char character);              // Add character to current word
    void RemoveLastCharacter();                     // Remove last character
    void ClearAllSlots();                           // Clear all slots and reset
    bool IsValidWord();                             // Check if current word is valid
    bool IsValidForSubmission();                    // Check if word can be submitted
    bool CanSubmitWord();                           // Check if word submission is allowed
    void SubmitWord();                              // Submit current word and award score
    WordDictionary GetWordDictionary();             // Get dictionary instance for external use
}
```

### **Data Structures**

#### **GameStateInfo**
Data structure containing complete game state information required for deadlock detection analysis.

```csharp
public struct GameStateInfo
{
    public HashSet<int> availableTileIds;           // Available tile IDs
    public Dictionary<int, int> childBlockedCounts; // Blocked tile counts
    public int remainingSlots;                      // Remaining word slots
    public string currentWord;                      // Current word being formed

    public GameStateInfo(HashSet<int> availableTileIds, Dictionary<int, int> childBlockedCounts, int remainingSlots, string currentWord);
}
```

#### **DeadlockResult**
Contains the results and analysis from deadlock detection, including whether the game is deadlocked and sample valid words.

```csharp
public struct DeadlockResult
{
    public bool isDeadlocked;                       // Whether game is in deadlock state
    public bool allTilesUsed;                       // Whether all tiles have been used
    public int possibleWordsCount;                  // Number of possible words found
    public List<string> sampleValidWords;           // Sample valid words (up to 5)

    public DeadlockResult(bool isDeadlocked, bool allTilesUsed, int possibleWordsCount, List<string> sampleValidWords = null);
}
```

#### **SlotInfo**
Information about word slot availability and position for tile placement.

```csharp
public struct SlotInfo
{
    public bool IsAvailable { get; private set; }   // Whether slot is available
    public Vector3 Position { get; private set; }   // World position of slot
    public int SlotIndex { get; private set; }       // Index of the slot

    public SlotInfo(bool isAvailable, Vector3 position, int slotIndex);
}
```

#### **WordValidationInfo**
Comprehensive information about current word validation status and possibilities.

```csharp
public struct WordValidationInfo
{
    public string CurrentWord { get; private set; }         // Current word string
    public bool IsValidWord { get; private set; }           // Whether word is valid
    public bool HasValidPrefix { get; private set; }        // Whether prefix is valid
    public bool AreSlotsFull { get; private set; }          // Whether all slots are full
    public List<char> PossibleNextCharacters { get; private set; }  // Possible next characters

    public WordValidationInfo(string currentWord, bool isValidWord, bool hasValidPrefix, bool areSlotsFull, List<char> possibleNextCharacters);
}
```

### **Usage Examples**

#### **Word Formation and Validation**
```csharp
// Get word checker instance
var wordChecker = container.Resolve<WordChecker>();

// Add characters to form a word
wordChecker.AddCharacter('C');
wordChecker.AddCharacter('A');
wordChecker.AddCharacter('T');

// Check if word can be submitted
if (wordChecker.CanSubmitWord())
{
    wordChecker.SubmitWord();  // This will trigger scoring and tile removal
}
```

#### **Deadlock Detection**
```csharp
// Setup game state for deadlock checking
var gameState = new GameStateInfo(availableTileIds, childBlockedCounts, remainingSlots, currentWord);
var deadlockDetector = container.Resolve<DeadlockDetector>();

// Check for deadlock
DeadlockResult result = deadlockDetector.CheckForDeadlock(gameState, tileData);
if (result.isDeadlocked)
{
    Debug.Log($"Game is deadlocked! Found {result.possibleWordsCount} possible words.");
}
```

#### **Score Management**
```csharp
// Listen to score updates
ScoreManager.OnScoreUpdated += (newScore) => {
    Debug.Log($"Score updated to: {newScore}");
};

// Score is automatically updated when words are submitted via WordActions.OnScoreAdded
```

## System Architecture and Data Flow

### Word Formation Flow
1. **Character Input**: Player selects a tile, character is added to WordChecker via AddCharacter()
2. **Slot Management**: WordChecker assigns character to next available slot and updates current word
3. **Real-time Validation**: Word validity and scoring are calculated and broadcasted via WordActions events
4. **Word Submission**: When player submits, WordChecker validates, awards score, and triggers tile removal animations
5. **Post-submission Check**: After tile removal, deadlock detection runs to ensure game remains playable

### Deadlock Detection Algorithm
1. **State Analysis**: Current game state is analyzed including available tiles, blocked tiles, and remaining slots
2. **Path Traversal**: All possible word formation paths are explored using recursive depth-first search
3. **Validation**: Each path is validated against the word dictionary for completeness
4. **Caching**: Results are memoized to improve performance for similar game states
5. **Result Reporting**: Deadlock status and sample valid words are returned for game state management

### Scoring System
1. **Letter Values**: Each character has a predefined score value from LetterSettings
2. **Word Calculation**: Total word score is sum of individual letter scores
3. **Real-time Updates**: Score previews are shown as player forms words
4. **Final Award**: Score is officially added to total when word is successfully submitted

## Internal Dependencies

- **TileManager** - Provides tile data and manages tile selection/removal
- **TileAnimationManager** - Handles tile removal animations after word submission
- **WordDictionary** - Core dictionary service for word validation and prefix checking
- **LetterSettings** - Configuration for letter scoring values
- **GameEvents** - Central event system for deadlock and game completion notifications
- **TileData** - Data structure representing individual tiles with position and dependency information

## External Dependencies

- **Zenject** - Dependency injection for WordChecker and DeadlockDetector services
- **Sirenix.OdinInspector** - Enhanced editor interface for LetterSettings ScriptableObject configuration

---