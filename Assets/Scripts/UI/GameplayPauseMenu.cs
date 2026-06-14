using CubeShift.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace CubeShift.UI
{
    [DefaultExecutionOrder(-500)]
    public sealed class GameplayPauseMenu : MonoBehaviour
    {
        private const string MainMenuSceneName = "MainMenu";
        private static GameplayPauseMenu instance;

        private Canvas canvas;
        private GameObject pauseButtonObject;
        private GameObject overlayObject;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Toggle muteToggle;
        private TMP_Text musicValueText;
        private TMP_Text sfxValueText;
        private bool paused;
        private float previousTimeScale = 1f;

        private static readonly Color AccentColor = new(0.05f, 0.78f, 0.9f, 1f);
        private static readonly Color PanelColor = new(0.035f, 0.052f, 0.075f, 0.98f);
        private static readonly Color MutedTextColor = new(0.66f, 0.75f, 0.82f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureExists();
        }

        private static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject menuObject = new GameObject("GameplayPauseMenu");
            DontDestroyOnLoad(menuObject);
            instance = menuObject.AddComponent<GameplayPauseMenu>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureEventSystem();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            RebuildForScene(SceneManager.GetActiveScene());
        }

        private void Update()
        {
            if (!IsLevelScene(SceneManager.GetActiveScene()))
            {
                return;
            }

            if (WasPausePressed())
            {
                if (paused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        private static bool WasPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null
                && (keyboard.escapeKey.wasPressedThisFrame
                    || keyboard.pKey.wasPressedThisFrame
                    || keyboard.spaceKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Space);
#endif
        }

        public void Pause()
        {
            if (paused)
            {
                return;
            }

            paused = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            RefreshAudioControls();
            if (overlayObject != null)
            {
                overlayObject.SetActive(true);
            }

            CubeShiftAudio.Instance.PlayUIClick();
        }

        public void Resume()
        {
            if (!paused)
            {
                return;
            }

            paused = false;
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            if (overlayObject != null)
            {
                overlayObject.SetActive(false);
            }

            CubeShiftAudio.Instance.PlayUIBack();
        }

        public void ExitToMainMenu()
        {
            paused = false;
            Time.timeScale = 1f;
            CubeShiftAudio.Instance.PlayUIBack();
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            paused = false;
            Time.timeScale = 1f;
            RebuildForScene(scene);
        }

        private void RebuildForScene(Scene scene)
        {
            if (canvas != null)
            {
                Destroy(canvas.gameObject);
                canvas = null;
            }

            if (!IsLevelScene(scene))
            {
                return;
            }

            BuildCanvas();
            BuildPauseButton();
            BuildOverlay();
            RefreshAudioControls();
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("GameplayPauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildPauseButton()
        {
            Button button = CreateButton(canvas.transform, "Pause", new Color(0.06f, 0.12f, 0.16f, 0.86f), Pause);
            pauseButtonObject = button.gameObject;
            RectTransform rect = pauseButtonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -28f);
            rect.sizeDelta = new Vector2(132f, 52f);
        }

        private void BuildOverlay()
        {
            overlayObject = new GameObject("PauseOverlay", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(canvas.transform, false);
            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            Stretch(overlayRect);
            overlayObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);

            GameObject panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlayObject.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(560f, 620f);
            panel.GetComponent<Image>().color = PanelColor;

            CreateText(panel.transform, "Paused", new Vector2(0f, 230f), new Vector2(420f, 64f), 44f, FontStyles.Bold);
            CreateText(panel.transform, "Audio", new Vector2(-170f, 120f), new Vector2(160f, 34f), 24f, FontStyles.Bold).alignment = TextAlignmentOptions.Left;

            musicValueText = CreateText(panel.transform, "0%", new Vector2(185f, 68f), new Vector2(90f, 34f), 20f, FontStyles.Normal);
            sfxValueText = CreateText(panel.transform, "0%", new Vector2(185f, -12f), new Vector2(90f, 34f), 20f, FontStyles.Normal);
            musicSlider = CreateSlider(panel.transform, "Music", new Vector2(0f, 52f));
            sfxSlider = CreateSlider(panel.transform, "SFX", new Vector2(0f, -28f));
            muteToggle = CreateToggle(panel.transform, "Mute", new Vector2(-145f, -105f));

            musicSlider.onValueChanged.AddListener(HandleMusicChanged);
            sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
            muteToggle.onValueChanged.AddListener(HandleMuteChanged);

            Button continueButton = CreateButton(panel.transform, "Continue", AccentColor, Resume);
            SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(0f, -190f), new Vector2(300f, 62f));
            Button exitButton = CreateButton(panel.transform, "Exit", new Color(0.44f, 0.12f, 0.17f, 0.95f), ExitToMainMenu);
            SetRect(exitButton.GetComponent<RectTransform>(), new Vector2(0f, -270f), new Vector2(300f, 62f));

            overlayObject.SetActive(false);
        }

        private void RefreshAudioControls()
        {
            CubeShiftAudio audio = CubeShiftAudio.Instance;
            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(audio.MusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(audio.SfxVolume);
            }

            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(audio.Muted);
            }

            UpdateAudioLabels();
        }

        private void HandleMusicChanged(float value)
        {
            CubeShiftAudio.Instance.SetMusicVolume(value);
            UpdateAudioLabels();
        }

        private void HandleSfxChanged(float value)
        {
            CubeShiftAudio.Instance.SetSfxVolume(value);
            CubeShiftAudio.Instance.PlayUISelect();
            UpdateAudioLabels();
        }

        private void HandleMuteChanged(bool value)
        {
            CubeShiftAudio.Instance.SetMuted(value);
            UpdateAudioLabels();
        }

        private void UpdateAudioLabels()
        {
            if (musicValueText != null && musicSlider != null)
            {
                musicValueText.text = $"{Mathf.RoundToInt(musicSlider.value * 100f)}%";
            }

            if (sfxValueText != null && sfxSlider != null)
            {
                sfxValueText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100f)}%";
            }
        }

        private static bool IsLevelScene(Scene scene)
        {
            return scene.IsValid() && scene.name.StartsWith("Level_");
        }

        private static Button CreateButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonAnimator), typeof(UIAudioFeedback));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);
            buttonObject.GetComponent<UIButtonAnimator>().SetColors(color, Color.Lerp(color, AccentColor, 0.35f), AccentColor);

            TMP_Text text = CreateText(buttonObject.transform, label, Vector2.zero, new Vector2(300f, 62f), 24f, FontStyles.Bold);
            text.raycastTarget = false;
            return button;
        }

        private static Slider CreateSlider(Transform parent, string label, Vector2 position)
        {
            CreateText(parent, label, position + new Vector2(-185f, 16f), new Vector2(120f, 34f), 20f, FontStyles.Bold).alignment = TextAlignmentOptions.Left;
            GameObject sliderObject = new GameObject($"{label}Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            SetRect(sliderObject.GetComponent<RectTransform>(), position, new Vector2(300f, 28f));

            Image background = CreateImage(sliderObject.transform, "Background", new Color(0.08f, 0.1f, 0.14f, 1f));
            Stretch(background.rectTransform);
            RectTransform fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
            fillArea.transform.SetParent(sliderObject.transform, false);
            Stretch(fillArea);
            fillArea.offsetMin = new Vector2(8f, 6f);
            fillArea.offsetMax = new Vector2(-8f, -6f);
            Image fill = CreateImage(fillArea, "Fill", AccentColor);
            Stretch(fill.rectTransform);

            RectTransform handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.transform.SetParent(sliderObject.transform, false);
            Stretch(handleArea);
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
            SetRect(toggleObject.GetComponent<RectTransform>(), position, new Vector2(220f, 42f));

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

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(Transform parent, string value, Vector2 position, Vector2 size, float fontSize, FontStyles style)
        {
            GameObject textObject = new GameObject($"{value}Text", typeof(RectTransform), typeof(TextMeshProUGUI));
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
    }
}
