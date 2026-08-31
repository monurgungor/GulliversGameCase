using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Bakes the raw word list into the asset the game ships with.
///
/// The board holds seven letters, so roughly three quarters of the raw list can
/// never be played. Dropping those words and sorting the rest turns a 2.7 MB
/// asset into a 520 KB one, and lets the runtime binary search it instead of
/// building a graph at load time.
///
/// The raw list stays under an Editor folder, so it never reaches a build.
/// </summary>
public static class WordListBaker
{
    private const string SourcePath = "Assets/Game/Data/Editor/WordList.source.txt";
    private const string BakedPath = "Assets/Game/Data/WordList.txt";

    [MenuItem("Tools/Word Game/Rebuild Word List")]
    public static void Rebuild()
    {
        if (!File.Exists(SourcePath))
        {
            Debug.LogError($"WordListBaker: source list not found at {SourcePath}");
            return;
        }

        string[] rawWords = File.ReadAllText(SourcePath)
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        var playable = new List<string>(rawWords.Length / 3);
        int rejected = 0;

        foreach (string rawWord in rawWords)
        {
            string word = rawWord.Trim().ToUpperInvariant();

            if (word.Length < 2 || word.Length > BoardRules.SlotCount)
            {
                continue;
            }

            if (!IsPlainLetters(word))
            {
                rejected++;
                continue;
            }

            playable.Add(word);
        }

        playable.Sort(StringComparer.Ordinal);
        RemoveAdjacentDuplicates(playable);

        var baked = new StringBuilder(playable.Count * 8);
        foreach (string word in playable)
        {
            baked.Append(word).Append('\n');
        }

        File.WriteAllText(BakedPath, baked.ToString());
        AssetDatabase.ImportAsset(BakedPath);

        long sourceSize = new FileInfo(SourcePath).Length;
        long bakedSize = new FileInfo(BakedPath).Length;
        Debug.Log($"WordListBaker: {playable.Count:N0} of {rawWords.Length:N0} words kept " +
                  $"({sourceSize / 1024} KB -> {bakedSize / 1024} KB), {rejected} rejected as non-letters.");
    }

    private static bool IsPlainLetters(string word)
    {
        foreach (char letter in word)
        {
            if (letter < 'A' || letter > 'Z')
            {
                return false;
            }
        }

        return true;
    }

    private static void RemoveAdjacentDuplicates(List<string> sortedWords)
    {
        int write = 0;

        for (int read = 0; read < sortedWords.Count; read++)
        {
            if (write > 0 && string.Equals(sortedWords[write - 1], sortedWords[read], StringComparison.Ordinal))
            {
                continue;
            }

            sortedWords[write++] = sortedWords[read];
        }

        sortedWords.RemoveRange(write, sortedWords.Count - write);
    }
}
