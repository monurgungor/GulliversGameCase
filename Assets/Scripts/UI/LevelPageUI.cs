using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using PrimeTween;

/// <summary>
/// Handles UI for level page - creates and manages level slots
/// </summary>
public class LevelPageUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject levelSlotPrefab;
    [SerializeField] private Transform levelSlotContainer;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Animation Settings")]
    [SerializeField] private float scrollToLevelDuration = 1f;
    
    
    // Events
    public Action<string> OnLevelSelected;
    public Action OnLevelsLoaded;
    
    private List<LevelSlotUI> levelSlots = new List<LevelSlotUI>();
    private bool isInitialized = false;
    
    // Zenject injection
    [Inject] private LevelController levelController;
    
    private void Start()
    {
        CreateLevelSlots(levelController.GetAvailableLevels());
    }
    

    
    /// <summary>
    /// Create level slots for all available levels
    /// </summary>
    /// <param name="levels">List of LevelData</param>
    private void CreateLevelSlots(List<LevelData> levels)
    {
        if (levelSlotPrefab == null || levelSlotContainer == null)
        {
            Debug.LogError("LevelPage_UI: Level slot prefab or container not assigned!");
            return;
        }
        
        
        if (levels == null || levels.Count == 0) return;
        
        for (int i = 0; i < levels.Count; i++)
        {
            CreateLevelSlot(levels[i], i);
        }
        
        
        OnLevelsLoaded?.Invoke();
        isInitialized = true;
        
        Debug.Log($"LevelPage_UI: Created {levelSlots.Count} level slots");
    }
    
    /// <summary>
    /// Create a single level slot
    /// </summary>
    /// <param name="levelData">LevelData for the slot</param>
    /// <param name="index">Slot index for animation</param>
    private void CreateLevelSlot(LevelData levelData, int index)
    {
        GameObject levelSlotGO = Instantiate(levelSlotPrefab, levelSlotContainer);
        LevelSlotUI levelSlot = levelSlotGO.GetComponent<LevelSlotUI>();
        
        if (levelSlot == null)
        {
            Debug.LogError($"LevelPage_UI: LevelSlot_UI component not found on prefab!");
            return;
        }
        
        levelSlot.Initialize(levelData);
        
        
        levelSlots.Add(levelSlot);
        
    }
    
    
    
    /// <summary>
    /// Refresh all level slots
    /// </summary>
    public void RefreshLevelSlots()
    {
        if (levelController != null)
        {
            levelController.RefreshLevelSlots();
        }
    }
    
    
    /// <summary>
    /// Get all level slots
    /// </summary>
    /// <returns>List of level slots</returns>
    public List<LevelSlotUI> GetLevelSlots()
    {
        return new List<LevelSlotUI>(levelSlots);
    }
    
    /// <summary>
    /// Check if level page is initialized
    /// </summary>
    /// <returns>True if initialized</returns>
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    
    /// <summary>
    /// Check for completed levels and play unlock animations
    /// </summary>
    public void CheckForCompletedLevels()
    {
        var completedLevel = PlayerPrefs.GetInt("UnlockedLevel");

        if(completedLevel == 0) return;

        ScrollToLevel(completedLevel, () =>
                {
                    levelSlots[completedLevel-1].PlayUnlockAnimation();
                });
        
        PlayerPrefs.DeleteKey("UnlockedLevel");
        PlayerPrefs.Save();

    }
    
    /// <summary>
    /// Scroll to specific level in the scroll view
    /// </summary>
    /// <param name="levelId">Level ID to scroll to</param>
    /// <param name="onComplete">Callback when scroll is complete</param>
    private void ScrollToLevel(int levelId, System.Action onComplete = null)
    {
        if (scrollRect == null || levelSlots.Count == 0) 
        {
            onComplete?.Invoke();
            return;
        }
        
        int levelIndex = levelId - 1;
        if (levelIndex < 0 || levelIndex >= levelSlots.Count)
        {
            onComplete?.Invoke();
            return;
        }
        
        RectTransform content = scrollRect.content;
        RectTransform targetSlot = levelSlots[levelIndex].GetComponent<RectTransform>();
        
        float targetPosition = 1f - (float)levelIndex / (levelSlots.Count - 1);
        targetPosition = Mathf.Clamp01(targetPosition);
        
        Tween.Custom(scrollRect.verticalNormalizedPosition, targetPosition, scrollToLevelDuration, 
            onValueChange: value => scrollRect.verticalNormalizedPosition = value,
            ease: Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke());
    }
} 