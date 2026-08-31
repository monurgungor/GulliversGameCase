using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// The level list. It fills the scroll view once, then scrolls to and opens the
/// lock on whichever level the player just unlocked.
/// </summary>
public class LevelPageUI : MonoBehaviour
{
    [SerializeField] private LevelSlotUI levelSlotPrefab;
    [SerializeField] private Transform levelSlotContainer;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private float scrollDuration = 1f;

    [Inject] private LevelController levelController;

    private readonly List<LevelSlotUI> slots = new List<LevelSlotUI>();

    private void Start()
    {
        foreach (LevelData level in levelController.Levels)
        {
            LevelSlotUI slot = Instantiate(levelSlotPrefab, levelSlotContainer);
            slot.Initialize(level);
            slots.Add(slot);
        }
    }

    /// <summary>
    /// Called once the level panel has slid in: brings the newly unlocked level
    /// into view and plays its unlock animation.
    /// </summary>
    public void CelebrateNewlyUnlockedLevel()
    {
        int levelId = LevelProgress.ConsumeJustUnlocked();
        int index = levelId - 1;

        if (index < 0 || index >= slots.Count)
        {
            return;
        }

        ScrollTo(index, () => slots[index].PlayUnlockAnimation());
    }

    private void ScrollTo(int index, System.Action onArrived)
    {
        if (slots.Count < 2)
        {
            onArrived();
            return;
        }

        float target = Mathf.Clamp01(1f - (float)index / (slots.Count - 1));

        Tween.Custom(
                scrollRect.verticalNormalizedPosition,
                target,
                scrollDuration,
                onValueChange: value => scrollRect.verticalNormalizedPosition = value,
                ease: Ease.OutQuad)
            .OnComplete(onArrived);
    }
}
