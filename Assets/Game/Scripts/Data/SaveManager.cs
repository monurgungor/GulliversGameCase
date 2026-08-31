using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Reads and writes the save file. Loading happens off the main thread at
/// startup; writing is rare and small, so it stays synchronous and is triggered
/// whenever the app loses focus, which is the last moment a phone reliably gives
/// us before the process can be killed.
/// </summary>
public class SaveManager : MonoBehaviour
{
    [SerializeField] private string saveFileName = "playerdata.json";

    public static event Action<PlayerData> DataLoaded;

    // Resolved on the main thread in Awake, because Application.persistentDataPath
    // cannot be read from the thread the load runs on.
    private string saveFilePath;

    public PlayerData PlayerData { get; private set; }

    public bool IsDataLoaded { get; private set; }

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
    }

    public async Task LoadAsync()
    {
        PlayerData = await Task.Run(ReadFromDisk);
        IsDataLoaded = true;
        DataLoaded?.Invoke(PlayerData);
    }

    public void Save()
    {
        if (!IsDataLoaded)
        {
            return;
        }

        try
        {
            File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(PlayerData, Formatting.Indented));
        }
        catch (Exception exception)
        {
            Debug.LogError($"SaveManager: could not write the save file: {exception.Message}");
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Save();
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private PlayerData ReadFromDisk()
    {
        try
        {
            if (!File.Exists(saveFilePath))
            {
                return new PlayerData();
            }

            return JsonConvert.DeserializeObject<PlayerData>(File.ReadAllText(saveFilePath)) ?? new PlayerData();
        }
        catch (Exception exception)
        {
            Debug.LogError($"SaveManager: save file unreadable, starting fresh: {exception.Message}");
            return new PlayerData();
        }
    }
}
