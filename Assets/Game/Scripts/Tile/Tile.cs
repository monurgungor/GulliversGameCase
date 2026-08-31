using UnityEngine;

/// <summary>How a tile reads on the board.</summary>
public enum TileState
{
    /// <summary>Free to tap: letter and score are shown.</summary>
    Clickable,

    /// <summary>Covered, but one of the tiles covering it can be tapped now.</summary>
    Potential,

    /// <summary>Covered with nothing playable above it.</summary>
    Blocked,
}

/// <summary>
/// One letter on the board. It holds its data and hands presentation to
/// <see cref="TileVisual"/>; TileManager decides what state it should be in.
/// </summary>
public class Tile : MonoBehaviour
{
    [SerializeField] private TileVisual tileVisual;

    private Collider2D tileCollider;

    public TileData TileData { get; private set; }

    private void Awake()
    {
        tileCollider = GetComponent<Collider2D>();
    }

    public void Initialize(TileData tileData)
    {
        TileData = tileData;
        tileVisual.SetScore(tileData.Score);
        tileVisual.SetLetter(tileData.Character);
    }

    public void SetState(TileState state, VisualSettings visualSettings)
    {
        tileVisual.Apply(state, visualSettings);
    }

    /// <summary>
    /// Only tiles that can be played keep a collider, so the tap test never has
    /// to reason about tiles the player cannot take.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (tileCollider != null)
        {
            tileCollider.enabled = interactable;
        }
    }

    public void MoveTo(Vector3 position)
    {
        transform.position = position;
    }
}
