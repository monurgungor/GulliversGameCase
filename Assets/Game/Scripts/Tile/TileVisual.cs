using TMPro;
using UnityEngine;

/// <summary>
/// Draws one tile. The letter and score are sprite glyphs from the tile font,
/// and the texts are hidden while the tile is still covered.
/// </summary>
public class TileVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private TextMeshPro letterText;
    [SerializeField] private SpriteRenderer tileSprite;
    [SerializeField] private GameObject textsContainer;

    private void Awake()
    {
        if (textsContainer == null && scoreText != null)
        {
            textsContainer = scoreText.transform.parent.gameObject;
        }
    }

    public void Apply(TileState state, VisualSettings visualSettings)
    {
        if (visualSettings == null)
        {
            return;
        }

        switch (state)
        {
            case TileState.Clickable:
                tileSprite.sprite = visualSettings.openedSprite;
                tileSprite.color = visualSettings.openedColor;
                textsContainer.SetActive(true);
                break;

            case TileState.Potential:
                tileSprite.sprite = visualSettings.closedSprite;
                tileSprite.color = visualSettings.openedColor;
                textsContainer.SetActive(false);
                break;

            case TileState.Blocked:
                tileSprite.sprite = visualSettings.closedSprite;
                tileSprite.color = visualSettings.closedColor;
                textsContainer.SetActive(false);
                break;
        }
    }

    /// <summary>Two digit scores are drawn as two glyphs joined by a plus.</summary>
    public void SetScore(int score)
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.text = score >= 10
            ? $"<sprite index={score / 10}>+<sprite index={score % 10}>"
            : $"<sprite index={score}>";
    }

    public void SetLetter(char letter)
    {
        if (letterText != null)
        {
            letterText.text = $"<sprite name={letter}>";
        }
    }
}
