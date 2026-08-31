using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool for resetting player data and PlayerPrefs
/// </summary>
public class DataResetTool : EditorWindow
{
    [MenuItem("Tools/Reset Player Data")]
    public static void ResetPlayerData()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Reset Player Data", 
            "Are you sure you want to reset all player data and PlayerPrefs?\n\nThis action cannot be undone!", 
            "Yes, Reset All Data", 
            "Cancel"
        );
        
        if (confirmed)
        {
            ResetAllData();
        }
    }
    
    /// <summary>
    /// Reset all player data and PlayerPrefs
    /// </summary>
    private static void ResetAllData()
    {
        try
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            string savePath = Path.Combine(Application.persistentDataPath, "playerdata.json");
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("DataResetTool: Player data file deleted successfully");
            }
            
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            AssetDatabase.Refresh();
            
            Debug.Log("DataResetTool: All player data and PlayerPrefs have been reset successfully!");
                
            EditorUtility.DisplayDialog(
                "Reset Complete", 
                "All player data and PlayerPrefs have been reset successfully!\n\nYou may need to restart the game for changes to take effect.", 
                "OK"
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DataResetTool: Error resetting data: {e.Message}");
            EditorUtility.DisplayDialog(
                "Reset Failed", 
                $"Error resetting data: {e.Message}", 
                "OK"
            );
        }
    }
    
    /// <summary>
    /// Reset only PlayerPrefs
    /// </summary>
    [MenuItem("Tools/Reset PlayerPrefs Only")]
    public static void ResetPlayerPrefsOnly()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Reset PlayerPrefs", 
            "Are you sure you want to reset all PlayerPrefs?\n\nThis action cannot be undone!", 
            "Yes, Reset PlayerPrefs", 
            "Cancel"
        );
        
        if (confirmed)
        {
            try
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                
                Debug.Log("DataResetTool: PlayerPrefs have been reset successfully!");
                
                EditorUtility.DisplayDialog(
                    "Reset Complete", 
                    "All PlayerPrefs have been reset successfully!", 
                    "OK"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DataResetTool: Error resetting PlayerPrefs: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Reset Failed", 
                    $"Error resetting PlayerPrefs: {e.Message}", 
                    "OK"
                );
            }
        }
    }
    
    /// <summary>
    /// Reset only save file
    /// </summary>
    [MenuItem("Tools/Reset Save File Only")]
    public static void ResetSaveFileOnly()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Reset Save File", 
            "Are you sure you want to delete the save file?\n\nThis action cannot be undone!", 
            "Yes, Delete Save File", 
            "Cancel"
        );
        
        if (confirmed)
        {
            try
            {
                string savePath = Path.Combine(Application.persistentDataPath, "playerdata.json");
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log("DataResetTool: Save file deleted successfully");
                    
                    EditorUtility.DisplayDialog(
                        "Reset Complete", 
                        "Save file has been deleted successfully!", 
                        "OK"
                    );
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "No Save File", 
                        "No save file found to delete.", 
                        "OK"
                    );
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DataResetTool: Error deleting save file: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Reset Failed", 
                    $"Error deleting save file: {e.Message}", 
                    "OK"
                );
            }
        }
    }
} 