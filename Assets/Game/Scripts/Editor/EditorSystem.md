# EditorSystem

## Overview

EditorSystem is a Unity Editor development tools collection that provides essential development utilities for resetting and managing player data during development and testing. It offers convenient menu-based tools for developers to quickly reset player progress, PlayerPrefs, and save files without manually navigating to persistent data directories or registry entries.

## Why Needed

The system is essential for development workflow efficiency and testing scenarios. It provides quick and safe methods to reset player data during development, testing different game states, and debugging save/load functionality. Without it, developers would need to manually locate and delete persistent data files and PlayerPrefs, which is time-consuming and error-prone, especially across different platforms.

## Folder Structure

```
Editor/
└── DataResetTool.cs                 # Unity Editor tool for resetting player data
```

## Abstraction

### **Editor Tools**

#### **DataResetTool**

**Main Purpose**: Provides Unity Editor menu items for resetting player data, PlayerPrefs, and save files with confirmation dialogs for safe development workflow.

```csharp
public class DataResetTool : EditorWindow
{
    // Menu Items
    [MenuItem("Tools/Reset Player Data")]
    public static void ResetPlayerData();                // Resets all player data and PlayerPrefs

    [MenuItem("Tools/Reset PlayerPrefs Only")]
    public static void ResetPlayerPrefsOnly();           // Resets only PlayerPrefs

    [MenuItem("Tools/Reset Save File Only")]
    public static void ResetSaveFileOnly();              // Deletes only the save file

    // Core Methods
    private static void ResetAllData();                  // Internal method to reset all data
}
```

### **Data Structures**

This system doesn't define custom data structures as it works directly with Unity's PlayerPrefs and file system operations.

### **Usage Examples**

#### **Resetting All Player Data**
```csharp
// Available through Unity Editor menu: Tools/Reset Player Data
// This will:
// 1. Show confirmation dialog
// 2. Delete PlayerPrefs
// 3. Delete playerdata.json save file
// 4. Refresh AssetDatabase
// 5. Show completion dialog
```

#### **Resetting Only PlayerPrefs**
```csharp
// Available through Unity Editor menu: Tools/Reset PlayerPrefs Only
// This will reset all PlayerPrefs while keeping save files intact
```

#### **Resetting Only Save Files**
```csharp
// Available through Unity Editor menu: Tools/Reset Save File Only
// This will delete the playerdata.json file while keeping PlayerPrefs intact
```

## System Architecture and Data Flow

### Data Reset Flow
1. **User Selection**: Developer selects appropriate reset option from Unity Tools menu
2. **Confirmation**: System displays confirmation dialog to prevent accidental data loss
3. **Data Deletion**: Upon confirmation, system deletes specified data (PlayerPrefs, save files, or both)
4. **Asset Refresh**: System refreshes Unity's AssetDatabase if needed
5. **Completion Feedback**: System shows success/failure dialog with appropriate messaging

### Safety Features
- **Confirmation Dialogs**: All operations require user confirmation before execution
- **Error Handling**: Try-catch blocks around all operations with user-friendly error messages
- **Logging**: Debug logging for successful operations and errors
- **Selective Reset**: Options to reset specific data types rather than everything

## Internal Dependencies

- **Save System** - Interacts with the game's save file structure (playerdata.json)
- **Settings System** - May affect PlayerPrefs used by game settings

## External Dependencies

- **UnityEngine** - Core Unity functionality for PlayerPrefs and Application paths
- **UnityEditor** - Unity Editor API for menu items, dialogs, and AssetDatabase
- **System.IO** - .NET file system operations for save file management

---