using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zenject;
using System.Runtime.InteropServices;

public class WordChecker : MonoBehaviour
{
    [SerializeField] private Transform[] Slots;
    [SerializeField] private TextAsset wordDictionaryAsset;
    [SerializeField]LetterSettings letterSettings;
    private WordDictionary wordDictionary;
    [Inject]private TileAnimationManager tileAnimationManager;
    
    private HashSet<string> submittedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private string currentWord = "";
    private const int SLOT_LIMIT = 7;
    private int currentSlotIndex = 0;

    private int currentWordScore = 0;

    public static Action<string> OnWordChanged;

    private void Start()
    {
        wordDictionary = new WordDictionary(wordDictionaryAsset);
    }

    private void OnEnable()
    {
        WordActions.OnWordSubmitRequested += SubmitWord;
    }

    private void OnDisable()
    {
        WordActions.OnWordSubmitRequested -= SubmitWord;
    }



    /// <summary>
    /// Check if there's an available slot and return its position
    /// </summary>
    /// <returns>SlotInfo containing availability and position</returns>
    public SlotInfo GetAvailableSlot()
    {
        if (currentSlotIndex >= SLOT_LIMIT)
        {
            return new SlotInfo(false, Vector3.zero, -1);
        }

        if (currentSlotIndex < Slots.Length)
        {
            Transform availableSlot = Slots[currentSlotIndex];
            if (availableSlot != null)
            {
                Vector3 position = availableSlot.position;
                return new SlotInfo(true, position, currentSlotIndex);
            }
        }

        return new SlotInfo(false, Vector3.zero, -1);
    }

    /// <summary>
    /// Add a character to the current word and move to next slot
    /// </summary>
    /// <param name="character">Character to add</param>
    public void AddCharacter(char character)
    {
        if (currentSlotIndex < SLOT_LIMIT)
        {
            currentWord += character;
            currentSlotIndex++;
            
            bool isValid = IsValidForSubmission();
            WordActions.OnWordValidityChanged?.Invoke(isValid);
            
            if (letterSettings != null)
            {
                int score = isValid ? CalculateWordScore(currentWord) : 0;
                currentWordScore = score;
                WordActions.OnWordScoreChanged?.Invoke(score);
            }
            
            OnWordChanged?.Invoke(currentWord);
        }
    }

    /// <summary>
    /// Remove the last character from the current word
    /// </summary>
    public void RemoveLastCharacter()
    {
        if (currentWord.Length > 0)
        {
            currentWord = currentWord.Substring(0, currentWord.Length - 1);
            currentSlotIndex = Mathf.Max(0, currentSlotIndex - 1);
            
            bool isValid = IsValidForSubmission();
            WordActions.OnWordValidityChanged?.Invoke(isValid);
            
            if (letterSettings != null)
            {
                int score = isValid ? CalculateWordScore(currentWord) : 0;
                currentWordScore = score;
                WordActions.OnWordScoreChanged?.Invoke(score);
            }
            
            OnWordChanged?.Invoke(currentWord);
        }
    }



    /// <summary>
    /// Clear all slots and reset word
    /// </summary>
    public void ClearAllSlots()
    {
        currentWord = "";
        currentSlotIndex = 0;
        currentWordScore = 0;
        
        WordActions.OnWordValidityChanged?.Invoke(false);
        if (letterSettings != null)
        {
            WordActions.OnWordScoreChanged?.Invoke(0);
        }
        
        OnWordChanged?.Invoke(currentWord);
    }

    /// <summary>
    /// Get current word
    /// </summary>
    /// <returns>Current word string</returns>
    public string GetCurrentWord()
    {
        return currentWord;
    }

    /// <summary>
    /// Get current slot index
    /// </summary>
    /// <returns>Current slot index</returns>
    public int GetCurrentSlotIndex()
    {
        return currentSlotIndex;
    }

    /// <summary>
    /// Check if slots are full
    /// </summary>
    /// <returns>True if all slots are occupied</returns>
    public bool AreSlotsFull()
    {
        return currentSlotIndex >= SLOT_LIMIT;
    }

    /// <summary>
    /// Check if slots are empty
    /// </summary>
    /// <returns>True if no slots are occupied</returns>
    public bool AreSlotsEmpty()
    {
        return currentSlotIndex == 0;
    }

