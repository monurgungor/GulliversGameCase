using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Tile : MonoBehaviour, IClickable
{
    private TileData tileData;
    private TileState currentState;

    public TileState CurrentState => currentState;

    [Header("Tile Visual Component")]
    [SerializeField] private TileVisual tileVisual;

    public void Initialize(TileData tileData)
    {
        this.tileData = tileData;
        tileVisual.SetScoreText(tileData.Score);
        tileVisual.SetLetterText(tileData.Character);
    }

    public void ChangeSprite(Sprite newSprite)
    {
        tileVisual.SetSprite(newSprite);
    }

    /// <summary>
    /// Set tile sprite for visual state
    /// </summary>
    /// <param name="sprite">Sprite to set</param>
    public void SetSprite(Sprite sprite)
    {
        tileVisual.SetSprite(sprite);
    }

    /// <summary>
    /// Set tile color for visual state
    /// </summary>
    /// <param name="color">Color to set</param>
    public void SetColor(Color color)
    {
        tileVisual.SetColor(color);
    }

    /// <summary>
    /// Set tile state and apply visual changes
    /// </summary>
    /// <param name="newState">The new state to set</param>
    /// <param name="visualSettings">Visual settings to use</param>
    public void SetState(TileState newState, VisualSettings visualSettings)
    {
        currentState = newState;

        SetClickable(newState == TileState.Clickable);

        tileVisual.ApplyVisualForState(newState, visualSettings);
    }

    public TileData TileData => tileData;

    public void OnClick()
    {
        TileClickHandler.OnTileClicked?.Invoke(this);
    }

    /// <summary>
    /// Set whether this tile can be clicked
    /// </summary>
    /// <param name="clickable">True if tile should be clickable</param>
    public void SetClickable(bool clickable)
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = clickable;
        }
    }


    public void MovePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void MoveLocalPosition(Vector3 newPosition)
    {
        tileVisual.MoveLocalPosition(newPosition);
    }


}

    public enum TileState
    {
        Clickable,
        Potential, 
        Blocked
    }


