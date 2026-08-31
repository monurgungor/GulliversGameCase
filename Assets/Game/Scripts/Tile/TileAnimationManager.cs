using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

/// <summary>
/// Moves tiles between the board and the slots. A tile has at most one
/// animation at a time, so tapping and undoing quickly cannot leave one half
/// way between two places.
/// </summary>
public class TileAnimationManager : MonoBehaviour
{
    [Header("Durations")]
    [SerializeField] private float selectDuration = 0.5f;
    [SerializeField] private float undoDuration = 0.4f;
    [SerializeField] private float submitDuration = 0.8f;
    [SerializeField] private float submitStagger = 0.1f;

    [Header("Easing")]
    [SerializeField] private Ease selectEase = Ease.OutBack;
    [SerializeField] private Ease undoEase = Ease.OutQuad;
    [SerializeField] private Ease submitEase = Ease.InBack;

    [Header("Submit")]
    [Tooltip("Where submitted tiles fly to. Falls back to the offset below when unset.")]
    [SerializeField] private RectTransform submitTarget;
    [SerializeField] private Vector3 submitExitOffset = new Vector3(0f, 10f, 0f);

    private readonly Dictionary<int, Vector3> homePositions = new Dictionary<int, Vector3>();
    private readonly Dictionary<int, Sequence> running = new Dictionary<int, Sequence>();

    /// <summary>Remembers where a tile sits on the board so undo can send it back.</summary>
    public void StoreHomePosition(Tile tile)
    {
        homePositions[tile.TileData.Id] = tile.transform.position;
    }

    public void AnimateSelect(Tile tile, Vector3 slotPosition)
    {
        Prepare(tile);
        Track(tile, Sequence.Create(
            Tween.Position(tile.transform, Flatten(slotPosition), selectDuration, selectEase)));
    }

    public void AnimateUndo(Tile tile)
    {
        Prepare(tile);

        Vector3 home;
        if (!homePositions.TryGetValue(tile.TileData.Id, out home))
        {
            home = tile.TileData.Position;
        }

        Track(tile, Sequence.Create(
            Tween.Position(tile.transform, Flatten(home), undoDuration, undoEase)));
    }

    /// <summary>Sends a submitted word off the board, one tile after another.</summary>
    public void AnimateSubmit(List<Tile> tiles, Action onComplete)
    {
        if (tiles == null || tiles.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        int remaining = tiles.Count;

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            Prepare(tile);

            Vector3 exit = submitTarget != null
                ? Flatten(submitTarget.position)
                : tile.transform.position + submitExitOffset;

            float startAt = i * submitStagger;
            int tileId = tile.TileData.Id;

            Sequence sequence = Sequence.Create()
                .Insert(startAt, Tween.Position(tile.transform, exit, submitDuration, submitEase))
                .Insert(startAt, Tween.Scale(tile.transform, Vector3.zero, submitDuration, submitEase))
                .OnComplete(() =>
                {
                    homePositions.Remove(tileId);
                    running.Remove(tileId);

                    if (--remaining == 0)
                    {
                        onComplete?.Invoke();
                    }
                });

            Track(tile, sequence);
        }
    }

    /// <summary>
    /// Stops whatever the tile was doing and lifts it to the front. The board is
    /// authored with depth, but a moving tile should always draw over the rest.
    /// </summary>
    private void Prepare(Tile tile)
    {
        Sequence previous;
        if (running.TryGetValue(tile.TileData.Id, out previous) && previous.isAlive)
        {
            previous.Stop();
        }

        tile.transform.position = Flatten(tile.transform.position);
    }

    private void Track(Tile tile, Sequence sequence)
    {
        running[tile.TileData.Id] = sequence;
    }

    private static Vector3 Flatten(Vector3 position)
    {
        return new Vector3(position.x, position.y, 0f);
    }

    private void OnDestroy()
    {
        foreach (Sequence sequence in running.Values)
        {
            if (sequence.isAlive)
            {
                sequence.Stop();
            }
        }

        running.Clear();
        homePositions.Clear();
    }
}
