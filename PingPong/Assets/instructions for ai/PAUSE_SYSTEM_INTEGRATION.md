# Pause menu UI integration

The pause overlay is installed and updated from the gameplay scene only. The main menu is a separate feature and is not changed by this installer. The Settings button remains a placeholder.

## Install or update the pause UI

1. Open the gameplay scene (SampleScene).
2. Wait for Unity to finish recompiling scripts.
3. Run **Tools > PingPong > Install Pause System In Open Scene**.
4. The installer reuses the existing PauseBootstrap and PauseMenuCanvas. When the canvas already exists, it updates the existing pause panel and adds the Reset Ball button only if it is missing. It does not create a duplicate canvas or replace the existing Continue, Settings, and Return to Menu handlers.
5. The installer assigns the ball's Rigidbody2D to PauseBootstrap > Ball Rigidbody by reading the BallPhysicsView reference in the open scene.
6. The same command removes the obsolete HpText and HealthView scene objects.
7. Save SampleScene.

The installer uses Undo for created and changed scene objects and marks the active scene dirty. Run it again safely if the menu needs to be refreshed.

## Controls

- Press Escape in Play Mode to open or close the pause overlay.
- Продолжить resumes the game.
- Сбросить мяч teleports the ball to (0, 0) while keeping the game paused. It stores the current linear velocity, keeps the ball stationary, and clears angular velocity. After Продолжить (or unpausing with Escape), the ball waits 1 second before continuing with its stored velocity. The delay uses scaled time and pauses if the game is paused again.
- Настройки calls OpenSettings(), which currently logs a warning and does not open a screen.
- В меню loads the scene name configured in PauseBootstrap > Main Menu Scene Name. Ensure that scene is included in Build Profiles / Build Settings.

## If the menu does not appear

- Confirm the Console shows no compile errors.
- Confirm PauseMenuCanvas exists in the Hierarchy and the scene is saved.
- Enter Play Mode and press Escape.
- Confirm the scene contains an EventSystem with an Input System UI Input Module. The installer creates one only when there is no EventSystem.
- Confirm PauseBootstrap > Ball Rigidbody references the ball's Rigidbody2D.

The installer code is committed in the repository, but it must be run in Unity Editor to apply and save scene-object changes. Play Mode validation also requires running the project in Unity locally.
