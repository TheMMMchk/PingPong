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
/// Creates an isolated RIFT PONG main-menu UX experiment scene.
/// The existing MainMenu runtime components are reused without modification.
/// This generator intentionally does not change Build Settings.
/// </summary>
public static class MainMenuUXExperimentSceneGenerator
{
    private const string ExperimentScenePath = "Assets/Scenes/MainMenuUXExperiment.unity";
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";

    private static TMP_FontAsset _menuFont;

    [MenuItem("PingPong/UX Experiment/Generate Main Menu UX Experiment")]
    public static void GenerateScene()
    {
        SceneAsset existingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ExperimentScenePath);
        if (existingScene != null)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(ExperimentScenePath, OpenSceneMode.Single);
            Debug.Log(
                "MainMenuUXExperiment already exists. It was opened without regenerating or overwriting it.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath) == null)
        {
            EditorUtility.DisplayDialog(
                "RIFT PONG UX Experiment",
                "The gameplay scene Assets/Scenes/SampleScene.unity was not found. No experiment scene was created.",
                "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        try
        {
            BuildScene();
            EditorSceneManager.MarkSceneDirty(scene);

            if (!EditorSceneManager.SaveScene(scene, ExperimentScenePath))
            {
                EditorUtility.DisplayDialog(
                    "RIFT PONG UX Experiment",
                    "Unity could not save MainMenuUXExperiment.unity. Check the Console for details.",
                    "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            bool gameplayEnabled = EditorBuildSettings.scenes.Any(
                buildScene => buildScene.path == GameplayScenePath && buildScene.enabled);

            if (!gameplayEnabled)
            {
                Debug.LogWarning(
                    "SampleScene is not enabled in Build Settings. The experiment generator did not change Build Settings; " +
                    "the existing MainMenuNavigationService requires SampleScene to be included for Play to work in a build.");
            }

            Debug.Log(
                "Created Assets/Scenes/MainMenuUXExperiment.unity. " +
                "Build Settings and all existing scenes were left unchanged.");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "RIFT PONG UX Experiment",
                "The experiment scene could not be generated. Check the Unity Console for details.",
                "OK");
        }
    }

    private static void BuildScene()
    {
        CreateMenuSystem();
        EventSystem eventSystem = CreateEventSystem();

        GameObject canvasObject = CreateObject(
            "MainMenuUXExperimentCanvas",
            null,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        ConfigureCanvas(canvasObject);

        Transform canvasTransform = canvasObject.transform;

        CreateStretchImage(canvasTransform, "Background", Hex("#101A20"));
        CreateCourtDecoration(canvasTransform);

        CreateText(
            canvasTransform,
            "GameTitle",
            "<color=#F2EBDD>RIFT</color> <color=#DCEB7B>PONG</color>",
            Hex("#F2EBDD"),
            96f,
            64f,
            96f,
            FontStyles.Bold,
            new Vector2(0f, 0f),
            new Vector2(0f, -290f),
            new Vector2(0.10f, 1f),
            new Vector2(0.90f, 1f),
            true,
            4.5f);

        GameObject menuColumn = CreateObject(
            "MenuButtons",
            canvasTransform,
            typeof(RectTransform),
            typeof(VerticalLayoutGroup));

        RectTransform menuRect = menuColumn.GetComponent<RectTransform>();
        SetCenteredRect(menuRect, new Vector2(0f, -48f), new Vector2(540f, 270f));

        VerticalLayoutGroup layout = menuColumn.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 14f;
        layout.padding = new RectOffset(0, 0, 4, 4);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Button playButton = CreateButton(
            menuColumn.transform,
            "PlayButton",
            "Play",
            Hex("#DCEB7B"),
            Hex("#F0FAA9"),
            Hex("#B8C65B"),
            Hex("#F4FFB9"),
            Hex("#18252A"),
            Hex("#18252A"));

        Button settingsButton = CreateButton(
            menuColumn.transform,
            "SettingsButton",
            "Settings",
            Hex("#1B2B32"),
            Hex("#2B4148"),
            Hex("#101B21"),
            Hex("#38515A"),
            Hex("#F2EBDD"),
            Hex("#8DDFC8"));

        Button exitButton = CreateButton(
            menuColumn.transform,
            "ExitButton",
            "Exit",
            Hex("#1B2B32"),
            Hex("#2B4148"),
            Hex("#101B21"),
            Hex("#38515A"),
            Hex("#F2EBDD"),
            Hex("#F58D75"));

        ConfigureNavigation(playButton, settingsButton, exitButton);
        eventSystem.firstSelectedGameObject = playButton.gameObject;

        // These existing runtime components already implement the project's menu behavior.
        // Reusing them keeps scene navigation and button semantics consistent with MainMenu.
        MainMenuView view = Undo.AddComponent<MainMenuView>(canvasObject);
        SetObjectReference(view, "playButton", playButton);
        SetObjectReference(view, "settingsButton", settingsButton);
        SetObjectReference(view, "exitButton", exitButton);

        GameObject systemRoot = CreateObject("MainMenuUXExperimentSystem", null);
        MainMenuBootstrap bootstrap = Undo.AddComponent<MainMenuBootstrap>(systemRoot);
        SetObjectReference(bootstrap, "view", view);
    }

    private static EventSystem CreateEventSystem()
    {
        GameObject eventSystemObject = CreateObject(
            "EventSystem",
            null,
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));

        EventSystem eventSystem = eventSystemObject.GetComponent<EventSystem>();
        eventSystem.sendNavigationEvents = true;

        InputSystemUIInputModule inputModule =
            eventSystemObject.GetComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();

        return eventSystem;
    }

    private static void ConfigureCanvas(GameObject canvasObject)
    {
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
    }

    private static void CreateCourtDecoration(Transform parent)
    {
        GameObject court = CreateObject("ArenaCourt", parent, typeof(RectTransform));
        RectTransform courtRect = court.GetComponent<RectTransform>();
        courtRect.anchorMin = Vector2.zero;
        courtRect.anchorMax = Vector2.one;
        courtRect.offsetMin = new Vector2(84f, 76f);
        courtRect.offsetMax = new Vector2(-84f, -76f);

        Color boundaryColor = WithAlpha(Hex("#58727A"), 0.48f);
        Color centerLineColor = WithAlpha(Hex("#58727A"), 0.38f);

        CreateAnchoredImage(
            court.transform, "TopBoundary", boundaryColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, 2f), new Vector2(0f, -1f));

        CreateAnchoredImage(
            court.transform, "BottomBoundary", boundaryColor,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 2f), new Vector2(0f, 1f));

        CreateAnchoredImage(
            court.transform, "LeftBoundary", boundaryColor,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(1f, 0f));

        CreateAnchoredImage(
            court.transform, "RightBoundary", boundaryColor,
            new Vector2(1f, 0f), new Vector2(1f, 1f),
            new Vector2(1f, 0.5f), new Vector2(2f, 0f), new Vector2(-1f, 0f));

        for (int dash = -4; dash <= 4; dash++)
        {
            CreateCenteredImage(
                court.transform,
                "CenterLineDash",
                centerLineColor,
                new Vector2(2f, 20f),
                new Vector2(0f, dash * 48f),
                0f);
        }

        CreateAnchoredImage(
            court.transform, "LeftPaddle", Hex("#8DDFC8"),
            Vector2.zero, Vector2.zero,
            new Vector2(0f, 0.5f), new Vector2(12f, 116f), new Vector2(24f, -18f),
            new Vector2(0f, 0.5f));

        CreateAnchoredImage(
            court.transform, "RightPaddle", Hex("#F58D75"),
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f), new Vector2(12f, 116f), new Vector2(-24f, 18f),
            new Vector2(1f, 0.5f));

