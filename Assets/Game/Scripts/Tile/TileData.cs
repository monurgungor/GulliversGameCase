using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TileData 
{
    public int Id{get; private set;}
    public char Character{get; private set;}
    public Vector3 Position{get; private set;}
    public int[] Children{get; private set;}

    public int Score {get; private set;}

    public TileData(int id, char character, Vector3 position, int[] children, int score)
    {
        this.Id = id;
        this.Character = character;
        this.Position = position;
        this.Children = children;
        this.Score = score;
    }
}
