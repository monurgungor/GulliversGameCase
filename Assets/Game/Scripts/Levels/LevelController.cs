using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Holds what the player has done with each level and is the way the menu asks
/// for one to be opened. Levels are keyed by id rather than by list position,
/// so a gap in the level files cannot shift everyone's progress.
/// </summary>
public class LevelController : MonoBehaviour
{
    [Inject] private SaveManager saveManager;
    [Inject] private SceneController sceneController;

    private readonly Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>();
    private readonly List<LevelData> orderedLevels = new List<LevelData>();

    private PlayerData playerData;

    /// <summary>Raised by the menu when the player picks a level.</summary>
    public static event Action<int> LevelRequested;

    /// <summary>
    /// Raised whenever the level list is rebuilt. The menu is loaded before the
    /// save file has finished reading when the gameplay scene is entered
    /// directly, so the list it draws has to be able to arrive late.
    /// </summary>
    public static event Action LevelsChanged;

    public static void RequestLevel(int levelId) => LevelRequested?.Invoke(levelId);

    private void OnEnable()
    {
        SaveManager.DataLoaded += OnDataLoaded;
        LevelRequested += LoadLevelScene;
    }

    private void OnDisable()
    {
        SaveManager.DataLoaded -= OnDataLoaded;
        LevelRequested -= LoadLevelScene;
    }

    private void Start()
    {
        if (saveManager.IsDataLoaded)
        {
            OnDataLoaded(saveManager.PlayerData);
        }
    }

    private void OnDataLoaded(PlayerData data)
    {
        playerData = data;
        BuildLevelList();
    }

    /// <summary>Every level in the project with the player's progress folded in.</summary>
    public IReadOnlyList<LevelData> Levels => orderedLevels;

    public int GetHighScore(int levelId)
    {
        LevelData level;
        return levels.TryGetValue(levelId, out level) ? level.highScore : 0;
    }

    public string GetTitle(int levelId)
    {
        LevelData level;
        return levels.TryGetValue(levelId, out level) ? level.levelTitle : string.Empty;
    }

    /// <summary>
    /// Records a finished level. The next level unlocks whatever the score was:
    /// finishing is what earns it, beating your own record is a separate thing.
    /// </summary>
    public void CompleteLevel(int levelId, int score)
    {
        LevelData level;
        if (!levels.TryGetValue(levelId, out level))
        {
            return;
        }

        level.isUnlocked = true;
        level.highScore = Mathf.Max(level.highScore, score);

        LevelData next;
        if (levels.TryGetValue(levelId + 1, out next) && !next.isUnlocked)
        {
            next.isUnlocked = true;
            LevelProgress.MarkJustUnlocked(next.levelId);
        }

        playerData.Apply(orderedLevels);
        saveManager.Save();
    }

    public async void LoadLevelScene(int levelId)
    {
        LevelData level;
        if (!levels.TryGetValue(levelId, out level) || !level.isUnlocked)
        {
            return;
        }

        LevelProgress.RequestLevel(levelId);
        await sceneController.LoadGameSceneAsync();
    }

    public async void ReturnToMainMenu()
    {
        await sceneController.LoadMainMenuAsync();
    }

    private void BuildLevelList()
    {
        levels.Clear();
        orderedLevels.Clear();

        foreach (LevelInfo info in LevelCatalog.Levels)
        {
            bool unlocked = info.Id == 1 || playerData.IsLevelUnlocked(info.Id);
            var level = new LevelData(info.Id, info.Title, playerData.GetHighScore(info.Id), unlocked);

            levels[info.Id] = level;
            orderedLevels.Add(level);
        }

        LevelsChanged?.Invoke();
    }
}
