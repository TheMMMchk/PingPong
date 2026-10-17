using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Installs the pause runtime component and its pause-only overlay into the open gameplay scene.
/// It never creates or edits the main menu or a settings screen.
/// </summary>
public static class PauseSystemInstaller
{
    private const string PauseCanvasName = "PauseMenuCanvas";

    [MenuItem("Tools/PingPong/Install Pause System In Open Scene")]
    private static void InstallPauseSystem()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog(
                "Pause System",
                "Open the gameplay scene first, then run this command again.",
                "OK");
            return;
        }

        PauseBootstrap bootstrap = FindPauseBootstrap(scene);
        if (bootstrap == null)
        {
            GameObject pauseSystem = new GameObject("PauseSystem");
            Undo.RegisterCreatedObjectUndo(pauseSystem, "Install Pause System");
            bootstrap = Undo.AddComponent<PauseBootstrap>(pauseSystem);
        }

        GameObject existingCanvas = FindPauseCanvas(scene);
        if (existingCanvas != null)
        {
            Selection.activeGameObject = existingCanvas;
            EditorUtility.DisplayDialog(
                "Pause System",
                "The pause UI already exists in this scene. Select PauseMenuCanvas to inspect it.",
                "OK");
            return;
        }

        EnsureEventSystem(scene);
        GameObject pauseCanvas = CreatePauseMenu(bootstrap);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.SetDirty(bootstrap);
        Selection.activeGameObject = pauseCanvas;

        Debug.Log(
            "Pause menu UI created in scene '" + scene.name +
            "'. Save the scene. Press Escape in Play Mode to show the pause screen.");
    }

    private static PauseBootstrap FindPauseBootstrap(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            PauseBootstrap bootstrap = root.GetComponentInChildren<PauseBootstrap>(true);
            if (bootstrap != null)
                return bootstrap;
        }

        return null;
    }

    private static GameObject FindPauseCanvas(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == PauseCanvasName && root.GetComponentInChildren<Canvas>(true) != null)
                return root;

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == PauseCanvasName && child.GetComponent<Canvas>() != null)
                    return child.gameObject;
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
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create UI Event System");
        SceneManager.MoveGameObjectToScene(eventSystem, scene);
    }

    private static GameObject CreatePauseMenu(PauseBootstrap bootstrap)
    {
        GameObject canvasObject = new GameObject(
            PauseCanvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Pause Menu UI");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject overlay = CreateUIObject("PauseOverlay", canvasObject.transform);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        StretchToParent(overlayRect);

        Image backdrop = overlay.AddComponent<Image>();
        backdrop.color = new Color(0.018f, 0.027f, 0.047f, 0.82f);
        backdrop.raycastTarget = true;

        GameObject panel = CreateUIObject("PausePanel", overlay.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        SetCenteredRect(panelRect, Vector2.zero, new Vector2(560f, 620f));

        Image panelImage = panel.AddComponent<Image>();
        ApplyUISprite(panelImage, uiSprite);
        panelImage.color = new Color(0.075f, 0.095f, 0.14f, 0.99f);

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.18f, 0.31f, 0.46f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.48f);
        shadow.effectDistance = new Vector2(0f, -8f);

        GameObject topAccent = CreateUIObject("TopAccent", panel.transform);
        RectTransform accentRect = topAccent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, 6f);
        Image accentImage = topAccent.AddComponent<Image>();
        accentImage.color = new Color(0.24f, 0.72f, 1f, 1f);
        accentImage.raycastTarget = false;

        CreateText(panel.transform, "Title", "ПАУЗА", font, 48, FontStyle.Bold,
            Color.white, new Vector2(0f, 215f), new Vector2(470f, 74f));

        CreateText(panel.transform, "Subtitle", "ИГРА ПРИОСТАНОВЛЕНА", font, 16, FontStyle.Normal,
            new Color(0.62f, 0.72f, 0.84f, 1f), new Vector2(0f, 161f), new Vector2(440f, 32f));

        Button continueButton = CreateButton(
            panel.transform, "ContinueButton", "Продолжить", font, uiSprite,
            new Vector2(0f, 62f), new Color(0.12f, 0.31f, 0.48f, 1f),
            new Color(0.36f, 0.83f, 1f, 1f), bootstrap.ContinueGame);

        Button settingsButton = CreateButton(
            panel.transform, "SettingsButton", "Настройки", font, uiSprite,
            new Vector2(0f, -34f), new Color(0.13f, 0.16f, 0.22f, 1f),
            new Color(0.43f, 0.53f, 0.66f, 1f), bootstrap.OpenSettings);

        Button menuButton = CreateButton(
            panel.transform, "ReturnToMenuButton", "В меню", font, uiSprite,
            new Vector2(0f, -130f), new Color(0.23f, 0.14f, 0.18f, 1f),
            new Color(1f, 0.43f, 0.43f, 1f), bootstrap.ReturnToMenu);

        CreateText(panel.transform, "EscapeHint", "ESC  —  продолжить игру", font, 14, FontStyle.Normal,
            new Color(0.55f, 0.63f, 0.74f, 1f), new Vector2(0f, -251f), new Vector2(440f, 26f));

        // The entire dimmed overlay starts hidden and follows the pause state.
        overlay.SetActive(false);
        UnityEventTools.AddPersistentListener<bool>(bootstrap.PauseStateChanged, overlay.SetActive);

        EditorUtility.SetDirty(continueButton);
        EditorUtility.SetDirty(settingsButton);
        EditorUtility.SetDirty(menuButton);
        EditorUtility.SetDirty(bootstrap);

        return canvasObject;
    }

    private static Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Font font,
        Sprite uiSprite,
        Vector2 position,
        Color backgroundColor,
        Color accentColor,
        UnityAction onClick)
    {
        GameObject buttonObject = CreateUIObject(
            objectName, parent, typeof(Image), typeof(Button));

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetCenteredRect(buttonRect, position, new Vector2(410f, 72f));

        Image image = buttonObject.GetComponent<Image>();
        ApplyUISprite(image, uiSprite);
        image.color = backgroundColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.88f, 0.94f, 1f, 1f);
        colors.pressedColor = new Color(0.72f, 0.82f, 0.92f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.12f;
        button.colors = colors;

        GameObject stripe = CreateUIObject("Accent", buttonObject.transform);
        RectTransform stripeRect = stripe.GetComponent<RectTransform>();
        stripeRect.anchorMin = new Vector2(0f, 0f);
        stripeRect.anchorMax = new Vector2(0f, 1f);
        stripeRect.pivot = new Vector2(0f, 0.5f);
        stripeRect.anchoredPosition = new Vector2(15f, 0f);
        stripeRect.sizeDelta = new Vector2(4f, -20f);
        Image stripeImage = stripe.AddComponent<Image>();
        stripeImage.color = accentColor;
        stripeImage.raycastTarget = false;

        CreateText(buttonObject.transform, "Label", label, font, 24, FontStyle.Bold,
            Color.white, Vector2.zero, new Vector2(360f, 62f));

        UnityEventTools.AddPersistentListener(button.onClick, onClick);
        EditorUtility.SetDirty(button);
        return button;
    }

    private static Text CreateText(
        Transform parent,
        string objectName,
        string value,
        Font font,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        Vector2 position,
        Vector2 size)
    {
        GameObject textObject = CreateUIObject(objectName, parent, typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, position, size);

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateUIObject(
        string name,
        Transform parent,
        params System.Type[] additionalComponents)
    {
        System.Type[] componentTypes = new System.Type[additionalComponents.Length + 2];
        componentTypes[0] = typeof(RectTransform);
        componentTypes[1] = typeof(CanvasRenderer);
        for (int i = 0; i < additionalComponents.Length; i++)
            componentTypes[i + 2] = additionalComponents[i];

        GameObject gameObject = new GameObject(name, componentTypes);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void ApplyUISprite(Image image, Sprite uiSprite)
    {
        if (uiSprite == null)
            return;

        image.sprite = uiSprite;
        image.type = Image.Type.Sliced;
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
}
