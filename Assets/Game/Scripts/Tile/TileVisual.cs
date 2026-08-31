using UnityEngine;
using TMPro;

/// <summary>
/// Handles all visual operations for tiles
/// </summary>
public class TileVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private TextMeshPro letterText;
    [SerializeField] private SpriteRenderer tileSprite;
    private GameObject textsContainer;

    void Awake()
    {
        textsContainer = scoreText.transform.parent.gameObject;
    }

    /// <summary>
    /// Set the sprite of the tile
    /// </summary>
    /// <param name="sprite">Sprite to set</param>
    public void SetSprite(Sprite sprite)
    {
        if (tileSprite != null && sprite != null)
        {
            tileSprite.sprite = sprite;
        }
    }

    /// <summary>
    /// Set the color of the tile
    /// </summary>
    /// <param name="color">Color to set</param>
    public void SetColor(Color color)
    {
        if (tileSprite != null)
        {
            tileSprite.color = color;
        }
    }

    /// <summary>
    /// Apply visual state to the tile
    /// </summary>
    /// <param name="state">Tile state to apply</param>
    /// <param name="visualSettings">Visual settings to use</param>
    public void ApplyVisualForState(TileState state, VisualSettings visualSettings)
    {
        if (visualSettings == null) return;
        
        switch (state)
        {
            case TileState.Clickable:
                SetSprite(visualSettings.openedSprite);
                SetColor(visualSettings.openedColor);
                textsContainer.SetActive(true);
                break;
            case TileState.Potential:
                SetSprite(visualSettings.closedSprite);
                SetColor(visualSettings.openedColor);
                textsContainer.SetActive(false);
                break;
            case TileState.Blocked:
                SetSprite(visualSettings.closedSprite);
                SetColor(visualSettings.closedColor);
                textsContainer.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Set the score text
    /// </summary>
    /// <param name="score">Score value to display</param>
    public void SetScoreText(int score)
    {
        if (scoreText != null)
        {
            if (score >= 10)
            {
                int tensDigit = score / 10;
                int onesDigit = score % 10;
                scoreText.text = $"<sprite index={tensDigit}>+<sprite index={onesDigit}>";
            }
            else
            {
                scoreText.text = $"<sprite index={score}>";
            }
        }
    }

    /// <summary>
    /// Set the letter text
    /// </summary>
    /// <param name="letter">Letter to display</param>
    public void SetLetterText(char letter)
    {
        if (letterText != null)
        {
            letterText.text = $"<sprite name={letter}>";
        }
    }

    /// <summary>
    /// Move the tile to a new local position
    /// </summary>
    /// <param name="newPosition">New local position</param>
    public void MoveLocalPosition(Vector3 newPosition)
    {
        transform.localPosition = newPosition;
    }
} 