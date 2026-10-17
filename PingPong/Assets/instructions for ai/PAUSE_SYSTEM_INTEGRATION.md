# Pause menu UI integration

This branch owns the pause overlay only. The main menu is a separate feature and is deliberately not created or edited here. The Settings button is present as a placeholder; no settings screen or transition is implemented.

## Install or add the missing UI

1. Pull/checkout the latest `feature/pause-menu` branch and let Unity finish recompiling scripts.
2. Open the gameplay scene (`SampleScene` in the current project).
3. Run **Tools > PingPong > Install Pause System In Open Scene**.
4. The installer reuses an existing `PauseBootstrap` and creates `PauseMenuCanvas` only if it does not exist.
5. Save the scene. If asked, save changes to `SampleScene`.

The command is safe to rerun: it will not add a second pause canvas. It only creates pause-related UI in the gameplay scene and does not alter the existing HUD, gameplay objects, main menu, or settings scene.

## Result

- Press `Escape` in Play Mode to open or close the overlay.
- **Продолжить** resumes the game.
- **Настройки** calls `OpenSettings()`, which currently logs a warning and does not open a screen.
- **В меню** loads the scene name configured in `PauseBootstrap > Main Menu Scene Name`. Set this field to the exact scene name provided by the separate main-menu branch, and ensure that scene is included in Build Profiles / Build Settings.

The overlay is hidden when the scene starts. Its visibility is bound to the `PauseStateChanged` event. It uses a Screen Space Overlay canvas, a dimmed backdrop, a centered pause card, and three buttons.

## If the screen still does not appear

- Confirm that Console shows no compile errors after switching branches.
- Confirm `PauseMenuCanvas` exists in the Hierarchy and the scene is saved.
- Enter Play Mode and press `Escape`.
- Confirm the scene contains an EventSystem with an Input System UI Input Module (the installer creates one only if there is no EventSystem).

The repository change adds the editor installer code, but it cannot execute Unity Editor on GitHub. Run the installer locally to create the scene objects.
