using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Owns the board: which tiles are still in play, which of them are free to
/// tap, and what happens when a word is submitted. It is also the one place
/// that decides a level is over, so that the penalty is always applied before
/// anyone reads the final score.
/// </summary>
[RequireComponent(typeof(TilePlacer))]
[RequireComponent(typeof(TileClickHandler))]
public class TileManager : MonoBehaviour
{
    [Inject] private WordChecker wordChecker;
    [Inject] private VisualSettings visualSettings;
    [Inject] private ScoreManager scoreManager;
    [Inject] private TileAnimationManager tileAnimationManager;
    [Inject] private DeadlockSolver deadlockSolver;

    /// <summary>Every tile still in play, on the board or in a slot.</summary>
    private readonly Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();

    /// <summary>Tiles that have not been moved into a slot.</summary>
    private readonly HashSet<int> onBoard = new HashSet<int>();

    /// <summary>How many tiles still on the board cover each tile.</summary>
    private readonly Dictionary<int, int> coveredBy = new Dictionary<int, int>();

    /// <summary>Which tiles cover each tile, built once so state updates stay linear.</summary>
    private readonly Dictionary<int, List<int>> covers = new Dictionary<int, List<int>>();

    /// <summary>Tiles sitting in the slots, in the order they were tapped.</summary>
    private readonly List<Tile> selected = new List<Tile>();

    /// <summary>Reused so the deadlock search does not allocate a board copy per move.</summary>
    private readonly Dictionary<int, TileData> boardSnapshot = new Dictionary<int, TileData>();

    private void OnEnable()
    {
        TileClickHandler.TileClicked += OnTileClicked;
        WordActions.UndoRequested += Undo;
        WordActions.WordSubmitted += OnWordSubmitted;
    }

    private void OnDisable()
    {
        TileClickHandler.TileClicked -= OnTileClicked;
        WordActions.UndoRequested -= Undo;
        WordActions.WordSubmitted -= OnWordSubmitted;
    }

    private void Start()
    {
        WordActions.RaiseUndoAvailabilityChanged(false);
    }

    /// <summary>Takes ownership of the tiles TilePlacer just spawned.</summary>
    public void SetTiles(Dictionary<int, Tile> spawnedTiles)
    {
        tiles.Clear();
        onBoard.Clear();
        coveredBy.Clear();
        covers.Clear();
        selected.Clear();

        foreach (var entry in spawnedTiles)
        {
            tiles[entry.Key] = entry.Value;
            onBoard.Add(entry.Key);
            coveredBy[entry.Key] = 0;
        }

        foreach (var tile in tiles.Values)
        {
            int[] children = tile.TileData.Children;
            if (children == null)
            {
                continue;
            }

            foreach (int childId in children)
            {
                if (!tiles.ContainsKey(childId))
                {
                    continue;
                }

                coveredBy[childId]++;

                List<int> parents;
                if (!covers.TryGetValue(childId, out parents))
                {
                    parents = new List<int>();
                    covers[childId] = parents;
                }

                parents.Add(tile.TileData.Id);
            }
        }

        RefreshTileStates();
        WordActions.RaiseUndoAvailabilityChanged(false);
    }

    private void OnTileClicked(Tile tile)
    {
        if (tile == null || !IsFree(tile.TileData.Id) || wordChecker.SlotsAreFull)
        {
            return;
        }

        Vector3 slotPosition;
        if (!wordChecker.TryGetFreeSlot(out slotPosition))
        {
            return;
        }

        int tileId = tile.TileData.Id;

        onBoard.Remove(tileId);
        selected.Add(tile);
        wordChecker.AddLetter(tile.TileData.Character);
        Uncover(tile, -1);

        tileAnimationManager.AnimateSelect(tile, slotPosition);

        RefreshTileStates();
        WordActions.RaiseUndoAvailabilityChanged(true);
    }

