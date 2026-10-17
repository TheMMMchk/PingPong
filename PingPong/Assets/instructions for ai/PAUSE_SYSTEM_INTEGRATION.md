# Pause system integration

This branch adds the pause logic and button-call entry points only. It intentionally does not create or edit a pause panel, main menu, or settings screen.

## Install the runtime component

1. Open the gameplay scene that contains GameplayBootstrap (currently SampleScene).
2. In Unity, run Tools > PingPong > Install Pause System In Open Scene.
3. Select PauseSystem and set Main Menu Scene Name to the exact scene name that will be provided by the main-menu branch.
4. Save the scene. The installer is idempotent: running it again will select the existing PauseBootstrap rather than adding a duplicate.

The installer only adds PauseBootstrap. It does not build UI objects or alter existing gameplay objects.

## API for the pause UI

On PauseBootstrap, connect the pause menu's buttons to these public methods:

- Continue: ContinueGame()
- Settings: OpenSettings() — placeholder; it logs that settings are not implemented and opens no screen.
- Return to Menu: ReturnToMenu() — validates the configured scene name and restores time scale before loading it.

Escape toggles pause/resume through the new Input System. While paused, Time.timeScale is zero; resuming restores the time scale that was active before the pause.

## Show/hide the future pause panel

PauseBootstrap exposes the serialized UnityEvent<bool> called Pause State Changed. Connect its dynamic bool callback to the future panel's SetActive(bool) method. The event receives true when paused and false when resumed. Keep the panel inactive by default in the UI branch.

The menu scene must be included in Build Profiles / Build Settings for ReturnToMenu() to load it.
