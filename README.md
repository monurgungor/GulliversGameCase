# Word Tiles Go

A mobile word puzzle game built in Unity. Tiles are stacked on the board. Tap the
free ones to spell a word, then submit it to clear them and score points. The
level ends when the board is empty or when no word is left to play.

![Gameplay](docs/gameplay.gif)

## Requirements

- Unity **6000.5.5f1**
- Universal Render Pipeline (2D Renderer)

## Run it

1. Open the project in Unity.
2. Open `Assets/Game/Scenes/Bootstrap.unity`.
3. Press Play.

You can also press Play on `MainMenu.unity` or `Gameplay.unity` directly.

## Tests

Open **Window > General > Test Runner**, select **EditMode**, and press
**Run All**. 19 tests cover the dictionary, the deadlock search, and the shipped
level and word data.

## Project layout

| Path | What is in it |
| --- | --- |
| `Assets/Game/Scenes` | Bootstrap, MainMenu and Gameplay scenes |
| `Assets/Game/Scripts` | Runtime code, split by area (Core, Data, Gameplay, Levels, Tile, UI) |
| `Assets/Game/Scripts/Editor` | Editor tools: word list baker, art import rules |
| `Assets/Game/Tests/EditMode` | Unit tests |
| `Assets/Game/Art` | Sprites, grouped by screen |
| `Assets/Game/Data` | Word list |
| `Assets/Resources/Levels` | 20 level files as JSON |

## Built with

- **Zenject** for dependency injection
- **PrimeTween** for animation
- **TextMeshPro** for text
- **Newtonsoft.Json** for save data and level files

## Notes

- Every level is unlocked so the whole game can be reviewed. The progression code
  is still in place: restore the check in `LevelController.BuildLevelList`.
- The word list is baked by **Tools > Word Game > Rebuild Word List**. It holds
  75,329 words and is searched with a binary search over a sorted array.
- Art import settings are applied by **Tools > Word Game > Apply Mobile Art
  Settings**, which sets ASTC compression and sprite atlas rules.
- The game ships without audio.
