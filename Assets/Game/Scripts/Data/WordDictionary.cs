using System;

/// <summary>
/// The word list the game validates against, held as one ordinally sorted array
/// so that both membership and prefix tests are binary searches. The deadlock
/// search runs thousands of prefix tests per move, so it gets overloads that
/// read straight from a character buffer and never allocate.
/// </summary>
public sealed class WordDictionary
{
    private static readonly char[] LineSeparators = { '\n', '\r' };

    private readonly string[] words;

    private WordDictionary(string[] sortedWords)
    {
        words = sortedWords;
    }

    public int Count => words.Length;

    /// <summary>
    /// Reads a baked word list. This is pure string work, so it is safe to call
    /// off the main thread. The list must already be sorted with
    /// <see cref="StringComparer.Ordinal"/>, which WordListBaker guarantees.
    /// </summary>
    public static WordDictionary Parse(string wordList)
    {
        if (string.IsNullOrEmpty(wordList))
        {
            return new WordDictionary(Array.Empty<string>());
        }

        return new WordDictionary(wordList.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>True when the word is in the list.</summary>
    public bool Contains(string word)
    {
        return !string.IsNullOrEmpty(word)
            && Array.BinarySearch(words, word, StringComparer.Ordinal) >= 0;
    }

    /// <summary>True when the first <paramref name="length"/> characters spell a word.</summary>
    public bool Contains(char[] buffer, int length)
    {
        if (buffer == null || length <= 0)
        {
            return false;
        }

        int index = LowerBound(buffer, length);
        return index < words.Length && Compare(words[index], buffer, length) == 0;
    }

    /// <summary>
    /// True when at least one word starts with the first <paramref name="length"/>
    /// characters. The search relies on the list being sorted: if any word carries
    /// that prefix, the first word that does not sort before it is one of them.
    /// </summary>
    public bool HasWordWithPrefix(char[] buffer, int length)
    {
        if (buffer == null || length <= 0)
        {
            return words.Length > 0;
        }

        int index = LowerBound(buffer, length);
        if (index >= words.Length)
        {
            return false;
        }

        string candidate = words[index];
        if (candidate.Length < length)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            if (candidate[i] != buffer[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Index of the first word that does not sort before the buffer.</summary>
    private int LowerBound(char[] buffer, int length)
    {
        int low = 0;
        int high = words.Length;

        while (low < high)
        {
            int middle = low + ((high - low) >> 1);
            if (Compare(words[middle], buffer, length) < 0)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    private static int Compare(string word, char[] buffer, int length)
    {
        int shared = word.Length < length ? word.Length : length;

        for (int i = 0; i < shared; i++)
        {
            int difference = word[i] - buffer[i];
            if (difference != 0)
            {
                return difference;
            }
        }

        return word.Length - length;
    }
}
