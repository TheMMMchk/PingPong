using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-time scene setup tool for the main menu. It creates the UI using Unity Editor APIs,
/// wires the serialized references, and adds MainMenu before SampleScene in Build Settings.
/// It never edits or replaces SampleScene.
/// </summary>
public static class MainMenuSceneGenerator
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";

    private static TMP_FontAsset _menuFont;

    [MenuItem("PingPong/Generate Main Menu Scene")]
    public static void GenerateMainMenuScene()
    {
        SceneAsset existingMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
        if (existingMenu != null)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            ConfigureBuildSettings();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Debug.Log("MainMenu already exists. Opened it and refreshed Build Settings; no duplicate scene was created.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath) == null)
        {
            EditorUtility.DisplayDialog(
                "PingPong: cannot create menu",
                "The expected gameplay scene Assets/Scenes/SampleScene.unity was not found. No scene was created.",
                "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        try
        {
            BuildMenuScene();
            EditorSceneManager.MarkSceneDirty(scene);

            if (!EditorSceneManager.SaveScene(scene, MainMenuScenePath))
            {
                EditorUtility.DisplayDialog(
                    "PingPong: scene not saved",
                    "Unity could not save Assets/Scenes/MainMenu.unity. Check the Console for details.",
                    "OK");
                return;
            }

            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "MainMenu created at Assets/Scenes/MainMenu.unity. " +
                "It is now the first scene in Build Settings; SampleScene remains included.");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "PingPong: menu generation failed",
                "The menu scene could not be generated. Check the Unity Console for the exception.",
                "OK");
        }
    }

    private static void BuildMenuScene()
    {
        GameObject systemRoot = CreateObject("MainMenuSystem", null);
        MainMenuBootstrap bootstrap = Undo.AddComponent<MainMenuBootstrap>(systemRoot);

        GameObject eventSystemObject = CreateObject(
            "EventSystem",
            systemRoot.transform,
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));

        InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();

        GameObject canvasObject = CreateObject(
            "MainMenuCanvas",
            null,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Transform canvasTransform = canvasObject.transform;

        // Background and restrained arcade accents.
        CreateStretchImage(canvasTransform, "Background", Hex("#0A1020"));
        CreateImage(canvasTransform, "LeftNeonRail", WithAlpha(Hex("#35E0CF"), 0.35f),
            new Vector2(3f, 360f), new Vector2(-610f, 0f));
        CreateImage(canvasTransform, "RightNeonRail", WithAlpha(Hex("#FF6B91"), 0.28f),
            new Vector2(3f, 250f), new Vector2(610f, 0f));
        CreateImage(canvasTransform, "TitleAccent", Hex("#35E0CF"),
            new Vector2(82f, 4f), new Vector2(0f, 374f));
        CreateImage(canvasTransform, "TitleAccentSecondary", Hex("#FF6B91"),
            new Vector2(24f, 4f), new Vector2(55f, 374f));

        CreateText(canvasTransform, "GameTitle",
            "<color=#F5F7FF>PING</color><color=#35E0CF>PONG</color>",
            Hex("#F5F7FF"), 100f, FontStyles.Bold,
            new Vector2(0f, 267f), new Vector2(1000f, 135f), 2.5f, true);

        CreateText(canvasTransform, "Subtitle", "ARCADE  /  ONE ON ONE",
            Hex("#A7B4C9"), 19f, FontStyles.Bold,
            new Vector2(0f, 184f), new Vector2(650f, 36f), 4.5f, false);

        Button playButton = CreateButton(
            canvasTransform, "PlayButton", "ИГРАТЬ",
            new Vector2(0f, 57f), new Vector2(480f, 78f),
            Hex("#35E0CF"), Hex("#F3FFFD"), Hex("#19C8B8"),
            Hex("#071925"), Hex("#06141F"));

        Button settingsButton = CreateButton(
            canvasTransform, "SettingsButton", "НАСТРОЙКИ",
            new Vector2(0f, -48f), new Vector2(480f, 78f),
            Hex("#111B2D"), Hex("#1A2940"), Hex("#152338"),
            Hex("#F5F7FF"), Hex("#35E0CF"));

        Button exitButton = CreateButton(
            canvasTransform, "ExitButton", "ВЫХОД",
            new Vector2(0f, -153f), new Vector2(480f, 78f),
            Hex("#111B2D"), Hex("#1A2940"), Hex("#152338"),
            Hex("#FF8BA7"), Hex("#FF6B91"));

        CreateText(canvasTransform, "Footer",
            "READY WHEN YOU ARE",
            Hex("#6F7E96"), 15f, FontStyles.Normal,
            new Vector2(0f, -350f), new Vector2(600f, 32f), 3f, false);

        MainMenuView view = Undo.AddComponent<MainMenuView>(canvasObject);
        SerializedObject viewSerialized = new SerializedObject(view);
        viewSerialized.FindProperty("playButton").objectReferenceValue = playButton;
        viewSerialized.FindProperty("settingsButton").objectReferenceValue = settingsButton;
        viewSerialized.FindProperty("exitButton").objectReferenceValue = exitButton;
        viewSerialized.ApplyModifiedProperties();

        SerializedObject bootstrapSerialized = new SerializedObject(bootstrap);
        bootstrapSerialized.FindProperty("view").objectReferenceValue = view;
        bootstrapSerialized.ApplyModifiedProperties();

        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(bootstrap);
    }

    private static Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 position,
        Vector2 size,
        Color normalColor,
        Color highlightedColor,
        Color pressedColor,
        Color textColor,
        Color accentColor)
    {
        GameObject buttonObject = CreateObject(
            objectName, parent, typeof(RectTransform), typeof(Image), typeof(Button));

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, position, size);

        Image image = buttonObject.GetComponent<Image>();
        image.color = normalColor;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.interactable = true;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = highlightedColor;
        colors.disabledColor = WithAlpha(normalColor, 0.4f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

        TextMeshProUGUI labelText = CreateText(
            buttonObject.transform, "Label", label, textColor, 28f, FontStyles.Bold,
            Vector2.zero, new Vector2(size.x - 40f, size.y), 1.5f, false);
        labelText.color = textColor;
        labelText.raycastTarget = false;

        // This slim bar adds an accent without intercepting pointer input.
        CreateImage(buttonObject.transform, "Accent",
            accentColor, new Vector2(4f, size.y * 0.55f),
            new Vector2(-size.x * 0.5f + 15f, 0f));

        // Keep the text color fixed in each button state; color transitions apply to the background.
        button.targetGraphic = image;
        return button;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string value,
        Color color,
        float fontSize,
        FontStyles fontStyle,
        Vector2 position,
        Vector2 size,
        float characterSpacing,
        bool richText)
    {
        GameObject textObject = CreateObject(
            objectName, parent, typeof(RectTransform), typeof(TextMeshProUGUI));

        RectTransform rect = textObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, position, size);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = ResolveFont();
        if (font != null)
            text.font = font;

        text.text = value;
        text.color = color;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAlignmentOptions.Center;
        text.characterSpacing = characterSpacing;
        text.enableWordWrapping = false;
        text.richText = richText;
        text.raycastTarget = false;
        text.margin = Vector4.zero;

        return text;
    }

    private static TMP_FontAsset ResolveFont()
    {
        if (_menuFont != null)
            return _menuFont;

        if (TMP_Settings.defaultFontAsset != null)
        {
            _menuFont = TMP_Settings.defaultFontAsset;
            return _menuFont;
        }

        string[] fontGuids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (fontGuids.Length > 0)
            _menuFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                AssetDatabase.GUIDToAssetPath(fontGuids[0]));

        if (_menuFont == null)
            Debug.LogWarning("TextMeshPro default font was not found. Assign a TMP font to the menu text if it renders without glyphs.");

        return _menuFont;
    }

    private static Image CreateStretchImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = CreateObject(
            objectName, parent, typeof(RectTransform), typeof(Image));

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateImage(
        Transform parent, string objectName, Color color, Vector2 size, Vector2 position)
    {
        GameObject imageObject = CreateObject(
            objectName, parent, typeof(RectTransform), typeof(Image));

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, position, size);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static GameObject CreateObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject instance = components == null || components.Length == 0
            ? new GameObject(name)
            : new GameObject(name, components);

        Undo.RegisterCreatedObjectUndo(instance, $"Create {name}");

        if (parent != null)
            instance.transform.SetParent(parent, false);

        return instance;
    }

    private static void SetCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void ConfigureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        scenes.RemoveAll(scene => scene.path == MainMenuScenePath);

        int gameplayIndex = scenes.FindIndex(scene => scene.path == GameplayScenePath);
        if (gameplayIndex >= 0)
        {
            scenes[gameplayIndex].enabled = true;
        }
        else if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath) != null)
        {
            scenes.Add(new EditorBuildSettingsScene(GameplayScenePath, true));
        }

        scenes.Insert(0, new EditorBuildSettingsScene(MainMenuScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
