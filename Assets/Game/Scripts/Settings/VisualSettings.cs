using UnityEngine;

/// <summary>How a tile looks in each of its three states.</summary>
[CreateAssetMenu(fileName = "VisualSettings", menuName = "Word Game/Visual Settings")]
public class VisualSettings : ScriptableObject
{
    [Header("Free tiles")]
    [field: SerializeField] public Sprite openedSprite { get; private set; }
    [field: SerializeField] public Color openedColor { get; private set; }

    [Header("Covered tiles")]
    [field: SerializeField] public Sprite closedSprite { get; private set; }
    [field: SerializeField] public Color closedColor { get; private set; }
}
