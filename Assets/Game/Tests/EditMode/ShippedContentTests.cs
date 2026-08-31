using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Guards the assets the game ships with. The word list is a baked file, so
/// nothing at runtime would notice if it were re-generated wrong; these checks
/// are what notice.
/// </summary>
public class ShippedContentTests
{
    private const string WordListPath = "Assets/Game/Data/WordList.txt";
    private const string LetterSettingsPath = "Assets/Game/Data/LetterSettings.asset";

    private static string[] LoadWords()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(WordListPath);
        Assert.IsNotNull(asset, WordListPath + " is missing; run Tools > Word Game > Rebuild Word List");

        return asset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    }

    [Test]
    public void The_word_list_is_sorted_because_lookups_binary_search_it()
    {
        string[] words = LoadWords();

        for (int i = 1; i < words.Length; i++)
        {
            Assert.LessOrEqual(string.CompareOrdinal(words[i - 1], words[i]), 0,
                $"'{words[i - 1]}' and '{words[i]}' are out of order at index {i}");
        }
    }

    [Test]
    public void The_word_list_only_holds_words_the_board_can_actually_spell()
    {
        foreach (string word in LoadWords())
        {
            Assert.GreaterOrEqual(word.Length, 2, word);
            Assert.LessOrEqual(word.Length, BoardRules.SlotCount, word);

            foreach (char letter in word)
            {
                Assert.IsTrue(letter >= 'A' && letter <= 'Z', $"'{word}' contains '{letter}'");
            }
        }
    }

    [Test]
    public void Every_letter_the_board_can_show_has_a_score()
    {
        var settings = AssetDatabase.LoadAssetAtPath<LetterSettings>(LetterSettingsPath);
        Assert.IsNotNull(settings, LetterSettingsPath + " is missing");

        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            Assert.Greater(settings.GetLetterScore(letter), 0, $"'{letter}' scores nothing");
        }
    }

    [Test]
    public void Every_level_file_parses_and_its_children_point_at_real_tiles()
    {
        Assert.IsNotEmpty(LevelCatalog.Levels, "no level files were found");

        foreach (LevelInfo info in LevelCatalog.Levels)
        {
            LevelJsonData level = LevelCatalog.LoadLevel(info.Id);
            Assert.IsNotNull(level, $"level {info.Id} did not parse");
            Assert.IsNotNull(level.tiles, $"level {info.Id} has no tiles");
            Assert.IsNotEmpty(level.tiles, $"level {info.Id} has no tiles");

            var ids = new System.Collections.Generic.HashSet<int>();
            foreach (TileData tile in level.tiles)
            {
                Assert.IsTrue(ids.Add(tile.Id), $"level {info.Id} repeats tile id {tile.Id}");
            }

            foreach (TileData tile in level.tiles)
            {
                if (tile.Children == null)
                {
                    continue;
                }

                foreach (int childId in tile.Children)
                {
                    Assert.IsTrue(ids.Contains(childId),
                        $"level {info.Id}: tile {tile.Id} covers {childId}, which does not exist");
                }
            }
        }
    }

    [Test]
    public void Every_level_starts_out_playable()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(WordListPath);
        var solver = new DeadlockSolver(WordDictionary.Parse(asset.text));
        var nothingPlayed = new string[0];

        foreach (LevelInfo info in LevelCatalog.Levels)
        {
            LevelJsonData level = LevelCatalog.LoadLevel(info.Id);
            var board = new System.Collections.Generic.Dictionary<int, TileData>();

            foreach (TileData tile in level.tiles)
            {
                board[tile.Id] = tile;
            }

            string word;
            Assert.IsTrue(solver.TryFindWord(board, BoardRules.SlotCount, nothingPlayed, out word),
                $"level {info.Id} ('{info.Title}') is unplayable from the first move");
        }
    }
}
