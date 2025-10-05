# UISystem

## Overview

UISystem is a comprehensive UI management framework that handles popup creation, canvas management, and user interface lifecycle in the Word Tiles GO game. It provides a centralized service for managing UI elements, animations, layering, and user interactions across the entire application using a modular popup-based architecture with dependency injection support.

## Why Needed

The system is essential for managing complex UI interactions in a Unity-based mobile game. It provides centralized popup management, proper UI layering and sorting, animation coordination, and lifecycle management. Without it, UI elements would be scattered, difficult to manage, and prone to conflicts between overlapping interfaces, leading to poor user experience and maintenance challenges.

## Folder Structure

```
tempfold/UI/                           # Sample UI components from analyzed folder
├── UndoButton.cs                      # Specialized undo button with hold detection
├── LevelPageUI.cs                     # Level selection page management
├── LevelSlotUI.cs                     # Individual level slot components
├── MainMenuUI.cs                      # Main menu interface with animations
├── HighscorePanelUI.cs               # Game end score display panel
└── WordSectionUI.cs                  # In-game word input and display UI

Draft/Assets/Scripts/UI/               # Main UI system implementation
├── Management/                        # Core UI management services
│   ├── Abstraction/                  # Interface definitions
│   │   ├── IUIManagementService.cs   # Main UI service interface
│   │   ├── IBaseCanvasUI.cs          # Base canvas interface
│   │   ├── ICanvasUI.cs              # Generic canvas interface
│   │   └── IUILoadService.cs         # UI loading service interface
│   ├── DataStructures/               # UI data structures
│   │   └── UIOpenDataBase.cs         # Base popup data class
│   ├── UIManagementService.cs        # Main UI management implementation
│   ├── PopUpBase.cs                  # Base popup component class
│   └── UILoadService.cs              # UI loading service implementation
├── Buttons/                          # UI button components
│   └── Abstraction/                  # Button base classes
└── Installers/                       # Dependency injection setup
    └── UIInstaller.cs                # UI system DI configuration
```

## Abstraction

### **Interfaces**

#### **IUIManagementService**
Main Purpose: Central service interface for managing all UI popups, canvases, and their lifecycle including loading, opening, closing, and state tracking.

```csharp
public interface IUIManagementService
{
    GGDictionary<string, IBaseCanvasUI> UIDictionary { get; }    // Registry of all UI canvases
    List<CanvasInfo> CanvasInfos { get; set; }                  // Canvas configuration information

    // Loading and registration
    UniTask<bool> RequestLoadUIAsync(string identifier, CancellationToken token = default);    // Load UI asynchronously
    void RegisterToCanvas(string identifier, IBaseCanvasUI baseCanvasUI);                       // Register canvas to management

    // Opening and closing
    UniTask ImmediateRequestAsync<T>(string identifier, T data) where T : UIOpenDataBase;      // Open popup immediately
    void RequestClose(string identifier);                                                       // Close specific popup

    // State checking
    bool IsCanvasAlreadyOpened(string identifier);              // Check if canvas is open
    bool IsCanvasRegistered(string identifier);                 // Check if canvas is registered
    bool AnyPopupOpen();                                         // Check if any popup is open
    bool IsAnyCanvasInOpenQueue();                              // Check if any canvas is queued

    // Wait operations
    UniTask WaitUntilCanvasRegisteredAsync(string identifier);  // Wait for canvas registration
    UniTask WaitForCanvasToOpen(string identifier, CancellationToken token);  // Wait for canvas open
    UniTask WaitForCanvasToClose(string identifier, CancellationToken token); // Wait for canvas close
}
```

#### **IBaseCanvasUI**
Main Purpose: Base interface for all UI canvas components providing essential lifecycle methods and state properties.

```csharp
public interface IBaseCanvasUI
{
    bool OpenStatus { get; }                        // Current open/closed state
    bool IsOpenMethodCompleted { get; set; }        // Whether open animation completed

    PopUpOpenType GetOpenType();                     // Get popup opening behavior type
    UniTask WaitUntilInitialized();                 // Wait for canvas initialization
    void ManualAwake();                              // Manual initialization trigger
    void OnClosed();                                 // Called when canvas closes
    void Close();                                    // Close the canvas
    void Open();                                     // Open the canvas
}
```

#### **ICanvasUI<T>**
Main Purpose: Generic interface extending base canvas functionality with typed data support for popup-specific configurations.

```csharp
public interface ICanvasUI<in T> : IBaseCanvasUI
{
    void OnOpened(T canvasData);                     // Called when canvas opens with data
}
```

### **Singleton Services**

