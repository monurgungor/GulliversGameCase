using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public class TileAnimationManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float selectAnimationDuration = 0.5f;
    [SerializeField] private float undoAnimationDuration = 0.4f;
    [SerializeField] private float submitAnimationDuration = 0.8f;
    [SerializeField] private float submitDelayBetweenTiles = 0.1f;
    
    [Header("Animation Easing")]
    [SerializeField] private Ease selectEase = Ease.OutBack;
    [SerializeField] private Ease undoEase = Ease.OutQuad;
    [SerializeField] private Ease submitEase = Ease.InBack;
    
    [Header("Submit Animation")]
    [SerializeField] private RectTransform submitTargetTransform;
    [SerializeField] private Vector3 submitExitOffset = new Vector3(0, 10f, 0);
    [SerializeField] private bool enableSubmitScale = true;
    [SerializeField] private Vector3 submitScaleTarget = Vector3.zero;
    
    private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();
    private Dictionary<int, object> activeTweens = new Dictionary<int, object>();
    
    public static event Action<Tile> OnSelectAnimationComplete;
    public static event Action<Tile> OnUndoAnimationComplete;
    public static event Action<List<Tile>> OnSubmitAnimationComplete;
    
    public void StoreOriginalPosition(Tile tile)
    {
        if (tile != null)
        {
            int tileId = tile.TileData.Id;
            originalPositions[tileId] = tile.transform.position;
        }
    }
    
    public void AnimateSelect(Tile tile, Vector3 targetPosition, Action onComplete = null)
    {
        if (tile == null) return;
        
        int tileId = tile.TileData.Id;
        
        CancelTileAnimation(tileId);
        
        if (!originalPositions.ContainsKey(tileId))
        {
            StoreOriginalPosition(tile);
        }
        
        Vector3 animationTarget = new Vector3(targetPosition.x, targetPosition.y, 0f);
        tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, 0f);
        
        Tween selectTween = Tween.Position(tile.transform, animationTarget, selectAnimationDuration, selectEase)
            .OnComplete(() =>
            {
                activeTweens.Remove(tileId);
                OnSelectAnimationComplete?.Invoke(tile);
                onComplete?.Invoke();
            });
        
        activeTweens[tileId] = selectTween;
    }
    
    public void AnimateUndo(Tile tile, Action onComplete = null)
    {
        if (tile == null) return;
        
        int tileId = tile.TileData.Id;
        
        CancelTileAnimation(tileId);
        
        Vector3 originalPosition;
        if (originalPositions.ContainsKey(tileId))
        {
            originalPosition = originalPositions[tileId];
        }
        else
        {
            originalPosition = tile.TileData.Position;
            Debug.LogWarning($"TileAnimationManager: Original position not found for tile {tileId}, using TileData.Position");
        }
        
        tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, 0f);
        
        Tween undoTween = Tween.Position(tile.transform, originalPosition, undoAnimationDuration, undoEase)
            .OnComplete(() =>
            {
                activeTweens.Remove(tileId);
                OnUndoAnimationComplete?.Invoke(tile);
                onComplete?.Invoke();
            });
        
        activeTweens[tileId] = undoTween;
    }
    
    public void AnimateUndoMultiple(List<Tile> tiles, Action onComplete = null)
    {
        if (tiles == null || tiles.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }
        
        int completedAnimations = 0;
        int totalAnimations = tiles.Count;
        
        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            float delay = i * 0.05f;
            
            Tween.Delay(delay).OnComplete(() =>
            {
                AnimateUndo(tile, () =>
                {
                    completedAnimations++;
                    if (completedAnimations >= totalAnimations)
                    {
                        onComplete?.Invoke();
                    }
                });
            });
        }
    }
    
    public void AnimateSubmit(List<Tile> tiles, Action onComplete = null)
    {
        if (tiles == null || tiles.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }
        
        int completedAnimations = 0;
        int totalAnimations = tiles.Count;
        
        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            float delay = i * submitDelayBetweenTiles;
            
            Tween.Delay(delay).OnComplete(() =>
            {
                AnimateSingleTileSubmit(tile, () =>
                {
                    completedAnimations++;
                    if (completedAnimations >= totalAnimations)
                    {
                        OnSubmitAnimationComplete?.Invoke(tiles);
                        onComplete?.Invoke();
                    }
                });
            });
        }
    }
    
    private void AnimateSingleTileSubmit(Tile tile, Action onComplete = null)
    {
        if (tile == null) return;
        
        int tileId = tile.TileData.Id;
        
        CancelTileAnimation(tileId);
        
        tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, 0f);
        
        Vector3 exitPosition;
        if (submitTargetTransform != null)
        {
            exitPosition = submitTargetTransform.position;
            exitPosition.z = 0f;
        }
        else
        {
            exitPosition = tile.transform.position + submitExitOffset;
        }
        
        Sequence submitSequence = Sequence.Create();
        
        submitSequence.Chain(Tween.Position(tile.transform, exitPosition, submitAnimationDuration, submitEase));
        
        if (enableSubmitScale)
        {
            submitSequence.Group(Tween.Scale(tile.transform, submitScaleTarget, submitAnimationDuration, submitEase));
        }
        
        submitSequence.OnComplete(() =>
        {
            activeTweens.Remove(tileId);
            originalPositions.Remove(tileId);
            onComplete?.Invoke();
        });
        
        activeTweens[tileId] = submitSequence;
    }
    
    private void CancelTileAnimation(int tileId)
    {
        if (activeTweens.ContainsKey(tileId))
        {
            if (activeTweens[tileId] is Tween tween)
                tween.Stop();
            else if (activeTweens[tileId] is Sequence sequence)
                sequence.Stop();
            activeTweens.Remove(tileId);
        }
    }
    
    public void CancelAllAnimations()
    {
        foreach (var animationObject in activeTweens.Values)
        {
            if (animationObject is Tween tween)
                tween.Stop();
            else if (animationObject is Sequence sequence)
                sequence.Stop();
        }
        activeTweens.Clear();
    }
    
    public void ClearStoredPositions()
    {
        originalPositions.Clear();
    }
    
    public Vector3 GetOriginalPosition(int tileId)
    {
        return originalPositions.ContainsKey(tileId) ? originalPositions[tileId] : Vector3.zero;
    }
    
    public bool IsTileAnimating(int tileId)
    {
        return activeTweens.ContainsKey(tileId);
    }
    
    void OnDestroy()
    {
        CancelAllAnimations();
        ClearStoredPositions();
    }
}