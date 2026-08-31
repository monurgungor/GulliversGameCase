using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LetterSettings", menuName = "Settings/LetterSettings")]
public class LetterSettings : ScriptableObject
{
    [Header("Letter Score Categories")]
    [SerializeField] private LetterScoreCategory[] letterCategories;
    
    private Dictionary<char, int> letterScoreLookup;
    
    [System.Serializable]
    public class LetterScoreCategory
    {
        [SerializeField] private string letters;
        [SerializeField] private int score;
        
        public string Letters => letters;
        public int Score => score;
    }
    
    private void OnEnable()
    {
        InitializeLookup();
    }
    
    /// <summary>
    /// Initialize the fast lookup dictionary
    /// </summary>
    private void InitializeLookup()
    {
        letterScoreLookup = new Dictionary<char, int>();
        
        foreach (var category in letterCategories)
        {
            foreach (char letter in category.Letters)
            {
                if (!letterScoreLookup.ContainsKey(letter))
                {
                    letterScoreLookup[letter] = category.Score;
                }
            }
        }
    }
    
    /// <summary>
    /// Get score for a letter (O(1) lookup)
    /// </summary>
    /// <param name="letter">The letter to get score for</param>
    /// <returns>Score for the letter, or 0 if not found</returns>
    public int GetLetterScore(char letter)
    {
        if (letterScoreLookup == null)
        {
            InitializeLookup();
        }
        
        return letterScoreLookup.TryGetValue(letter, out int score) ? score : 0;
    }
    
    /// <summary>
    /// Check if a letter exists in the score system
    /// </summary>
    /// <param name="letter">The letter to check</param>
    /// <returns>True if letter has a score, false otherwise</returns>
    public bool HasLetterScore(char letter)
    {
        if (letterScoreLookup == null)
        {
            InitializeLookup();
        }
        
        return letterScoreLookup.ContainsKey(letter);
    }
    
    /// <summary>
    /// Get all letters and their scores (for debugging)
    /// </summary>
    /// <returns>Dictionary of all letters and scores</returns>
    public Dictionary<char, int> GetAllLetterScores()
    {
        if (letterScoreLookup == null)
        {
            InitializeLookup();
        }
        
        return new Dictionary<char, int>(letterScoreLookup);
    }

    
}