    /// <summary>
    /// Check if current word is valid
    /// </summary>
    /// <returns>True if word exists in dictionary</returns>
    public bool IsValidWord()
    {
        if (wordDictionary == null || string.IsNullOrEmpty(currentWord))
            return false;
        
        return wordDictionary.IsValidWord(currentWord);
    }

    /// <summary>
    /// Check if current word can be submitted (valid in dictionary and not previously submitted)
    /// </summary>
    /// <returns>True if word can be submitted</returns>
    public bool IsValidForSubmission()
    {
        if (!IsValidWord())
            return false;
            
        if (submittedWords.Contains(currentWord))
            return false;
            
        return true;
    }

    /// <summary>
    /// Check if current word is a valid prefix (can be extended)
    /// </summary>
    /// <returns>True if word is a valid prefix</returns>
    public bool HasValidPrefix()
    {
        if (wordDictionary == null || string.IsNullOrEmpty(currentWord))
            return true; // Empty word is always a valid prefix
        
        return wordDictionary.HasValidPrefix(currentWord);
    }


    /// <summary>
    /// Get all possible words that start with current prefix
    /// </summary>
    /// <returns>List of possible words</returns>
    public List<string> GetPossibleWords()
    {
        if (wordDictionary == null || string.IsNullOrEmpty(currentWord))
            return new List<string>();
        
        return wordDictionary.GetWordsWithPrefix(currentWord);
    }

    /// <summary>
    /// Get possible next characters from current word
    /// </summary>
    /// <returns>List of possible next characters</returns>
    public List<char> GetPossibleNextCharacters()
    {
        if (wordDictionary == null)
            return new List<char>();
        
        return wordDictionary.GetPossibleNextCharacters(currentWord);
    }


    /// <summary>
    /// Get word validation info
    /// </summary>
    /// <returns>WordValidationInfo with details about current word</returns>
    public WordValidationInfo GetWordValidationInfo()
    {
        return new WordValidationInfo(
            currentWord,
            IsValidWord(),
            HasValidPrefix(),
            AreSlotsFull(),
            GetPossibleNextCharacters()
        );
    }

    /// <summary>
    /// Check if current word can be submitted (valid word with at least 1 character)
    /// </summary>
    /// <returns>True if word can be submitted</returns>
    public bool CanSubmitWord()
    {
        return !string.IsNullOrEmpty(currentWord) && IsValidForSubmission();
    }

    /// <summary>
    /// Check if current word is too short (less than minimum length)
    /// </summary>
    /// <param name="minimumLength">Minimum word length (default: 1)</param>
    /// <returns>True if word is too short</returns>
    public bool IsWordTooShort(int minimumLength = 1)
    {
        return currentWord.Length < minimumLength;
    }

    /// <summary>
    /// Get current word length
    /// </summary>
    /// <returns>Number of characters in current word</returns>
    public int GetWordLength()
    {
        return currentWord.Length;
    }

    /// <summary>
    /// Check if a word has been submitted before
    /// </summary>
    /// <param name="word">Word to check</param>
    /// <returns>True if the word has been submitted before</returns>
    public bool HasBeenSubmitted(string word)
    {
        if (string.IsNullOrEmpty(word))
            return false;
            
        return submittedWords.Contains(word);
    }

    /// <summary>
    /// Get the list of all submitted words
    /// </summary>
    /// <returns>ReadOnly collection of submitted words</returns>
    public IReadOnlyCollection<string> GetSubmittedWords()
    {
        return submittedWords;
    }

    /// <summary>
    /// Clear all submitted words (useful for new levels)
    /// </summary>
    public void ClearSubmittedWords()
    {
        submittedWords.Clear();
    }

    /// <summary>
    /// Get count of submitted words
    /// </summary>
    /// <returns>Number of words submitted</returns>
    public int GetSubmittedWordCount()
    {
        return submittedWords.Count;
    }

    /// <summary>
    /// Submit current word if valid
    /// </summary>
    public void SubmitWord()
    {
        if (IsValidForSubmission() && letterSettings != null)
        {
            int score = CalculateWordScore(currentWord);
            
            submittedWords.Add(currentWord);
            
            WordActions.OnWordSubmitted?.Invoke(currentWord);
            WordActions.OnScoreAdded?.Invoke(score);
            
            ClearAllSlots();
            
            RemoveSelectedTiles();
        }
    }
    
