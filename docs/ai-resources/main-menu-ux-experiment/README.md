# Main Menu UX Experiment — AI resources

This folder contains reference material for a **one-off UI/UX experiment** in the PingPong / RIFT PONG project. It is not a project-wide instruction system and none of these documents should be treated as auto-loaded skills in an ordinary ChatGPT conversation.

## Canonical project rules

The project's existing instruction file remains canonical:

- `PingPong/Assets/instructions for ai/INSTRUCTIONS.md`

Read it first. Do not copy it here, fork it, or replace it with a second general instruction file.

## Read these resources by role

| Order | File | Authority / responsibility |
|---|---|---|
| 1 | Existing project `INSTRUCTIONS.md` | Repository architecture, naming, workflow, safety of changes, Git policy. Highest project-level authority. |
| 2 | `skills/01-game-ui-ux-visual/SKILL.md` | Primary design reference: game-specific visual language, hierarchy, readable copy, interaction states, restraint, review. |
| 3 | `skills/02-game-ui-ux-layout/SKILL.md` | Secondary technical UX reference only: anchors/containers, scaling, safe area, keyboard/gamepad focus and responsive verification. It does **not** choose the visual art direction. |
| 4 | `skills/03-unity-ui-router/SKILL.md` | Framework detection / routing only. This project has Canvas-based runtime UI, so it routes to uGUI. |
| 5 | `skills/04-unity-ugui/SKILL.md` | Concrete Unity uGUI reference: Canvas, RectTransform, Layout Groups, EventSystem, GraphicRaycaster, TMP and Button setup. |

## Conflict-resolution rules

These files come from different authors and are **reference playbooks, not four competing root instruction files**. Apply them with this strict division:

1. The user's current task and explicit scope win over all defaults in the skills.
2. The repository's existing `INSTRUCTIONS.md` controls project architecture and Git workflow.
3. The visual skill controls visual design and player-facing copy.
4. The layout skill controls responsive layout and navigation mechanics, not visual style.
5. The Unity router and uGUI skill control only the engine-specific implementation choices.
6. Where a skill example conflicts with the actual project version or installed packages, inspect the project and verify the applicable API for Unity `6000.4.10f1`; do not blindly copy examples written for another version.
7. Do not invent extra menu sections, subtitles, slogans, settings screens, game modes, or functionality. The task brief is limited to the game title and three requested actions.

In particular, the visual skill's preference against decorative subtitles is compatible with this brief: use one primary game title and the three requested buttons. Do not add decorative taglines or extra headings.

The layout skill recommends anchors and layout containers rather than fixed screen coordinates. Use that as the responsive-layout default for the new experiment scene. Do not modify or "correct" the existing menu merely to make it match this recommendation.

## Isolation boundary

The experiment is deliberately separated from the existing menu:

- Scene: `Assets/Scenes/MainMenuUXExperiment.unity`
- Editor generator, if needed: `Assets/Editor/MainMenuUXExperimentSceneGenerator.cs`
- New code, if truly needed: `Assets/Scripts/Features/MainMenuUXExperiment/`

These are target names for the experiment. Before creating anything, check whether they already exist. Do not overwrite or rename the existing `MainMenu` scene, its generator, its scripts, or its README. Reuse existing runtime components only if they can be used unchanged and do not cause cross-scene side effects.

## Important for ordinary ChatGPT

A normal ChatGPT chat does not automatically activate repository `SKILL.md` files just because they exist in GitHub. In the implementation chat, make the repository available through the connected GitHub source or attach the four skill files (and the canonical project `INSTRUCTIONS.md`). Then paste the task prompt. The prompt must explicitly tell ChatGPT to read and apply the files; do not assume automatic skill loading.

## Source and licensing

See `SOURCES.md` for source repositories, pinned commits, and attribution. The original license texts and notices are retained in `licenses/`. The Unity agent plugin's material is licensed under the Unity Companion License for Unity-dependent projects; it is included only as Unity-specific reference material for this Unity project.
