using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>One level as the menu needs to know it.</summary>
public readonly struct LevelInfo
{
    public readonly int Id;
    public readonly string Title;

    public LevelInfo(int id, string title)
    {
        Id = id;
        Title = title;
    }
}

/// <summary>
/// The single place that knows where level files live. Titles are read once and
/// cached, and the text assets are released again, so opening the menu does not
/// keep twenty json files resident.
/// </summary>
public static class LevelCatalog
{
    private const string LevelsFolder = "Levels";
    private const string LevelFilePrefix = "level_";

    private static List<LevelInfo> levels;

    /// <summary>Every level found in the project, ordered by id.</summary>
    public static IReadOnlyList<LevelInfo> Levels
    {
        get
        {
            if (levels == null)
            {
                levels = ReadAll();
            }

            return levels;
        }
    }

    /// <summary>Reads one level's board. Returns null when the file is missing or broken.</summary>
    public static LevelJsonData LoadLevel(int levelId)
    {
        var asset = Resources.Load<TextAsset>($"{LevelsFolder}/{LevelFilePrefix}{levelId}");

        if (asset == null)
        {
            Debug.LogError($"LevelCatalog: no level file for id {levelId}.");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<LevelJsonData>(asset.text);
        }
        catch (JsonException exception)
        {
            Debug.LogError($"LevelCatalog: level {levelId} is not valid json: {exception.Message}");
            return null;
        }
        finally
        {
            Resources.UnloadAsset(asset);
        }
    }

    private static List<LevelInfo> ReadAll()
    {
        var found = new List<LevelInfo>();
        TextAsset[] assets = Resources.LoadAll<TextAsset>(LevelsFolder);

        foreach (TextAsset asset in assets)
        {
            if (!asset.name.StartsWith(LevelFilePrefix, System.StringComparison.Ordinal))
            {
                continue;
            }

            int levelId;
            if (!int.TryParse(asset.name.Substring(LevelFilePrefix.Length), out levelId) || levelId <= 0)
            {
                continue;
            }

            found.Add(new LevelInfo(levelId, ReadTitle(asset)));
        }

        found.Sort((left, right) => left.Id.CompareTo(right.Id));

        foreach (TextAsset asset in assets)
        {
            Resources.UnloadAsset(asset);
        }

        return found;
    }

    private static string ReadTitle(TextAsset asset)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<LevelJsonData>(asset.text);
            return string.IsNullOrEmpty(data?.title) ? asset.name : data.title;
        }
        catch (JsonException)
        {
            Debug.LogWarning($"LevelCatalog: could not read a title from {asset.name}.");
            return asset.name;
        }
    }
}