#### **UIManagementService**
Main Purpose: Central singleton service that manages all UI popups, handles opening/closing queues, manages canvas layering, and coordinates UI lifecycle events.

```csharp
public class UIManagementService : IUIManagementService, IDisposable
{
    // Core Properties
    public GGDictionary<string, IBaseCanvasUI> UIDictionary { get; set; }    // All registered canvases
    public List<CanvasInfo> CanvasInfos { get; set; }                        // Canvas configuration data

    // Core Methods
    UniTask<bool> RequestLoadUIAsync(string identifier, CancellationToken token);  // Load UI canvas
    UniTask ImmediateRequestAsync<T>(string identifier, T data);                   // Open popup with data
    void RequestClose(string identifier);                                          // Close specific popup
    bool AnyPopupOpen();                                                           // Check for open popups
    void RegisterToCanvas(string identifier, IBaseCanvasUI baseCanvasUI);         // Register new canvas
}
```

### **Data Structures**

#### **UIOpenDataBase**
Base data structure containing configuration and callbacks for popup opening behavior, including animation settings, sound effects, and lifecycle callbacks.

```csharp
public class UIOpenDataBase
{
    public readonly string PopupId;                    // Unique popup identifier
    public Action OnPopupClosed;                       // Callback when popup closes
    public Action OnPopupOpened;                       // Callback when popup opens
    public readonly Action<object> ManualCloseCallback; // Manual close callback with data
    public readonly PopUpOpenType OpenType;            // How popup should open (default/additive)
    public bool CanCloseWithTint;                      // Whether tint tap closes popup
    public readonly bool PlaySfx;                      // Whether to play sound effects
    public readonly ActionTintType TintType;           // Type of background tint
    public readonly bool BlockLink;                    // Whether to block popup linking
}
```

#### **PopUpBase<T>**
Abstract base class for all popup implementations providing standard lifecycle management, animation coordination, and dependency injection integration.

```csharp
public abstract class PopUpBase<T> : LifetimeMonoBehaviour, ICanvasUI<T> where T : UIOpenDataBase
{
    public abstract PopupIds PopupId { get; }          // Unique popup identifier
    public bool OpenStatus { get; private set; }       // Current open/closed state
    public T PopUpData;                                 // Current popup data

    // Key methods
    public virtual void Open();                         // Open popup with animation
    public void Close();                                // Close popup immediately
    protected virtual void OnUIOpened(T data);         // Override for open logic
    protected virtual void OnUIClosed();               // Override for close logic
    protected void RequestClose();                     // Request popup closure
}
```

### **Usage Examples**

#### **Opening a Popup**
```csharp
// Open a popup with custom data
var uiService = container.Resolve<IUIManagementService>();
var popupData = new CustomPopupData("MyPopup", playSfx: true);
await uiService.ImmediateRequestAsync("MyPopup", popupData);
```

#### **Checking Popup State**
```csharp
// Check if any popups are currently open
var uiService = container.Resolve<IUIManagementService>();
bool hasOpenPopups = uiService.AnyPopupOpen();
```

#### **Waiting for Popup Events**
```csharp
// Wait for a specific popup to close
var uiService = container.Resolve<IUIManagementService>();
await uiService.WaitForCanvasToClose("MyPopup", cancellationToken);
```

## System Architecture and Data Flow

### Popup Lifecycle Management
1. **Registration**: UI canvases register themselves with UIManagementService during Awake
2. **Loading**: UILoadService handles addressable loading of UI prefabs asynchronously
3. **Opening**: PopupBase handles animation, tinting, and state management during open
4. **Queue Management**: Service manages popup queues for regular and additive popups
5. **Closing**: Coordinated cleanup of animations, tints, and state restoration

### Canvas Layering System
- **Regular Popups**: Use default sorting with queue-based management
- **Additive Popups**: Use special sorting layer with incremental ordering
- **Tint Management**: Background tinting controlled by popup type and settings
- **Animation Coordination**: PrimeTween integration for smooth transitions

## Internal Dependencies

- **ActionSystem** - For managing UI-triggered actions and events
- **MainScene.GGTintController** - For background tinting and visual effects
- **Services.Sound** - For UI sound effects and audio feedback
- **UI.Management.Components** - For shared UI component functionality
- **Global.GlobalDataStructuress** - For shared data structures and enums

## External Dependencies

- **PrimeTween** - Animation and tweening library for UI transitions
- **UniTask** - Asynchronous programming utilities for UI operations
- **Zenject** - Dependency injection framework for service management
- **TextMeshPro** - Advanced text rendering for UI elements