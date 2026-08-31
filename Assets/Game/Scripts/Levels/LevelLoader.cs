using System;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

/// <summary>Shape of a level file on disk.</summary>
[Serializable]
public class LevelJsonData
{
    public string title;
    public TileData[] tiles;
}

/// <summary>
/// Reads the level the player picked and hands its tiles to TilePlacer.
/// Levels load through Resources rather than the file system, which is the only
/// form that also works inside a player build.
/// </summary>
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private TilePlacer tilePlacer;

    [Inject] private LetterSettings letterSettings;

    public static event Action<int> LevelLoaded;

    private void Start()
    {
        LoadLevel(LevelProgress.ConsumeRequestedLevel());
    }

    public void LoadLevel(int levelId)
    {
        LevelJsonData levelData = LevelCatalog.LoadLevel(levelId);

        if (levelData == null || levelData.tiles == null)
        {
            Debug.LogError($"LevelLoader: level {levelId} could not be read.");
            return;
        }

        tilePlacer.PlaceTiles(ToTiles(levelData.tiles));
        LevelLoaded?.Invoke(levelId);
    }

    /// <summary>
    /// Level files store board coordinates and letters. Scores come from
    /// LetterSettings so the two never disagree, and positions are scaled to
    /// world units here rather than in the placer.
    /// </summary>
    private TileData[] ToTiles(TileData[] source)
    {
        float scale = tilePlacer.PositionScale;
        var tiles = new TileData[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            TileData tile = source[i];
            tiles[i] = new TileData(
                tile.Id,
                tile.Character,
                tile.Position * scale,
                tile.Children ?? Array.Empty<int>(),
                letterSettings.GetLetterScore(tile.Character));
        }

        return tiles;
    }
}