    /// <summary>
    /// Check for deadlock after word submission
    /// </summary>
    private void CheckForDeadlockAfterSubmission()
    {
        var tileManager = FindObjectOfType<TileManager>();
        if (tileManager != null)
        {
            StartCoroutine(DelayedDeadlockCheck(tileManager));
        }
    }
    
    /// <summary>
    /// Delayed deadlock check coroutine
    /// </summary>
    private IEnumerator DelayedDeadlockCheck(TileManager tileManager)
    {
        yield return null;
        
        tileManager.CheckForDeadlock();
    }

    /// <summary>
    /// Remove selected tiles from the board with animation
    /// </summary>
    private void RemoveSelectedTiles()
    {
        var tileManager = FindObjectOfType<TileManager>();
        if (tileManager != null)
        {
            var selectedTileNodes = tileManager.GetSelectedTiles();
            var tiles = new List<Tile>();
            
            foreach (var tileNode in selectedTileNodes)
            {
                if (tileNode.Tile != null)
                {
                    tiles.Add(tileNode.Tile);
                }
            }
            
            tileManager.ClearSelectedTiles();
            
            if (tileAnimationManager != null && tiles.Count > 0)
            {
                tileAnimationManager.AnimateSubmit(tiles, () =>
                {
                    foreach (var tile in tiles)
                    {
                        if (tile != null)
                        {
                            Destroy(tile.gameObject);
                        }
                    }
                    
                    CheckForDeadlockAfterSubmission();
                });
            }
            else
            {
                foreach (var tile in tiles)
                {
                    if (tile != null)
                    {
                        Destroy(tile.gameObject);
                    }
                }
                
                CheckForDeadlockAfterSubmission();
            }
        }
    }


    /// <summary>
    /// Get WordDictionary instance for external access (used by dependency injection)
    /// </summary>
    /// <returns>WordDictionary instance</returns>
    public WordDictionary GetWordDictionary()
    {
        return wordDictionary;
    }

    /// <summary>
    /// Calculate total score for a word/string (private helper)
    /// </summary>
    /// <param name="word">The word to calculate score for</param>
    /// <returns>Total score for the word</returns>
    private int CalculateWordScore(string word)
    {
        if (string.IsNullOrEmpty(word) || letterSettings == null)
            return 0;
        
        int totalScore = 0;
        
        foreach (char letter in word)
        {
            totalScore += letterSettings.GetLetterScore(letter);
        }
        
        return totalScore;
    }

    /// <summary>
    /// Calculate word score for external use (ScoreManager)
    /// </summary>
    /// <param name="isValid">Whether the word is valid</param>
    /// <param name="word">The word to calculate score for</param>
    public void CalculateWordScore(bool isValid, string word)
    {
        if (!isValid) {
            WordActions.OnWordScoreChanged?.Invoke(0);
            return;
        }
        
        int totalScore = CalculateWordScore(word);
        WordActions.OnWordScoreChanged?.Invoke(totalScore);
    }
}

/// <summary>
/// Information about a slot's availability and position
/// </summary>
[System.Serializable]
public struct SlotInfo
{
    public bool IsAvailable { get; private set; }
    public Vector3 Position { get; private set; }
    public int SlotIndex { get; private set; }

    public SlotInfo(bool isAvailable, Vector3 position, int slotIndex)
    {
        IsAvailable = isAvailable;
        Position = position;
        SlotIndex = slotIndex;
    }
}

/// <summary>
/// Information about word validation status
/// </summary>
[System.Serializable]
public struct WordValidationInfo
{
    public string CurrentWord { get; private set; }
    public bool IsValidWord { get; private set; }
    public bool HasValidPrefix { get; private set; }
    public bool AreSlotsFull { get; private set; }
    public List<char> PossibleNextCharacters { get; private set; }

    public WordValidationInfo(string currentWord, bool isValidWord, bool hasValidPrefix, bool areSlotsFull, List<char> possibleNextCharacters)
    {
        CurrentWord = currentWord;
        IsValidWord = isValidWord;
        HasValidPrefix = hasValidPrefix;
        AreSlotsFull = areSlotsFull;
        PossibleNextCharacters = possibleNextCharacters;
    }
}
