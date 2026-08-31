using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Turns a tap on the board into a tile. Only free tiles keep their collider
/// enabled, so the point test can only ever return a playable tile; when a tap
/// lands on two of them the one nearest the camera wins, which is the one drawn
/// on top.
/// </summary>
public class TileClickHandler : MonoBehaviour
{
    private const int MaxOverlappingTiles = 8;

    [Tooltip("Ignores taps that arrive faster than this, which filters double taps.")]
    [SerializeField] private float clickCooldown = 0.1f;

    private readonly List<Collider2D> hits = new List<Collider2D>(MaxOverlappingTiles);
    private ContactFilter2D tileFilter;

    private Camera boardCamera;
    private float lastClickTime;

    public static event Action<Tile> TileClicked;

    private void Awake()
    {
        boardCamera = Camera.main;
        tileFilter = new ContactFilter2D { useTriggers = true };
        tileFilter.ClearLayerMask();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && !IsOverUI(touch.fingerId))
            {
                Pick(touch.position);
            }

            return;
        }

        if (Input.GetMouseButtonUp(0) && !IsOverUI(-1))
        {
            Pick(Input.mousePosition);
        }
    }

    private static bool IsOverUI(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    private void Pick(Vector2 screenPosition)
    {
        if (boardCamera == null || Time.unscaledTime - lastClickTime < clickCooldown)
        {
            return;
        }

        lastClickTime = Time.unscaledTime;

        Vector2 worldPosition = boardCamera.ScreenToWorldPoint(screenPosition);
        Physics2D.OverlapPoint(worldPosition, tileFilter, hits);

        Tile topmost = null;
        float nearestDepth = float.MaxValue;

        for (int i = 0; i < hits.Count; i++)
        {
            Tile tile = hits[i].GetComponent<Tile>();
            if (tile == null)
            {
                continue;
            }

            float depth = tile.transform.position.z;
            if (depth < nearestDepth)
            {
                nearestDepth = depth;
                topmost = tile;
            }
        }

        if (topmost != null)
        {
            TileClicked?.Invoke(topmost);
        }
    }
}
