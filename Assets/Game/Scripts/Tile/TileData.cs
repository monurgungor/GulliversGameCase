using System;
using UnityEngine;

/// <summary>
/// One tile as a level file describes it. Children are the tiles this one
/// covers, which is what makes the board a stack rather than a grid.
/// </summary>
[Serializable]
public readonly struct TileData
{
    public int Id { get; }
    public char Character { get; }
    public Vector3 Position { get; }
    public int[] Children { get; }
    public int Score { get; }

    public TileData(int id, char character, Vector3 position, int[] children, int score)
    {
        Id = id;
        Character = character;
        Position = position;
        Children = children;
        Score = score;
    }
}
