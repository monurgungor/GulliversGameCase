using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WordActions 
{
    public static Action<bool> OnWordValidityChanged;
    public static Action<int> OnWordScoreChanged;
    
    public static Action<string> OnWordSubmitted;
    public static Action OnWordSubmitRequested;
    public static Action<int> OnScoreAdded;
    
    public static Action OnWordUndo;
    public static Action<bool> OnWordUndoWithType;

    public static Action<bool> OnUndoAvailabilityChanged;
}
