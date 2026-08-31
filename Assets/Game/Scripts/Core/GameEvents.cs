using System;
using UnityEngine;

/// <summary>
/// Centralized game events for decoupled communication
/// </summary>
public static class GameEvents
{
    public static event Action OnGameCompleted;
    public static event Action<DeadlockResult> OnDeadlockDetected;
    
    /// <summary>
    /// Trigger game completion event
    /// </summary>
    public static void TriggerGameCompleted()
    {
        OnGameCompleted?.Invoke();
    }
    
    
    /// <summary>
    /// Trigger deadlock detection result event
    /// </summary>
    public static void TriggerDeadlockDetected(DeadlockResult result)
    {
        OnDeadlockDetected?.Invoke(result);
        
        if (result.allTilesUsed || result.isDeadlocked)
        {
            TriggerGameCompleted();
        }
    }
    
    

}