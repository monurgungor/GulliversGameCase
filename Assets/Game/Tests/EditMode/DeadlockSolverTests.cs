using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DeadlockSolverTests
{
    private static readonly string[] NoWordsPlayed = new string[0];

    private static DeadlockSolver Solver()
    {
        return new DeadlockSolver(WordDictionary.Parse("AT\nATE\nCAT\nTEA\n"));
    }

    /// <summary>Builds a board where every tile is free unless another tile lists it as a child.</summary>
    private static Dictionary<int, TileData> Board(params (int id, char letter, int[] children)[] tiles)
    {
        var board = new Dictionary<int, TileData>();

        foreach (var tile in tiles)
        {
            board[tile.id] = new TileData(tile.id, tile.letter, Vector3.zero, tile.children ?? new int[0], 1);
        }

        return board;
    }

    [Test]
    public void Finds_a_word_when_the_letters_are_all_free()
    {
        string word;
        bool found = Solver().TryFindWord(
            Board((0, 'C', null), (1, 'A', null), (2, 'T', null)),
            BoardRules.SlotCount, NoWordsPlayed, out word);

        Assert.IsTrue(found);
        Assert.AreEqual("CAT", word);
    }

    [Test]
    public void Reports_a_dead_board_when_no_letters_spell_anything()
    {
        string word;
        bool found = Solver().TryFindWord(
            Board((0, 'X', null), (1, 'Q', null)),
            BoardRules.SlotCount, NoWordsPlayed, out word);

        Assert.IsFalse(found);
    }

    [Test]
    public void A_covered_letter_can_still_be_used_once_its_cover_is_taken()
    {
        // C covers A, so the search has to take C before A becomes reachable.
        string word;
        bool found = Solver().TryFindWord(
            Board((0, 'C', new[] { 1 }), (1, 'A', null), (2, 'T', null)),
            BoardRules.SlotCount, NoWordsPlayed, out word);

        Assert.IsTrue(found);
        Assert.AreEqual("CAT", word);
    }

    [Test]
    public void A_word_already_played_does_not_keep_the_level_alive()
    {
        string word;
        bool found = Solver().TryFindWord(
            Board((0, 'C', null), (1, 'A', null), (2, 'T', null)),
            BoardRules.SlotCount, new[] { "CAT", "AT" }, out word);

        Assert.IsFalse(found, "CAT and AT are spent, and TEA needs an E");
    }

    [Test]
    public void Words_longer_than_the_slots_are_out_of_reach()
    {
        string word;
        bool found = Solver().TryFindWord(
            Board((0, 'C', null), (1, 'A', null), (2, 'T', null)),
            2, NoWordsPlayed, out word);

        Assert.IsTrue(found);
        Assert.AreEqual("AT", word, "CAT does not fit in two slots");
    }

    [Test]
    public void An_empty_board_has_nothing_to_play()
    {
        string word;
        Assert.IsFalse(Solver().TryFindWord(Board(), BoardRules.SlotCount, NoWordsPlayed, out word));
    }

    [Test]
    public void A_solver_instance_gives_the_same_answer_when_reused()
    {
        DeadlockSolver solver = Solver();
        Dictionary<int, TileData> playable = Board((0, 'C', null), (1, 'A', null), (2, 'T', null));
        Dictionary<int, TileData> dead = Board((0, 'X', null), (1, 'Q', null));

        string word;
        Assert.IsTrue(solver.TryFindWord(playable, BoardRules.SlotCount, NoWordsPlayed, out word));
        Assert.IsFalse(solver.TryFindWord(dead, BoardRules.SlotCount, NoWordsPlayed, out word));
        Assert.IsTrue(solver.TryFindWord(playable, BoardRules.SlotCount, NoWordsPlayed, out word),
            "state from the dead board must not leak into the next search");
    }
}
