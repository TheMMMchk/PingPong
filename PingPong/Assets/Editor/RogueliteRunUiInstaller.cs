using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates the run upgrade-selection and result overlays in SampleScene.
/// Existing gameplay, HUD, pause UI and MainMenu are left untouched.
/// </summary>
public static class RogueliteRunUiInstaller
{
    private const string CanvasName = "RogueliteRunCanvas";

    [MenuItem("Tools/PingPong/Install Roguelite Run UI")]
    private static void Install()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || scene.name != "SampleScene")
        {
            EditorUtility.DisplayDialog(
                "Roguelite Run UI",
                "Open PingPong/Assets/Scenes/SampleScene.unity before running this command.",
                "OK");
            return;
        }

        GameplayBootstrap bootstrap = FindGameplayBootstrap(scene);
        if (bootstrap == null)
        {
            EditorUtility.DisplayDialog(
                "Roguelite Run UI",
                "GameplayBootstrap was not found in SampleScene. No changes were made.",
                "OK");
            return;
        }

        EnsureEventSystem(scene);

        GameObject existingCanvas = FindCanvas(scene);
        RunUiView existingView = existingCanvas != null
            ? existingCanvas.GetComponent<RunUiView>()
            : null;

        if (existingView != null)
        {
            AssignViewToBootstrap(bootstrap, existingView);
            EditorUtility.DisplayDialog(
                "Roguelite Run UI",
                "Roguelite Run UI already exists. Its Bootstrap reference has been checked.",
                "OK");
            return;
        }

        if (existingCanvas != null)
            Undo.DestroyObjectImmediate(existingCanvas);

        GameObject canvasObject = CreateCanvas();
        RunUiView view = Undo.AddComponent<RunUiView>(canvasObject);

        GameObject upgradePanel = CreateOverlay(canvasObject.transform, "UpgradeSelectionPanel");
        CreateUpgradePanel(upgradePanel.transform, out Button[] optionButtons,
            out TMP_Text[] optionTitles, out TMP_Text[] optionDescriptions);

        GameObject resultPanel = CreateOverlay(canvasObject.transform, "RunResultPanel");
        CreateResultPanel(resultPanel.transform, out TMP_Text resultTitle,
            out Button restartButton, out Button mainMenuButton);

        upgradePanel.SetActive(false);
        resultPanel.SetActive(false);

        ConfigureView(view, upgradePanel, optionButtons, optionTitles,
            optionDescriptions, resultPanel, resultTitle, restartButton, mainMenuButton);
        AssignViewToBootstrap(bootstrap, view);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.SetDirty(canvasObject);
        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(bootstrap);
        Selection.activeGameObject = canvasObject;

        Debug.Log(
            "Roguelite Run UI created in SampleScene. Save the scene to keep the changes. " +
            "The generated canvas uses sorting order 200 so it appears above the pause overlay.");
    }

    private static GameplayBootstrap FindGameplayBootstrap(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            GameplayBootstrap bootstrap = root.GetComponentInChildren<GameplayBootstrap>(true);
            if (bootstrap != null)
                return bootstrap;
        }

        return null;
    }

    private static GameObject FindCanvas(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in transforms)
            {
                if (item.name == CanvasName && item.GetComponent<Canvas>() != null)
                    return item.gameObject;
            }
        }

        return null;
    }

    private static void EnsureEventSystem(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<EventSystem>(true) != null)
                return;
        }

        GameObject eventSystem = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create Run UI Event System");
        SceneManager.MoveGameObjectToScene(eventSystem, scene);
    }

    private static GameObject CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            CanvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Roguelite Run UI");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvasObject;
    }

    private static GameObject CreateOverlay(Transform parent, string name)
    {
        GameObject overlay = CreateUIObject(name, parent, typeof(Image));
        StretchToParent(overlay.GetComponent<RectTransform>());

        Image backdrop = overlay.GetComponent<Image>();
        ApplyUISprite(backdrop);
        backdrop.color = new Color(0.012f, 0.022f, 0.04f, 0.88f);
        backdrop.raycastTarget = true;

        return overlay;
    }

    private static void CreateUpgradePanel(
        Transform parent,
        out Button[] buttons,
        out TMP_Text[] titles,
        out TMP_Text[] descriptions)
    {
        GameObject panel = CreateUIObject("UpgradePanel", parent, typeof(Image));
        SetCenteredRect(panel.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1120f, 620f));
        Image panelImage = panel.GetComponent<Image>();
        ApplyUISprite(panelImage);
        panelImage.color = new Color(0.055f, 0.075f, 0.11f, 0.99f);

        CreateText(panel.transform, "Title", "ВЫБЕРИ УЛУЧШЕНИЕ",
            42, FontStyles.Bold, Color.white, new Vector2(0f, 245f), new Vector2(960f, 70f));
        CreateText(panel.transform, "Subtitle", "Победа за тобой. Выбери усиление перед следующим матчем.",
            19, FontStyles.Normal, new Color(0.65f, 0.75f, 0.88f, 1f),
            new Vector2(0f, 190f), new Vector2(980f, 42f));

        buttons = new Button[3];
        titles = new TMP_Text[3];
        descriptions = new TMP_Text[3];

        float[] positions = { -350f, 0f, 350f };
        for (int i = 0; i < 3; i++)
        {
            GameObject card = CreateUIObject(
                "UpgradeOption" + (i + 1), panel.transform, typeof(Image), typeof(Button));
            SetCenteredRect(card.GetComponent<RectTransform>(),
                new Vector2(positions[i], -35f), new Vector2(310f, 330f));

            Image image = card.GetComponent<Image>();
            ApplyUISprite(image);
            image.color = new Color(0.095f, 0.14f, 0.2f, 1f);

            Button button = card.GetComponent<Button>();
            button.targetGraphic = image;
            SetButtonColors(button);
            buttons[i] = button;

            titles[i] = CreateText(card.transform, "UpgradeTitle", "Название",
                26, FontStyles.Bold, Color.white, new Vector2(0f, 88f), new Vector2(270f, 90f));
            descriptions[i] = CreateText(card.transform, "UpgradeDescription", "Описание улучшения",
                18, FontStyles.Normal, new Color(0.72f, 0.8f, 0.9f, 1f),
                new Vector2(0f, -40f), new Vector2(260f, 150f));
        }
    }

    private static void CreateResultPanel(
        Transform parent,
        out TMP_Text title,
        out Button restartButton,
        out Button mainMenuButton)
    {
        GameObject panel = CreateUIObject("ResultPanel", parent, typeof(Image));
        SetCenteredRect(panel.GetComponent<RectTransform>(), Vector2.zero, new Vector2(680f, 460f));
        Image panelImage = panel.GetComponent<Image>();
        ApplyUISprite(panelImage);
        panelImage.color = new Color(0.055f, 0.075f, 0.11f, 0.99f);

        title = CreateText(panel.transform, "ResultTitle", "ПОБЕДА!",
            52, FontStyles.Bold, Color.white, new Vector2(0f, 115f), new Vector2(580f, 100f));

        restartButton = CreateActionButton(panel.transform, "RestartButton", "Ещё раз",
            new Vector2(0f, -35f), new Color(0.12f, 0.32f, 0.48f, 1f));
        mainMenuButton = CreateActionButton(panel.transform, "MainMenuButton", "В главное меню",
            new Vector2(0f, -145f), new Color(0.15f, 0.18f, 0.24f, 1f));
    }

    private static Button CreateActionButton(
        Transform parent, string name, string label, Vector2 position, Color color)
    {
        GameObject buttonObject = CreateUIObject(name, parent, typeof(Image), typeof(Button));
        SetCenteredRect(buttonObject.GetComponent<RectTransform>(), position, new Vector2(420f, 78f));

        Image image = buttonObject.GetComponent<Image>();
        ApplyUISprite(image);
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        SetButtonColors(button);

        CreateText(buttonObject.transform, "Label", label,
            24, FontStyles.Bold, Color.white, Vector2.zero, new Vector2(390f, 68f));
        return button;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        float fontSize,
        FontStyles style,
        Color color,
        Vector2 position,
        Vector2 size)
    {
        GameObject textObject = CreateUIObject(name, parent);
        SetCenteredRect(textObject.GetComponent<RectTransform>(), position, size);

        TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(textObject);
        text.text = value;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateUIObject(
        string name, Transform parent, params System.Type[] additionalComponents)
    {
        System.Type[] componentTypes = new System.Type[additionalComponents.Length + 2];
        componentTypes[0] = typeof(RectTransform);
        componentTypes[1] = typeof(CanvasRenderer);
        for (int i = 0; i < additionalComponents.Length; i++)
            componentTypes[i + 2] = additionalComponents[i];

        GameObject gameObject = new GameObject(name, componentTypes);
        Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void ApplyUISprite(Image image)
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null)
            return;

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
    }

    private static void SetButtonColors(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.88f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.72f, 0.84f, 0.96f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
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

    private static void ConfigureView(
        RunUiView view,
        GameObject upgradePanel,
        Button[] buttons,
        TMP_Text[] titles,
        TMP_Text[] descriptions,
        GameObject resultPanel,
        TMP_Text resultTitle,
        Button restartButton,
        Button mainMenuButton)
    {
        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("upgradeSelectionPanel").objectReferenceValue = upgradePanel;

        SetObjectArray(serializedView.FindProperty("upgradeButtons"), buttons);
        SetObjectArray(serializedView.FindProperty("upgradeTitles"), titles);
        SetObjectArray(serializedView.FindProperty("upgradeDescriptions"), descriptions);

        serializedView.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        serializedView.FindProperty("resultTitle").objectReferenceValue = resultTitle;
        serializedView.FindProperty("restartButton").objectReferenceValue = restartButton;
        serializedView.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
        serializedView.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray(SerializedProperty property, Object[] values)
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void AssignViewToBootstrap(GameplayBootstrap bootstrap, RunUiView view)
    {
        Undo.RecordObject(bootstrap, "Assign Run UI View");
        SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
        SerializedProperty property = serializedBootstrap.FindProperty("runUiView");
        if (property == null)
        {
            Debug.LogError("GameplayBootstrap does not expose the serialized field 'runUiView'.", bootstrap);
            return;
        }

        property.objectReferenceValue = view;
        serializedBootstrap.ApplyModifiedProperties();
        EditorUtility.SetDirty(bootstrap);
    }
}
