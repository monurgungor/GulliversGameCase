using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "VisualSettings", menuName = "Settings/VisualSettings")]
public class VisualSettings : ScriptableObject
{
    [field:Header("Clickable Tiles")]
    [field:SerializeField] public Sprite openedSprite{get; private set;}
    [field:SerializeField] public Color openedColor{get; private set;}
    
    [field:Header("Potentially Clickable Tiles (can be opened by clickable tiles)")]
    [field:SerializeField] public Sprite closedSprite{get; private set;}
    
    [field:Header("Blocked Tiles")]
    [field:SerializeField] public Color closedColor {get; private set;}

}


