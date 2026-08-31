using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

/// <summary>
/// Brings the game up and moves it between scenes. It lives on the project
/// context, so it outlives every scene and is the only thing that needs to know
/// their names.
/// </summary>
public class SceneController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "Gameplay";

    [Inject] private SaveManager saveManager;

    /// <summary>Progress of the current scene load, from 0 to 1.</summary>
    public static event Action<float> LoadProgressChanged;

    private async void Start()
    {
        Application.targetFrameRate = 60;

        await saveManager.LoadAsync();
        await LoadMainMenuAsync();
    }

    public Task LoadMainMenuAsync() => LoadSceneAsync(mainMenuSceneName);

    public Task LoadGameSceneAsync() => LoadSceneAsync(gameSceneName);

    private static async Task LoadSceneAsync(string sceneName)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);

        if (load == null)
        {
            Debug.LogError($"SceneController: scene '{sceneName}' is not in the build settings.");
            return;
        }

        while (!load.isDone)
        {
            LoadProgressChanged?.Invoke(load.progress);
            await Task.Yield();
        }

        LoadProgressChanged?.Invoke(1f);
    }
}
