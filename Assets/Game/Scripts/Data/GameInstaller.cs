using UnityEngine;
using Zenject;

/// <summary>
/// Zenject installer for game services
/// </summary>
public class GameInstaller : MonoInstaller
{
    [SerializeField] private LetterSettings letterSettings;
    [SerializeField] private VisualSettings visualSettings;
    
    public override void InstallBindings()
    {
        Container.Bind<LevelController>()
            .FromComponentInHierarchy()
            .AsSingle();
            

        Container.Bind<SaveManager>()
            .FromComponentInHierarchy()
            .AsSingle();
            
        Container.Bind<SceneController>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<LetterSettings>()
            .FromInstance(letterSettings)
            .AsSingle();

        Container.Bind<VisualSettings>()
            .FromInstance(visualSettings)
            .AsSingle();
            
        Container.Bind<GameStateManager>()
            .FromComponentInHierarchy()
            .AsSingle();
            
    }
} 