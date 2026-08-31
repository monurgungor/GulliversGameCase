using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform logo;
    [SerializeField] private RectTransform playButton;
    [SerializeField] private RectTransform levelPanel;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.2f;
    [SerializeField] private float delayBetweenAnimations = 0.3f;
    [SerializeField] private Ease enterEase = Ease.OutBack;
    [SerializeField] private Ease exitEase = Ease.InBack;
    
    private bool isMainMenuActive = true;

    private void Start()
    {
 
        SetupAnimations();
    }
    
    
    private void SetupAnimations()
    {

        
        SetInitialPositions();
        AnimateMainMenuIn();
    }
    
    private void SetInitialPositions()
    {
        
        
        Vector2 logoStartPos = new Vector2(0, 0+ Screen.height * 1.5f);
        Vector2 buttonStartPos = new Vector2(0, 0 - Screen.height * 1.5f);
        Vector2 panelStartPos = new Vector2(0 + Screen.width * 1.5f, 0);
        
        logo.anchoredPosition = logoStartPos;
        playButton.anchoredPosition = buttonStartPos;
        levelPanel.anchoredPosition = panelStartPos;
        

    }
    
    private void AnimateMainMenuIn()
    {
        isMainMenuActive = true;
        

        Tween.UIAnchoredPosition(logo, Vector2.zero, animationDuration, enterEase);
        
        Tween.Delay(delayBetweenAnimations).OnComplete(() =>
        {

            Tween.UIAnchoredPosition(playButton, Vector2.zero, animationDuration, enterEase);
        });
    }
    
    private void AnimateMainMenuOut()
    {
        isMainMenuActive = false;
        
        Vector2 logoExitPos = new Vector2(0, 0 + Screen.height * 1.5f);
        Tween.UIAnchoredPosition(logo, logoExitPos, animationDuration, exitEase);
        
        Tween.Delay(delayBetweenAnimations).OnComplete(() =>
        {
            Vector2 buttonExitPos = new Vector2(0, 0 - Screen.height * 1.5f);
            Tween.UIAnchoredPosition(playButton, buttonExitPos, animationDuration, exitEase);
        });
    }
    
    private void AnimateLevelPanelIn()
    {
        Tween.UIAnchoredPosition(levelPanel, Vector2.zero, animationDuration, enterEase).OnComplete(() =>
        {
            LevelPageUI levelPageUI = FindAnyObjectByType<LevelPageUI>();
            levelPageUI.CheckForCompletedLevels();
        });
    }
    
    private void AnimateLevelPanelOut()
    {
        Vector2 panelExitPos = new Vector2(0 + Screen.width * 1.5f, 0);
        Tween.UIAnchoredPosition(levelPanel, panelExitPos, animationDuration, exitEase);
    }
    
    public void ShowLevelPanel()
    {
        if (!isMainMenuActive) return;
        
        AnimateMainMenuOut();
        
        Tween.Delay(delayBetweenAnimations * 0.5f).OnComplete(() =>
        {
            AnimateLevelPanelIn();
        });
    }
    
    public void ShowMainMenu()
    {
        if (isMainMenuActive) return;
        
        AnimateLevelPanelOut();
        
        Tween.Delay(delayBetweenAnimations * 0.5f).OnComplete(() =>
        {
            AnimateMainMenuIn();
        });
    }
}