        Image ball = CreateCenteredImage(
            court.transform,
            "ArenaBall",
            Hex("#E8EF78"),
            new Vector2(18f, 18f),
            new Vector2(290f, 55f),
            45f);
        ball.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
    }

    private static Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Color normalColor,
        Color highlightedColor,
        Color pressedColor,
        Color selectedColor,
        Color textColor,
        Color accentColor)
    {
        GameObject buttonObject = CreateObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement));

        Image background = buttonObject.GetComponent<Image>();
        background.color = normalColor;
        background.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.interactable = true;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = selectedColor;
        colors.disabledColor = WithAlpha(normalColor, 0.42f);
        colors.fadeDuration = 0.10f;
        button.colors = colors;

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.minWidth = 380f;
        layoutElement.preferredWidth = 460f;
        layoutElement.minHeight = 70f;
        layoutElement.preferredHeight = 74f;

        CreateText(
            buttonObject.transform,
            "Label",
            label,
            textColor,
            28f,
            22f,
            28f,
            FontStyles.Bold,
            new Vector2(0f, 0f),
            Vector2.zero,
            Vector2.zero,
            Vector2.one,
            false,
            2f,
            new Vector2(38f, 0f),
            new Vector2(-28f, 0f));

        CreateAnchoredImage(
            buttonObject.transform,
            "Accent",
            accentColor,
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(4f, 34f),
            new Vector2(18f, 0f));

        return button;
    }

    private static void ConfigureNavigation(Button play, Button settings, Button exit)
    {
        Navigation playNavigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = settings
        };
        play.navigation = playNavigation;

        Navigation settingsNavigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = play,
            selectOnDown = exit
        };
        settings.navigation = settingsNavigation;

        Navigation exitNavigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = settings
        };
        exit.navigation = exitNavigation;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string value,
        Color color,
        float fontSize,
        float minFontSize,
        float maxFontSize,
        FontStyles fontStyle,
        Vector2 position,
        Vector2 size,
        Vector2 anchorMin,
        Vector2 anchorMax,
        bool richText,
        float characterSpacing,
        Vector2? offsetMin = null,
        Vector2? offsetMax = null)
    {
        GameObject textObject = CreateObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        if (anchorMin == Vector2.zero && anchorMax == Vector2.one &&
            (offsetMin.HasValue || offsetMax.HasValue))
        {
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;
        }
        else
        {
            rect.sizeDelta = size;
        }

        rect.localScale = Vector3.one;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = ResolveFont();
        if (font != null)
            text.font = font;

        text.text = value;
        text.color = color;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = minFontSize;
        text.fontSizeMax = maxFontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAlignmentOptions.Center;
        text.characterSpacing = characterSpacing;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
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
        {
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
            _menuFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        }

        if (_menuFont == null)
        {
            Debug.LogWarning(
                "The TMP default font was not found. Assign a TextMeshPro font asset to the generated text objects.");
        }

        return _menuFont;
    }

    private static Image CreateStretchImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = CreateObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(Image));

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

    private static Image CreateCenteredImage(
        Transform parent,
        string objectName,
        Color color,
        Vector2 size,
        Vector2 position,
        float rotation)
    {
        GameObject imageObject = CreateObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(Image));

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, position, size);
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateAnchoredImage(
        Transform parent,
        string objectName,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 position,
        Vector2? explicitPivot = null)
    {
        GameObject imageObject = CreateObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(Image));

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = explicitPivot ?? pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static GameObject CreateObject(
        string objectName,
        Transform parent,
        params System.Type[] components)
    {
        GameObject instance = components == null || components.Length == 0
            ? new GameObject(objectName)
            : new GameObject(objectName, components);

        Undo.RegisterCreatedObjectUndo(instance, "Create " + objectName);

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

    private static void SetObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
            throw new System.InvalidOperationException(
                "Serialized field '" + propertyName + "' was not found on " + target.GetType().Name + ".");

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
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