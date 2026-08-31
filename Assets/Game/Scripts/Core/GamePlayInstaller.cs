using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GamePlayInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<TileManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<WordChecker>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ScoreManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TileAnimationManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameStateManager>().FromComponentInHierarchy().AsSingle();
    }
}
