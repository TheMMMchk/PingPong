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
/// Installs or updates the pause overlay in the open gameplay scene.
/// The same command also removes the obsolete Health HUD objects.
/// It never creates or edits the main menu or a settings screen.
/// </summary>
public static class PauseSystemInstaller
{
    private const string PauseCanvasName = "PauseMenuCanvas";
    private const float PanelWidth = 560f;
    private const float PanelHeight = 700f;

    [MenuItem("Tools/PingPong/Install Pause System In Open Scene")]
    private static void InstallPauseSystem()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || scene.name != "SampleScene")
        {
            EditorUtility.DisplayDialog(
                "Pause System",
                "Open SampleScene first, then run this command again. The installer only modifies the gameplay scene.",
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

        EnsureEventSystem(scene);
        AssignBallRigidbody(bootstrap, scene);

        GameObject existingCanvas = FindPauseCanvas(scene);
        GameObject pauseCanvas;
        if (existingCanvas == null)
            pauseCanvas = CreatePauseMenu(bootstrap);
        else
        {
            pauseCanvas = existingCanvas;
            UpdateExistingPauseMenu(existingCanvas, bootstrap);
        }

        int removedHealthObjects = RemoveHealthHud(scene);
        removedHealthObjects += RemoveHealthHudObjectsByComponentName(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.SetDirty(bootstrap);
        Selection.activeGameObject = pauseCanvas;

        Debug.Log(
            "Pause menu installed/updated in scene '" + scene.name +
            "'. Reset Ball is wired, and " + removedHealthObjects +
            " obsolete Health HUD object(s) were removed. Save SampleScene.");
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
                if (child != null && child.name == PauseCanvasName
                    && child.GetComponent<Canvas>() != null)
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

    private static void AssignBallRigidbody(PauseBootstrap bootstrap, Scene scene)
    {
        Rigidbody2D ballRigidbody = FindBallRigidbody(scene);
        if (ballRigidbody == null)
        {
            Debug.LogError(
                "Pause System Installer could not find the Rigidbody2D assigned to a BallPhysicsView. " +
                "Assign Ball Rigidbody manually in the PauseBootstrap Inspector.",
                bootstrap);
            return;
        }

        SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
        SerializedProperty ballProperty = serializedBootstrap.FindProperty("ballRigidbody");
        if (ballProperty == null)
        {
            Debug.LogError("PauseBootstrap.ballRigidbody was not found. Recompile the project and rerun the installer.", bootstrap);
            return;
        }

        if (ballProperty.objectReferenceValue == ballRigidbody)
            return;

        Undo.RecordObject(bootstrap, "Assign Pause Ball Rigidbody");
        ballProperty.objectReferenceValue = ballRigidbody;
        serializedBootstrap.ApplyModifiedProperties();
        EditorUtility.SetDirty(bootstrap);
    }

    private static Rigidbody2D FindBallRigidbody(Scene scene)
    {
        Rigidbody2D foundRigidbody = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            BallPhysicsView[] ballViews = root.GetComponentsInChildren<BallPhysicsView>(true);
            foreach (BallPhysicsView ballView in ballViews)
            {
                if (ballView == null)
                    continue;

                SerializedObject serializedBall = new SerializedObject(ballView);
                SerializedProperty rigidbodyProperty = serializedBall.FindProperty("_rigidbody");
                Rigidbody2D candidate = rigidbodyProperty != null
                    ? rigidbodyProperty.objectReferenceValue as Rigidbody2D
                    : null;

                if (candidate == null)
                    candidate = ballView.GetComponent<Rigidbody2D>();

                if (candidate == null)
                    continue;

                if (foundRigidbody != null && foundRigidbody != candidate)
                {
                    Debug.LogError(
                        "More than one BallPhysicsView with a Rigidbody2D was found. " +
                        "Assign the intended ball Rigidbody manually in PauseBootstrap.",
                        ballView);
                    return null;
                }

                foundRigidbody = candidate;
            }
        }

        return foundRigidbody;
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
        SetCenteredRect(panelRect, Vector2.zero, new Vector2(PanelWidth, PanelHeight));

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
            Color.white, new Vector2(0f, 250f), new Vector2(470f, 74f));

        CreateText(panel.transform, "Subtitle", "ИГРА ПРИОСТАНОВЛЕНА", font, 16, FontStyle.Normal,
            new Color(0.62f, 0.72f, 0.84f, 1f), new Vector2(0f, 196f), new Vector2(440f, 32f));

        CreateButton(
            panel.transform, "ContinueButton", "Продолжить", font, uiSprite,
            new Vector2(0f, 105f), new Color(0.12f, 0.31f, 0.48f, 1f),
            new Color(0.36f, 0.83f, 1f, 1f), bootstrap.ContinueGame);

        CreateButton(
            panel.transform, "SettingsButton", "Настройки", font, uiSprite,
            new Vector2(0f, 9f), new Color(0.13f, 0.16f, 0.22f, 1f),
            new Color(0.43f, 0.53f, 0.66f, 1f), bootstrap.OpenSettings);

        CreateButton(
            panel.transform, "ResetBallButton", "Сбросить мяч", font, uiSprite,
            new Vector2(0f, -87f), new Color(0.16f, 0.25f, 0.32f, 1f),
            new Color(0.35f, 0.83f, 1f, 1f), bootstrap.ResetBallToCenter);

        CreateButton(
            panel.transform, "ReturnToMenuButton", "Выход в меню", font, uiSprite,
            new Vector2(0f, -183f), new Color(0.23f, 0.14f, 0.18f, 1f),
            new Color(1f, 0.43f, 0.43f, 1f), bootstrap.ReturnToMenu);

        CreateText(panel.transform, "EscapeHint", "ESC  —  продолжить игру", font, 14, FontStyle.Normal,
            new Color(0.55f, 0.63f, 0.74f, 1f), new Vector2(0f, -282f), new Vector2(440f, 26f));

        // The entire dimmed overlay starts hidden and follows the pause state.
        overlay.SetActive(false);
        UnityEventTools.AddPersistentListener<bool>(bootstrap.PauseStateChanged, overlay.SetActive);

        EditorUtility.SetDirty(bootstrap);
        return canvasObject;
    }

    private static void UpdateExistingPauseMenu(GameObject canvasObject, PauseBootstrap bootstrap)
    {
        Transform panel = FindDescendant(canvasObject.transform, "PausePanel");
        if (panel == null)
        {
            Debug.LogError(
                "PauseMenuCanvas exists, but its PausePanel was not found. " +
                "The existing menu was left untouched to avoid creating a duplicate.",
                canvasObject);
            return;
        }

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            Undo.RecordObject(panelRect, "Resize Pause Menu Panel");
            SetCenteredRect(panelRect, panelRect.anchoredPosition, new Vector2(PanelWidth, PanelHeight));
            EditorUtility.SetDirty(panelRect);
        }

        SetButtonPosition(panel, "ContinueButton", new Vector2(0f, 105f));
        SetButtonPosition(panel, "SettingsButton", new Vector2(0f, 9f));
        SetButtonPosition(panel, "ReturnToMenuButton", new Vector2(0f, -183f));

        Transform resetTransform = panel.Find("ResetBallButton");
        if (resetTransform == null)
        {
            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Button resetButton = CreateButton(
                panel, "ResetBallButton", "Сбросить мяч", font, uiSprite,
                new Vector2(0f, -87f), new Color(0.16f, 0.25f, 0.32f, 1f),
                new Color(0.35f, 0.83f, 1f, 1f), bootstrap.ResetBallToCenter, true);
            EnsureResetButtonListener(resetButton, bootstrap);
        }
        else
        {
            SetButtonPosition(panel, "ResetBallButton", new Vector2(0f, -87f));
            Button resetButton = resetTransform.GetComponent<Button>();
            if (resetButton == null)
                Debug.LogError("ResetBallButton exists but has no Button component.", resetTransform);
            else
                EnsureResetButtonListener(resetButton, bootstrap);
        }

        Transform hint = panel.Find("EscapeHint");
        if (hint != null)
        {
            RectTransform hintRect = hint.GetComponent<RectTransform>();
            if (hintRect != null)
            {
                Undo.RecordObject(hintRect, "Move Pause Menu Hint");
                SetCenteredRect(hintRect, new Vector2(0f, -282f), hintRect.sizeDelta);
                EditorUtility.SetDirty(hintRect);
            }
        }

        EditorUtility.SetDirty(bootstrap);
    }

    private static void EnsureResetButtonListener(Button button, PauseBootstrap bootstrap)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == bootstrap
                && button.onClick.GetPersistentMethodName(i) == nameof(PauseBootstrap.ResetBallToCenter))
                return;
        }

        Undo.RecordObject(button, "Connect Reset Ball Button");
        UnityEventTools.AddPersistentListener(button.onClick, bootstrap.ResetBallToCenter);
        EditorUtility.SetDirty(button);
    }

    private static void SetButtonPosition(Transform panel, string buttonName, Vector2 position)
    {
        Transform buttonTransform = panel.Find(buttonName);
        if (buttonTransform == null)
            return;

        RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
        if (buttonRect == null)
            return;

        Undo.RecordObject(buttonRect, "Arrange Pause Menu Buttons");
        SetCenteredRect(buttonRect, position, buttonRect.sizeDelta);
        EditorUtility.SetDirty(buttonRect);
    }

    private static Transform FindDescendant(Transform parent, string objectName)
    {
        if (parent == null)
            return null;

        foreach (Transform child in parent)
        {
            if (child.name == objectName)
                return child;

            Transform nested = FindDescendant(child, objectName);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private static int RemoveHealthHud(Scene scene)
    {
        int removedObjects = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = transforms.Length - 1; i >= 0; i--)
            {
                Transform current = transforms[i];
                if (current == null)
                    continue;

                if (current.name == "HpText" || current.name == "HealthView")
                {
                    Undo.DestroyObjectImmediate(current.gameObject);
                    removedObjects++;
                }
            }
        }

        return removedObjects;
    }

    private static int RemoveHealthHudObjectsByComponentName(Scene scene)
    {
        int removedObjects = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour != null && behaviour.GetType().Name == "HealthView")
                {
                    Undo.DestroyObjectImmediate(behaviour.gameObject);
                    removedObjects++;
                }
            }
        }

        return removedObjects;
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
        UnityAction onClick,
        bool registerUndo = false)
    {
        GameObject buttonObject = CreateUIObject(
            objectName, parent, typeof(Image), typeof(Button));

        if (registerUndo)
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create Reset Ball Button");

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
