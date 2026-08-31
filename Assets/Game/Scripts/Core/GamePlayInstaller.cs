using Zenject;

/// <summary>Scene scope bindings: the systems that live for one level.</summary>
public class GamePlayInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<TileManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TilePlacer>().FromComponentInHierarchy().AsSingle();
        Container.Bind<WordChecker>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ScoreManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TileAnimationManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameStateManager>().FromComponentInHierarchy().AsSingle();
    }
}
