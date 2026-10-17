# Main Menu

The main menu uses the same separation-of-responsibility approach as the gameplay feature:

- MainMenuBootstrap creates and connects the runtime logic.
- MainMenuView presents the three buttons and publishes button-click events.
- MainMenuController handles requested actions without inheriting from MonoBehaviour.
- MainMenuNavigationService contains the Unity scene-loading and application-exit operations.

## Generate the scene

1. Open the project in Unity 6000.4.10f1.
2. From the Unity menu bar, choose **PingPong → Generate Main Menu Scene**.
3. The editor tool creates Assets/Scenes/MainMenu.unity through the Unity Editor API, wires the references, and puts MainMenu first in Build Settings while keeping SampleScene enabled.
4. The generated MainMenu scene opens. Press Play to test the menu.

The generator does not overwrite SampleScene. Running it again opens the existing MainMenu instead of creating duplicate objects.

The Settings button is intentionally clickable but does not perform an action yet. Exit uses Application.Quit, which exits a built game and does not close the Unity Editor.
