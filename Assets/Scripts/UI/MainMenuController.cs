using System.Collections.Generic;
using System.IO;
using System.Linq;
using CubeShift.Audio;
using CubeShift.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CubeShift.UI
{
    /// <summary>Runtime-built modern main menu and level select. Keeps the scene lightweight and easy to regenerate.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private static MainMenuController activeInstance;

        private readonly List<string> levelScenes = new();
        private readonly List<LevelButtonUI> levelButtons = new();
        private GameObject homePanel;
        private GameObject levelPanel;
        private SettingsPanelController settingsPanel;
        private SceneTransitionController transitionController;
        private TMP_Text previewTitleText;
        private TMP_Text previewDetailText;
        private TMP_Text progressText;
        private Button startSelectedButton;
        private int selectedLevel;

        private static readonly Color BackgroundColor = new(0.012f, 0.017f, 0.028f, 1f);
        private static readonly Color PanelColor = new(0.035f, 0.055f, 0.078f, 0.88f);
        private static readonly Color AccentColor = new(0.05f, 0.78f, 0.9f, 1f);
        private static readonly Color AccentSoftColor = new(0.18f, 0.44f, 0.58f, 0.92f);
        private static readonly Color TextMutedColor = new(0.66f, 0.75f, 0.82f, 1f);

        private void Awake()
        {
            if (activeInstance != null && activeInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            activeInstance = this;
            DiscoverLevels();
            EnsureEventSystem();
            BuildMenu();
        }

        private void OnDestroy()
        {
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        private void Start()
        {
            CubeShiftAudio.Instance.PlayMusicState(MusicState.MainMenu, true);
        }

        private void DiscoverLevels()
        {
            levelScenes.Clear();
            for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(index);
                string sceneName = Path.GetFileNameWithoutExtension(path);
                if (sceneName.StartsWith("Level_"))
                {
                    levelScenes.Add(sceneName);
                }
            }

            levelScenes.Sort((left, right) => ExtractNumber(left).CompareTo(ExtractNumber(right)));
        }

        private void BuildMenu()
        {
            CleanupGeneratedMenuCanvases();
            EnsureMenuCamera();
            Canvas canvas = CreateCanvas();
            BuildBackground(canvas.transform);

            homePanel = CreatePanel(canvas.transform, "HomePanel");
            BuildHomePanel(homePanel.transform);

            levelPanel = CreatePanel(canvas.transform, "LevelSelectPanel");
            levelPanel.SetActive(false);
            BuildLevelPanel(levelPanel.transform);

            settingsPanel = BuildSettingsPanel(canvas.transform);
            transitionController = BuildTransition(canvas.transform);
            ApplyMenuMode(false);
        }

        private void BuildBackground(Transform parent)
        {
            Image background = CreateImage(parent, "Background", BackgroundColor);
            Stretch(background.rectTransform);

            Image glowA = CreateImage(parent, "CyanGlow", new Color(0f, 0.78f, 0.92f, 0.18f));
            SetRect(glowA.rectTransform, new Vector2(-540f, 260f), new Vector2(620f, 620f));
            glowA.sprite = CreateSoftCircleSprite();

            Image glowB = CreateImage(parent, "VioletGlow", new Color(0.48f, 0.28f, 0.95f, 0.14f));
            SetRect(glowB.rectTransform, new Vector2(580f, -210f), new Vector2(720f, 720f));
            glowB.sprite = glowA.sprite;

            RectTransform[] shapes = new RectTransform[9];
            Image[] pulses = new Image[2] { glowA, glowB };
            for (int i = 0; i < shapes.Length; i++)
            {
                Image shape = CreateImage(parent, $"IsoTile_{i:00}", i % 3 == 0
                    ? new Color(0.08f, 0.36f, 0.46f, 0.24f)
                    : new Color(0.1f, 0.13f, 0.19f, 0.36f));
                Vector2 position = new(-720f + (i % 5) * 360f, -330f + (i / 5) * 520f);
                SetRect(shape.rectTransform, position, new Vector2(150f, 150f));
                shape.rectTransform.rotation = Quaternion.Euler(0f, 0f, 45f);
                shapes[i] = shape.rectTransform;
            }

            MenuBackgroundAnimator animator = parent.gameObject.AddComponent<MenuBackgroundAnimator>();
            animator.Configure(shapes, pulses);
        }

        private void BuildHomePanel(Transform parent)
        {
            GameObject card = new GameObject("HomeCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(0f, -8f);
            cardRect.sizeDelta = new Vector2(560f, 760f);
            card.GetComponent<Image>().color = new Color(0.018f, 0.028f, 0.045f, 0.78f);

            Image topLine = CreateImage(card.transform, "TopAccent", AccentColor);
            SetRect(topLine.rectTransform, new Vector2(0f, 358f), new Vector2(390f, 4f));

            Image cubeMark = CreateImage(card.transform, "CubeMark", new Color(0.07f, 0.55f, 0.75f, 0.55f));
            SetRect(cubeMark.rectTransform, new Vector2(0f, 245f), new Vector2(96f, 96f));
            cubeMark.rectTransform.rotation = Quaternion.Euler(0f, 0f, 45f);

            TMP_Text title = CreateText(card.transform, "Cube Shift", new Vector2(0f, 145f), new Vector2(520f, 104f), 68f, FontStyles.Bold);
            title.color = Color.white;
            title.enableAutoSizing = true;
            title.fontSizeMin = 44f;
            title.fontSizeMax = 68f;

            TMP_Text subtitle = CreateText(card.transform, "Roll the cube. Read the grid. Solve clean.", new Vector2(0f, 78f), new Vector2(460f, 40f), 21f, FontStyles.Normal);
            subtitle.color = TextMutedColor;

            GameObject buttonStack = CreatePanel(card.transform, "ButtonStack");
            RectTransform stackRect = buttonStack.GetComponent<RectTransform>();
            stackRect.anchorMin = stackRect.anchorMax = new Vector2(0.5f, 0.5f);
            stackRect.anchoredPosition = new Vector2(0f, -145f);
            stackRect.sizeDelta = new Vector2(420f, 390f);
            VerticalLayoutGroup layout = buttonStack.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Button continueButton = CreateButton(buttonStack.transform, "Continue", new Color(0.05f, 0.62f, 0.78f, 0.96f), () => LoadLevel(ProgressManager.LastPlayedLevel), false);
            continueButton.interactable = levelScenes.Count > 0;
            CreateButton(buttonStack.transform, "Play", new Color(0.1f, 0.66f, 0.38f, 0.98f), () => LoadLevel(1), false);
            CreateButton(buttonStack.transform, "Level Select", new Color(0.09f, 0.24f, 0.34f, 0.96f), ShowLevels, false);
            CreateButton(buttonStack.transform, "Settings", new Color(0.075f, 0.105f, 0.15f, 0.96f), () => settingsPanel.Show(), false);
            CreateButton(buttonStack.transform, "Quit", new Color(0.46f, 0.08f, 0.14f, 0.96f), ExitGame, true);

            TMP_Text footer = CreateText(card.transform, $"Progress {ProgressManager.CountCompleted(levelScenes.Count)} / {levelScenes.Count}", new Vector2(0f, -332f), new Vector2(420f, 34f), 18f, FontStyles.Bold);
            footer.color = new Color(0.5f, 0.8f, 0.88f, 0.95f);
        }

        private void BuildLevelPanel(Transform parent)
        {
            CreateText(parent, "Level Select", new Vector2(-500f, 365f), new Vector2(520f, 78f), 48f, FontStyles.Bold).alignment = TextAlignmentOptions.Left;
            progressText = CreateText(parent, string.Empty, new Vector2(-500f, 310f), new Vector2(520f, 40f), 22f, FontStyles.Normal);
            progressText.alignment = TextAlignmentOptions.Left;
            progressText.color = TextMutedColor;

            GameObject scrollObject = new GameObject("LevelScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(parent, false);
            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRectTransform.anchoredPosition = new Vector2(-285f, -35f);
            scrollRectTransform.sizeDelta = new Vector2(820f, 610f);
            scrollObject.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.045f, 0.45f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject gridObject = new GameObject("LevelGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            gridObject.transform.SetParent(viewport.transform, false);
            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0f, 1f);
            gridRect.anchorMax = new Vector2(1f, 1f);
            gridRect.pivot = new Vector2(0.5f, 1f);
            gridRect.anchoredPosition = new Vector2(0f, -24f);
            gridRect.sizeDelta = new Vector2(-48f, 0f);

            GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(136f, 118f);
            grid.spacing = new Vector2(18f, 18f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment = TextAnchor.UpperCenter;
            ContentSizeFitter fitter = gridObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = gridRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            BuildPreview(parent);
            CreateLevelButtons(gridObject.transform);
            RefreshProgressText();
        }

        private void BuildPreview(Transform parent)
        {
            GameObject preview = new GameObject("PreviewPanel", typeof(RectTransform), typeof(Image));
            preview.transform.SetParent(parent, false);
            RectTransform rect = preview.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(520f, -20f);
            rect.sizeDelta = new Vector2(430f, 600f);
            preview.GetComponent<Image>().color = PanelColor;

            previewTitleText = CreateText(preview.transform, "Select a Level", new Vector2(0f, 205f), new Vector2(330f, 70f), 34f, FontStyles.Bold);
            previewDetailText = CreateText(preview.transform, "Pick an unlocked card to inspect and start.", new Vector2(0f, 75f), new Vector2(330f, 190f), 22f, FontStyles.Normal);
            previewDetailText.color = TextMutedColor;

            startSelectedButton = CreateButton(preview.transform, "Start Level", AccentColor, StartSelectedLevel, false);
            SetRect(startSelectedButton.GetComponent<RectTransform>(), new Vector2(0f, -145f), new Vector2(270f, 64f));
            CreateButton(preview.transform, "Settings", new Color(0.12f, 0.16f, 0.22f, 0.95f), () => settingsPanel.Show(), false);
            SetRect(preview.transform.Find("SettingsButton").GetComponent<RectTransform>(), new Vector2(0f, -225f), new Vector2(270f, 58f));
            CreateButton(preview.transform, "Back", new Color(0.18f, 0.2f, 0.26f, 0.95f), ShowHome, true);
            SetRect(preview.transform.Find("BackButton").GetComponent<RectTransform>(), new Vector2(0f, -295f), new Vector2(270f, 58f));
        }

        private void CreateLevelButtons(Transform parent)
        {
            levelButtons.Clear();
            int lastPlayed = ProgressManager.LastPlayedLevel;
            foreach (string sceneName in levelScenes)
            {
                int levelNumber = ExtractNumber(sceneName);
                bool locked = !ProgressManager.IsUnlocked(levelNumber);
                bool completed = ProgressManager.IsCompleted(levelNumber);
                GameObject card = new GameObject($"Level_{levelNumber:00}_Card", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonAnimator), typeof(UIAudioFeedback), typeof(LevelButtonUI));
                card.transform.SetParent(parent, false);
                Image image = card.GetComponent<Image>();
                Button button = card.GetComponent<Button>();
                UIButtonAnimator animator = card.GetComponent<UIButtonAnimator>();
                LevelButtonUI levelButton = card.GetComponent<LevelButtonUI>();

                TMP_Text number = CreateText(card.transform, levelNumber.ToString("00"), new Vector2(0f, 25f), new Vector2(120f, 44f), 32f, FontStyles.Bold);
                TMP_Text status = CreateText(card.transform, "OPEN", new Vector2(0f, -14f), new Vector2(120f, 28f), 14f, FontStyles.Bold);
                status.color = TextMutedColor;
                TMP_Text stars = CreateText(card.transform, "---", new Vector2(0f, -42f), new Vector2(120f, 28f), 16f, FontStyles.Normal);
                stars.color = new Color(1f, 0.82f, 0.22f, 1f);

                levelButton.Bind(button, image, number, status, stars, animator);
                levelButton.Configure(
                    levelNumber,
                    locked,
                    completed,
                    ProgressManager.GetStars(levelNumber),
                    levelNumber == lastPlayed,
                    ProgressManager.GetBestTime(levelNumber),
                    ProgressManager.GetBestMoves(levelNumber));
                int capturedLevel = levelNumber;
                button.onClick.AddListener(() => SelectLevel(capturedLevel));
                levelButtons.Add(levelButton);
            }

            int defaultLevel = Mathf.Clamp(ProgressManager.LastPlayedLevel, 1, Mathf.Max(1, levelScenes.Count));
            SelectLevel(ProgressManager.IsUnlocked(defaultLevel) ? defaultLevel : 1);
        }

        private SettingsPanelController BuildSettingsPanel(Transform parent)
        {
            GameObject dim = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            dim.transform.SetParent(parent, false);
            Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);

            GameObject panel = new GameObject("PanelCard", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(dim.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(620f, 620f);
            panel.GetComponent<Image>().color = new Color(0.035f, 0.052f, 0.075f, 0.98f);

            CreateText(panel.transform, "Settings", new Vector2(0f, 245f), new Vector2(440f, 60f), 36f, FontStyles.Bold);
            TMP_Text musicValue = CreateText(panel.transform, "72%", new Vector2(205f, 162f), new Vector2(80f, 34f), 20f, FontStyles.Normal);
            TMP_Text sfxValue = CreateText(panel.transform, "86%", new Vector2(205f, 82f), new Vector2(80f, 34f), 20f, FontStyles.Normal);
            Slider musicSlider = CreateSlider(panel.transform, "Music", new Vector2(20f, 145f));
            Slider sfxSlider = CreateSlider(panel.transform, "SFX", new Vector2(20f, 65f));
            Toggle muteToggle = CreateToggle(panel.transform, "Mute", new Vector2(-165f, -15f));
            TMP_Text controlsText = CreateText(panel.transform, "Controls: WASD / Arrow Keys / Swipe / D-Pad", new Vector2(0f, -80f), new Vector2(520f, 34f), 18f, FontStyles.Normal);
            controlsText.color = TextMutedColor;
            Button quality = CreateButton(panel.transform, $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}", new Color(0.12f, 0.16f, 0.22f, 0.95f), CycleQuality, false);
            SetRect(quality.GetComponent<RectTransform>(), new Vector2(0f, -135f), new Vector2(320f, 52f));
            Button reset = CreateButton(panel.transform, "Reset Progress", new Color(0.44f, 0.12f, 0.17f, 0.95f), ResetProgressAndReload, true);
            SetRect(reset.GetComponent<RectTransform>(), new Vector2(0f, -200f), new Vector2(320f, 52f));
            Button close = CreateButton(panel.transform, "Close", AccentColor, () => settingsPanel.Hide(), true);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0f, -265f), new Vector2(230f, 54f));

            SettingsPanelController controller = dim.AddComponent<SettingsPanelController>();
            controller.Configure(musicSlider, sfxSlider, muteToggle, musicValue, sfxValue);
            return controller;
        }

        private void ShowLevels()
        {
            ApplyMenuMode(true);
            CubeShiftAudio.Instance.PlayMusicState(MusicState.LevelSelect);
            RefreshProgressText();
        }

        private void ShowHome()
        {
            ApplyMenuMode(false);
            CubeShiftAudio.Instance.PlayMusicState(MusicState.MainMenu);
        }

        private void ApplyMenuMode(bool showLevelSelect)
        {
            if (homePanel != null)
            {
                homePanel.SetActive(!showLevelSelect);
            }

            if (levelPanel != null)
            {
                levelPanel.SetActive(showLevelSelect);
            }
        }

        private void SelectLevel(int levelNumber)
        {
            if (!ProgressManager.IsUnlocked(levelNumber))
            {
                return;
            }

            selectedLevel = levelNumber;
            foreach (LevelButtonUI levelButton in levelButtons)
            {
                levelButton.SetSelected(levelButton.LevelNumber == selectedLevel);
            }

            bool completed = ProgressManager.IsCompleted(levelNumber);
            int stars = ProgressManager.GetStars(levelNumber);
            if (previewTitleText != null)
            {
                previewTitleText.text = $"Level {levelNumber:00}";
            }

            if (previewDetailText != null)
            {
                previewDetailText.text = completed
                    ? BuildCompletedPreview(levelNumber, stars)
                    : $"Selected\nLevel {levelNumber:00} is ready.";
            }

            if (startSelectedButton != null)
            {
                startSelectedButton.interactable = true;
            }

            CubeShiftAudio.Instance.PlayUISelect();
        }

        private void StartSelectedLevel()
        {
            if (selectedLevel <= 0)
            {
                return;
            }

            LoadLevel(selectedLevel);
        }

        private void LoadLevel(int levelNumber)
        {
            string sceneName = levelScenes.FirstOrDefault(scene => ExtractNumber(scene) == levelNumber);
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            ProgressManager.MarkLevelStarted(levelNumber);
            transitionController.LoadScene(sceneName);
        }

        private void RefreshProgressText()
        {
            if (progressText == null)
            {
                return;
            }

            int total = levelScenes.Count;
            progressText.text = $"Progress: {ProgressManager.CountCompleted(total)} / {total}    Unlocked: {Mathf.Min(ProgressManager.UnlockedLevel, total)}";
        }

        private static string BuildCompletedPreview(int levelNumber, int stars)
        {
            float bestTime = ProgressManager.GetBestTime(levelNumber);
            int bestMoves = ProgressManager.GetBestMoves(levelNumber);
            string bestTimeText = bestTime > 0f ? $"{bestTime:0.0}s" : "--";
            string bestMovesText = bestMoves > 0 ? bestMoves.ToString() : "--";
            return $"Completed\nRating: {FormatRating(stars)}\nBest Time: {bestTimeText}\nBest Moves: {bestMovesText}";
        }

        private static void CycleQuality()
        {
            int count = QualitySettings.names.Length;
            if (count <= 0)
            {
                return;
            }

            QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % count, true);
        }

        private static void ResetProgressAndReload()
        {
            ProgressManager.ResetProgress();
            SceneManager.LoadScene("MainMenu");
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private void CleanupGeneratedMenuCanvases()
        {
            string[] generatedNames = { "MainMenuCanvas", "MainMenuUI" };
            foreach (string generatedName in generatedNames)
            {
                GameObject existing = GameObject.Find(generatedName);
                if (existing == null || existing.transform == transform || existing.GetComponentInParent<MainMenuController>() == this)
                {
                    continue;
                }

                existing.SetActive(false);
                Destroy(existing);
            }
        }

        private SceneTransitionController BuildTransition(Transform parent)
        {
            Image fade = CreateImage(parent, "SceneFade", Color.clear);
            Stretch(fade.rectTransform);
            fade.transform.SetAsLastSibling();
            SceneTransitionController controller = fade.gameObject.AddComponent<SceneTransitionController>();
            return controller;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            Stretch(panel.GetComponent<RectTransform>());
            return panel;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Button CreateButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction action, bool backSound)
        {
            GameObject buttonObject = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonAnimator), typeof(UIAudioFeedback));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420f, 66f);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);
            buttonObject.GetComponent<UIAudioFeedback>().SetBackSound(backSound);
            buttonObject.GetComponent<UIButtonAnimator>().SetColors(color, Color.Lerp(color, AccentColor, 0.35f), AccentColor);

            Image shine = CreateImage(buttonObject.transform, "ButtonShine", new Color(1f, 1f, 1f, 0.055f));
            shine.rectTransform.anchorMin = new Vector2(0f, 0.55f);
            shine.rectTransform.anchorMax = new Vector2(1f, 1f);
            shine.rectTransform.offsetMin = Vector2.zero;
            shine.rectTransform.offsetMax = Vector2.zero;
            shine.raycastTarget = false;

            Image leftAccent = CreateImage(buttonObject.transform, "LeftAccent", Color.Lerp(color, Color.white, 0.35f));
            leftAccent.rectTransform.anchorMin = new Vector2(0f, 0f);
            leftAccent.rectTransform.anchorMax = new Vector2(0f, 1f);
            leftAccent.rectTransform.pivot = new Vector2(0f, 0.5f);
            leftAccent.rectTransform.anchoredPosition = Vector2.zero;
            leftAccent.rectTransform.sizeDelta = new Vector2(6f, 0f);
            leftAccent.raycastTarget = false;

            TMP_Text text = CreateText(buttonObject.transform, label, Vector2.zero, rect.sizeDelta, 24f, FontStyles.Bold);
            text.raycastTarget = false;
            return button;
        }

        private static void EnsureMenuCamera()
        {
            Camera existing = Camera.main;
            if (existing != null)
            {
                existing.clearFlags = CameraClearFlags.SolidColor;
                existing.backgroundColor = BackgroundColor;
                return;
            }

            GameObject cameraObject = new GameObject("MainMenuCamera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.depth = -10f;
            cameraObject.tag = "MainCamera";
        }

        private static Slider CreateSlider(Transform parent, string label, Vector2 position)
        {
            CreateText(parent, label, position + new Vector2(-185f, 16f), new Vector2(120f, 34f), 20f, FontStyles.Bold).alignment = TextAlignmentOptions.Left;
            GameObject sliderObject = new GameObject($"{label}Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            RectTransform rect = sliderObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300f, 28f);

            Image background = CreateImage(sliderObject.transform, "Background", new Color(0.08f, 0.1f, 0.14f, 1f));
            Stretch(background.rectTransform);
            RectTransform fillArea = CreatePanel(sliderObject.transform, "Fill Area").GetComponent<RectTransform>();
            fillArea.offsetMin = new Vector2(8f, 6f);
            fillArea.offsetMax = new Vector2(-8f, -6f);
            Image fill = CreateImage(fillArea, "Fill", AccentColor);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;
            RectTransform handleArea = CreatePanel(sliderObject.transform, "Handle Slide Area").GetComponent<RectTransform>();
            handleArea.offsetMin = new Vector2(8f, 0f);
            handleArea.offsetMax = new Vector2(-8f, 0f);
            Image handle = CreateImage(handleArea, "Handle", Color.white);
            handle.rectTransform.sizeDelta = new Vector2(24f, 24f);

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string label, Vector2 position)
        {
            GameObject toggleObject = new GameObject($"{label}Toggle", typeof(RectTransform), typeof(Toggle));
            toggleObject.transform.SetParent(parent, false);
            RectTransform rect = toggleObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(220f, 42f);

            Image box = CreateImage(toggleObject.transform, "Box", new Color(0.08f, 0.1f, 0.14f, 1f));
            SetRect(box.rectTransform, new Vector2(-82f, 0f), new Vector2(34f, 34f));
            Image check = CreateImage(box.transform, "Check", AccentColor);
            SetRect(check.rectTransform, Vector2.zero, new Vector2(20f, 20f));
            TMP_Text text = CreateText(toggleObject.transform, label, new Vector2(22f, 0f), new Vector2(140f, 36f), 20f, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Left;

            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            return toggle;
        }

        private static TMP_Text CreateText(Transform parent, string value, Vector2 position, Vector2 size, float fontSize, FontStyles style)
        {
            GameObject textObject = new GameObject($"{SanitizeName(value)}Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return text;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static int ExtractNumber(string sceneName)
        {
            string digits = new string(sceneName.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int number) ? number : int.MaxValue;
        }

        private static string FormatRating(int stars)
        {
            return Mathf.Clamp(stars, 1, 3) switch
            {
                3 => "S",
                2 => "A",
                _ => "B"
            };
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Text";
            }

            return new string(value.Where(char.IsLetterOrDigit).ToArray());
        }

        private static Sprite CreateSoftCircleSprite()
        {
            const int size = 128;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            Vector2 center = Vector2.one * (size - 1) * 0.5f;
            float radius = size * 0.48f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(1f - distance / radius);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha * alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
