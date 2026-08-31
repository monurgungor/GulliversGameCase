using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tap to take back the last letter, hold to take back the whole word.
/// </summary>
[RequireComponent(typeof(Button))]
public class UndoButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Holding at least this long returns every letter instead of one.")]
    [SerializeField] private float holdThreshold = 0.5f;

    private Button button;
    private float pressedAt;
    private bool isPressed;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        WordActions.UndoAvailabilityChanged += SetAvailable;
        SetAvailable(false);
    }

    private void OnDisable()
    {
        WordActions.UndoAvailabilityChanged -= SetAvailable;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        pressedAt = Time.unscaledTime;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed)
        {
            return;
        }

        isPressed = false;
        WordActions.RaiseUndoRequested(Time.unscaledTime - pressedAt >= holdThreshold);
    }

    private void SetAvailable(bool canUndo)
    {
        button.interactable = canUndo;
    }
}
