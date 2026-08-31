using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Spawns a level's tiles and centres the board and camera on them, so layouts
/// of different sizes all sit in the middle of the screen.
/// </summary>
public class TilePlacer : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform board;
    [SerializeField] private Transform tilesParent;

    [Tooltip("Level files are authored on a coarse grid; this scales it to world units.")]
    [SerializeField] private float positionScale = 0.5f;

    [Inject] private TileManager tileManager;
    [Inject] private TileAnimationManager tileAnimationManager;

    public float PositionScale => positionScale;

    public void PlaceTiles(TileData[] tileData)
    {
        if (tileData == null || tileData.Length == 0)
        {
            return;
        }

        var tiles = new Dictionary<int, Tile>(tileData.Length);

        foreach (TileData data in tileData)
        {
            Tile tile = Spawn(data);
            if (tile != null)
            {
                tiles[data.Id] = tile;
            }
        }

        CentreOn(tiles);
        tileManager.SetTiles(tiles);
    }

    private Tile Spawn(TileData data)
    {
        Tile tile = Instantiate(tilePrefab, tilesParent).GetComponent<Tile>();

        if (tile == null)
        {
            Debug.LogError("TilePlacer: the tile prefab has no Tile component.", this);
            return null;
        }

        tile.name = $"Tile_{data.Id}_{data.Character}";
        tile.transform.localPosition = data.Position;
        tile.Initialize(data);
        tileAnimationManager.StoreHomePosition(tile);

        return tile;
    }

    private void CentreOn(Dictionary<int, Tile> tiles)
    {
        if (tiles.Count == 0)
        {
            return;
        }

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        foreach (Tile tile in tiles.Values)
        {
            min = Vector3.Min(min, tile.transform.position);
            max = Vector3.Max(max, tile.transform.position);
        }

        Vector3 centre = (min + max) * 0.5f;

        if (board != null)
        {
            board.position = centre;
        }

        Camera boardCamera = Camera.main;
        if (boardCamera != null)
        {
            Vector3 cameraPosition = boardCamera.transform.position;
            boardCamera.transform.position = new Vector3(centre.x, centre.y, cameraPosition.z);
        }
    }
}
