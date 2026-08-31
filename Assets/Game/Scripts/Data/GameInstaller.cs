using UnityEngine;
using Zenject;

/// <summary>
/// Project scope bindings: everything that outlives a single level. The word
/// list is parsed once here instead of once per level load.
/// </summary>
public class GameInstaller : MonoInstaller
{
    [SerializeField] private LetterSettings letterSettings;
    [SerializeField] private VisualSettings visualSettings;

    [Tooltip("Baked word list from Tools > Word Game > Rebuild Word List.")]
    [SerializeField] private TextAsset wordList;

    public override void InstallBindings()
    {
        Container.Bind<SaveManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SceneController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<LevelController>().FromComponentInHierarchy().AsSingle();

        Container.Bind<LetterSettings>().FromInstance(letterSettings).AsSingle();
        Container.Bind<VisualSettings>().FromInstance(visualSettings).AsSingle();

        Container.Bind<WordDictionary>().FromInstance(WordDictionary.Parse(wordList.text)).AsSingle();
        Container.Bind<DeadlockSolver>().AsSingle();
    }
}
