using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UndoButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Undo Settings")]
    [SerializeField] private float holdThreshold = 0.5f;
    [SerializeField] private Button undoButton;

    private float pointerDownTime;
    private bool isPointerDown;
    
    
    private void Awake()
    {
        if (undoButton == null)
            undoButton = GetComponent<Button>();
            
    }
    
    private void OnEnable()
    {
        WordActions.OnUndoAvailabilityChanged += OnUndoAvailabilityChanged;
        
        UpdateButtonState(false);
    }
    
    private void OnDisable()
    {
        WordActions.OnUndoAvailabilityChanged -= OnUndoAvailabilityChanged;
        
    }
    
    
    public void OnPointerDown(PointerEventData eventData)
    {
         isPointerDown = true;
        pointerDownTime = Time.time;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;
        float heldTime = Time.time - pointerDownTime;
        isPointerDown = false;

        if (heldTime >= holdThreshold)
        {
            WordActions.OnWordUndoWithType?.Invoke(true);
        }
        else
        {
            WordActions.OnWordUndoWithType?.Invoke(false);
        }
    }
    
    
    /// <summary>
    /// Handle undo availability changes
    /// </summary>
    /// <param name="canUndo">Whether undo is available</param>
    private void OnUndoAvailabilityChanged(bool canUndo)
    {
        UpdateButtonState(canUndo);
    }
    
    /// <summary>
    /// Update button interactable state and visual appearance
    /// </summary>
    /// <param name="canUndo">Whether undo is available</param>
    private void UpdateButtonState(bool canUndo)
    {
        if (undoButton != null)
        {
            undoButton.interactable = canUndo;
        }
    }
    
}