    /// <summary>Returns the last letter, or every letter when <paramref name="undoAll"/> is set.</summary>
    private void Undo(bool undoAll)
    {
        int count = undoAll ? selected.Count : 1;
        if (count == 0 || selected.Count == 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Tile tile = selected[selected.Count - 1];
            selected.RemoveAt(selected.Count - 1);

            onBoard.Add(tile.TileData.Id);
            Uncover(tile, 1);
            wordChecker.RemoveLastLetter();
            tileAnimationManager.AnimateUndo(tile);
        }

        RefreshTileStates();
        WordActions.RaiseUndoAvailabilityChanged(selected.Count > 0);
    }

    /// <summary>Flies the submitted tiles off the board, then checks whether the level is over.</summary>
    private void OnWordSubmitted(string word)
    {
        var submitted = new List<Tile>(selected);
        selected.Clear();

        foreach (var tile in submitted)
        {
            tiles.Remove(tile.TileData.Id);
            onBoard.Remove(tile.TileData.Id);
        }

        RefreshTileStates();
        WordActions.RaiseUndoAvailabilityChanged(false);

        tileAnimationManager.AnimateSubmit(submitted, () =>
        {
            foreach (var tile in submitted)
            {
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }

            EvaluateLevelEnd();
        });
    }

    /// <summary>
    /// Ends the level when the board is clear, or when nothing playable is left.
    /// The penalty is applied here so that whoever saves the score sees the
    /// final number.
    /// </summary>
    private void EvaluateLevelEnd()
    {
        if (tiles.Count == 0)
        {
            GameEvents.RaiseGameEnded(GameEndReason.BoardCleared);
            return;
        }

        boardSnapshot.Clear();
        foreach (int tileId in onBoard)
        {
            boardSnapshot[tileId] = tiles[tileId].TileData;
        }

        string playableWord;
        if (deadlockSolver.TryFindWord(boardSnapshot, BoardRules.SlotCount, wordChecker.SubmittedWords, out playableWord))
        {
            return;
        }

        if (deadlockSolver.LastSearchWasCutShort)
        {
            Debug.LogWarning("TileManager: deadlock search hit its node budget; leaving the level playable.");
            return;
        }

        scoreManager.SubtractScore(onBoard.Count * BoardRules.DeadlockPenaltyPerTile);
        GameEvents.RaiseGameEnded(GameEndReason.NoWordsLeft);
    }

    /// <summary>A tile can be tapped when it is on the board and nothing covers it.</summary>
    private bool IsFree(int tileId)
    {
        int cover;
        return onBoard.Contains(tileId) && (!coveredBy.TryGetValue(tileId, out cover) || cover == 0);
    }

    private void Uncover(Tile tile, int delta)
    {
        int[] children = tile.TileData.Children;
        if (children == null)
        {
            return;
        }

        foreach (int childId in children)
        {
            int cover;
            if (coveredBy.TryGetValue(childId, out cover))
            {
                coveredBy[childId] = cover + delta;
            }
        }
    }

    private void RefreshTileStates()
    {
        foreach (var tile in tiles.Values)
        {
            int tileId = tile.TileData.Id;
            tile.SetState(StateOf(tileId), visualSettings);
            tile.SetInteractable(IsFree(tileId));
        }
    }

    private TileState StateOf(int tileId)
    {
        if (!onBoard.Contains(tileId) || IsFree(tileId))
        {
            return TileState.Clickable;
        }

        return CanBeUncoveredNow(tileId) ? TileState.Potential : TileState.Blocked;
    }

    /// <summary>
    /// A covered tile shows as the next one up when at least one of the tiles
    /// covering it can be tapped right now.
    /// </summary>
    private bool CanBeUncoveredNow(int tileId)
    {
        List<int> parents;
        if (!covers.TryGetValue(tileId, out parents))
        {
            return false;
        }

        foreach (int parentId in parents)
        {
            if (IsFree(parentId))
            {
                return true;
            }
        }

        return false;
    }
}
