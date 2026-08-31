using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What each letter is worth. Letters are grouped by score the way a word game
/// board is printed, and flattened into a lookup the first time it is needed.
/// </summary>
[CreateAssetMenu(fileName = "LetterSettings", menuName = "Word Game/Letter Settings")]
public class LetterSettings : ScriptableObject
{
    [SerializeField] private LetterScoreCategory[] letterCategories;

    private Dictionary<char, int> scoreByLetter;

    [System.Serializable]
    public class LetterScoreCategory
    {
        [SerializeField] private string letters;
        [SerializeField] private int score;

        public string Letters => letters;
        public int Score => score;
    }

    public int GetLetterScore(char letter)
    {
        if (scoreByLetter == null)
        {
            BuildLookup();
        }

        int score;
        return scoreByLetter.TryGetValue(letter, out score) ? score : 0;
    }

    private void OnEnable()
    {
        scoreByLetter = null;
    }

    private void BuildLookup()
    {
        scoreByLetter = new Dictionary<char, int>();

        foreach (LetterScoreCategory category in letterCategories)
        {
            foreach (char letter in category.Letters)
            {
                scoreByLetter[letter] = category.Score;
            }
        }
    }
}
