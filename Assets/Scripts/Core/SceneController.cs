using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

/// <summary>
/// Handles scene loading and game initialization
/// </summary>
public class SceneController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "PlayableLevel";
    
    [Header("Loading Settings")]
    [SerializeField] private bool showLoadingProgress = true;
    
    public static event Action<float> OnLoadingProgress;
    public static event Action OnLoadingComplete;
    public static event Action<string> OnLoadingError;
    public static event Action OnGameInitialized;
        
    [Inject] private SaveManager saveManager;
    
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    async void Start()
    {
        await InitializeGameAsync();
    }

    /// <summary>
    /// Initialize the game asynchronously
    /// </summary>
    public async Task InitializeGameAsync()
    {
        Debug.Log("SceneController: Starting game initialization...");

        try
        {
            await InitializeSaveManagerAsync();
            await LoadMainMenuAsync();
            isInitialized = true;
            OnGameInitialized?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneController: Error during initialization: {e.Message}");
            OnLoadingError?.Invoke($"Initialization failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Initialize SaveManager asynchronously
    /// </summary>
    private async Task InitializeSaveManagerAsync()
    {
        if (saveManager == null)
        {
            Debug.LogError("SceneController: SaveManager not found!");
            throw new InvalidOperationException("SaveManager not available");
        }
        
        await saveManager.InitializeAsync();
        Debug.Log("SceneController: SaveManager initialized");
    }
    
    /// <summary>
    /// Load main menu scene asynchronously
    /// </summary>
    private async Task LoadMainMenuAsync()
    {
        try
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainMenuSceneName);
            
            while (!asyncLoad.isDone)
            {
                OnLoadingProgress?.Invoke(0.5f + (asyncLoad.progress * 0.5f));
                await Task.Yield();
            }
            
            Debug.Log("SceneController: Main menu loaded");
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneController: Error loading main menu: {e.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Load game scene asynchronously
    /// </summary>
    /// <param name="levelName">Level name to load</param>
    public async Task LoadGameSceneAsync(int levelId)
    {
        try
        {
            
            PlayerPrefs.SetInt("LevelToLoad", levelId);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
            
            while (!asyncLoad.isDone)
            {
                OnLoadingProgress?.Invoke(asyncLoad.progress);
                await Task.Yield();
            }
            
            Debug.Log("SceneController: Game scene loaded");
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneController: Error loading game scene: {e.Message}");
            OnLoadingError?.Invoke($"Failed to load game scene: {e.Message}");
        }
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public async Task ReturnToMainMenuAsync()
    {
        try
        {
            Debug.Log("SceneController: Returning to main menu");
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainMenuSceneName);
            
            while (!asyncLoad.isDone)
            {
                OnLoadingProgress?.Invoke(asyncLoad.progress);
                await Task.Yield();
            }
            
            Debug.Log("SceneController: Returned to main menu");
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneController: Error returning to main menu: {e.Message}");
            OnLoadingError?.Invoke($"Failed to return to main menu: {e.Message}");
        }
    }
    
    /// <summary>
    /// Restart current scene
    /// </summary>
    public async Task RestartCurrentSceneAsync()
    {
        try
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"SceneController: Restarting scene: {currentSceneName}");
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentSceneName);
            
            while (!asyncLoad.isDone)
            {
                OnLoadingProgress?.Invoke(asyncLoad.progress);
                await Task.Yield();
            }
            
            Debug.Log("SceneController: Scene restarted");
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneController: Error restarting scene: {e.Message}");
            OnLoadingError?.Invoke($"Failed to restart scene: {e.Message}");
        }
    }
    
    /// <summary>
    /// Get current scene name
    /// </summary>
    /// <returns>Current scene name</returns>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Check if current scene is main menu
    /// </summary>
    /// <returns>True if current scene is main menu</returns>
    public bool IsMainMenuScene()
    {
        return GetCurrentSceneName() == mainMenuSceneName;
    }
    
    /// <summary>
    /// Check if current scene is game scene
    /// </summary>
    /// <returns>True if current scene is game scene</returns>
    public bool IsGameScene()
    {
        return GetCurrentSceneName() == gameSceneName;
    }
} 