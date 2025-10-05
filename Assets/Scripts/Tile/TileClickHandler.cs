using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    [Header("Click Settings")]
    [SerializeField] private float clickCooldown = 0.1f;
    [SerializeField] private bool enableHapticFeedback = true;
    
    
    private float lastClickTime;
    private bool isPressed = false;
    
    private Camera mainCamera;
    
    public static System.Action<Tile> OnTileClicked;
    
    private void Awake()
    {
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        HandleTouchInput();
    }
    
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isPressed = true;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isPressed)
                    {
                        PerformRaycast(touch.position);
                    }
                    isPressed = false;
                    break;
            }
        }
#if UNITY_EDITOR
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPressed = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (isPressed)
                {
                    PerformRaycast(Input.mousePosition);
                }
                isPressed = false;
            }
        }
#endif
    }
    
    private void PerformRaycast(Vector2 screenPosition)
    {
        if (mainCamera == null) 
        {
            return;
        }
        
    
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        
        RaycastHit2D hit2D = Physics2D.Raycast(worldPosition, Vector3.forward, 200f);
        
        if (hit2D.collider != null)
        {
            IClickable clickable = hit2D.collider.GetComponent<IClickable>();
            if (clickable != null)
            {
                HandleClick(clickable);
            }
        }
    }
    
    private void HandleClick(IClickable clickable)
    {
        if (Time.time - lastClickTime < clickCooldown)
            return;
            
        lastClickTime = Time.time;
        
        if (enableHapticFeedback)
        {
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }
        
        clickable.OnClick();
    }
    
    public void SetClickable(bool clickable)
    {
        enabled = clickable;
    }
    
    public void ResetState()
    {
        isPressed = false;
        lastClickTime = 0f;
    }
} 