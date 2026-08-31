using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Owns the letters sitting in the slots: what they spell, what they are worth,
/// and whether they can be submitted. It does not touch the tiles themselves;
/// TileManager listens for a submitted word and clears the board.
/// </summary>
public class WordChecker : MonoBehaviour
{
    [Tooltip("Slot transforms in play order. Must match BoardRules.SlotCount.")]
    [SerializeField] private Transform[] Slots;

    [Inject] private WordDictionary wordDictionary;
    [Inject] private LetterSettings letterSettings;

    private readonly HashSet<string> submittedWords = new HashSet<string>(StringComparer.Ordinal);
    private readonly char[] letters = new char[BoardRules.SlotCount];

    private int letterCount;
    private string currentWord = string.Empty;

    public static event Action<string> WordChanged;

    /// <summary>Words the player has already submitted this level.</summary>
    public ICollection<string> SubmittedWords => submittedWords;

    private void OnEnable()
    {
        WordActions.SubmitRequested += SubmitWord;
    }

    private void OnDisable()
    {
        WordActions.SubmitRequested -= SubmitWord;
    }

    private void OnValidate()
    {
        if (Slots != null && Slots.Length != BoardRules.SlotCount)
        {
            Debug.LogError($"{name}: {Slots.Length} slots wired but BoardRules.SlotCount is {BoardRules.SlotCount}.", this);
        }
    }

    /// <summary>Where the next tile should fly to, if there is room for one.</summary>
    public bool TryGetFreeSlot(out Vector3 position)
    {
        position = Vector3.zero;

        if (letterCount >= BoardRules.SlotCount || letterCount >= Slots.Length || Slots[letterCount] == null)
        {
            return false;
        }

        position = Slots[letterCount].position;
        return true;
    }

    /// <summary>Adds a letter to the slots.</summary>
    public void AddLetter(char letter)
    {
        if (letterCount >= BoardRules.SlotCount)
        {
            return;
        }

        letters[letterCount++] = letter;
        OnLettersChanged();
    }

    /// <summary>Takes the last letter back out of the slots.</summary>
    public void RemoveLastLetter()
    {
        if (letterCount == 0)
        {
            return;
        }

        letterCount--;
        OnLettersChanged();
    }

    /// <summary>Empties the slots without submitting anything.</summary>
    public void ClearSlots()
    {
        letterCount = 0;
        OnLettersChanged();
    }

    /// <summary>True when every slot is taken.</summary>
    public bool SlotsAreFull => letterCount >= BoardRules.SlotCount;

    /// <summary>
    /// Accepts the letters in the slots if they spell a word that has not been
    /// played yet. TileManager reacts to the submitted word by clearing tiles.
    /// </summary>
    public void SubmitWord()
    {
        if (!CanSubmit())
        {
            return;
        }

        string word = currentWord;
        submittedWords.Add(word);

        WordActions.RaiseWordSubmitted(word);
        WordActions.RaiseScoreAdded(ScoreOf(word));

        ClearSlots();
    }

    private bool CanSubmit()
    {
        return letterCount > 0
            && wordDictionary.Contains(currentWord)
            && !submittedWords.Contains(currentWord);
    }

    private void OnLettersChanged()
    {
        currentWord = letterCount == 0 ? string.Empty : new string(letters, 0, letterCount);

        bool canSubmit = CanSubmit();
        WordActions.RaiseWordValidityChanged(canSubmit);
        WordActions.RaiseWordScoreChanged(canSubmit ? ScoreOf(currentWord) : 0);
        WordChanged?.Invoke(currentWord);
    }

    private int ScoreOf(string word)
    {
        if (letterSettings == null)
        {
            return 0;
        }

        int total = 0;
        foreach (char letter in word)
        {
            total += letterSettings.GetLetterScore(letter);
        }

        return total;
    }
}
