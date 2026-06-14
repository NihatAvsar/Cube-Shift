using CubeShift.Audio;
using CubeShift.Player;
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
    [DefaultExecutionOrder(-450)]
    public sealed class MobileGameplayControls : MonoBehaviour
    {
        private static MobileGameplayControls instance;

        private Canvas canvas;
        private PlayerCubeController player;
        private TMP_Text feedbackText;

        private static readonly Color ButtonColor = new(0.04f, 0.1f, 0.14f, 0.78f);
        private static readonly Color AccentColor = new(0.05f, 0.78f, 0.9f, 0.95f);
        private static readonly Color WarnColor = new(1f, 0.32f, 0.22f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject controlsObject = new GameObject("MobileGameplayControls");
            DontDestroyOnLoad(controlsObject);
            instance = controlsObject.AddComponent<MobileGameplayControls>();
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
            if (canvas == null)
            {
                return;
            }

            if (player == null)
            {
                player = FindAnyObjectByType<PlayerCubeController>();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebuildForScene(scene);
        }

        private void RebuildForScene(Scene scene)
        {
            if (canvas != null)
            {
                Destroy(canvas.gameObject);
                canvas = null;
            }

            if (!scene.IsValid() || !scene.name.StartsWith("Level_"))
            {
                return;
            }

            player = FindAnyObjectByType<PlayerCubeController>();
            BuildCanvas();
            BuildDPad();
            BuildFeedbackText();
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("MobileControlsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 45;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildDPad()
        {
            GameObject pad = new GameObject("DPad", typeof(RectTransform));
            pad.transform.SetParent(canvas.transform, false);
            RectTransform padRect = pad.GetComponent<RectTransform>();
            padRect.anchorMin = padRect.anchorMax = new Vector2(1f, 0f);
            padRect.pivot = new Vector2(1f, 0f);
            padRect.anchoredPosition = new Vector2(-42f, 42f);
            padRect.sizeDelta = new Vector2(300f, 300f);

            CreateDirectionButton(pad.transform, "Up", "^", new Vector2(0f, 92f), Vector2Int.up);
            CreateDirectionButton(pad.transform, "Down", "v", new Vector2(0f, -92f), Vector2Int.down);
            CreateDirectionButton(pad.transform, "Left", "<", new Vector2(-92f, 0f), Vector2Int.left);
            CreateDirectionButton(pad.transform, "Right", ">", new Vector2(92f, 0f), Vector2Int.right);
        }

        private void BuildFeedbackText()
        {
            feedbackText = CreateText(canvas.transform, "Move", new Vector2(0f, 105f), new Vector2(520f, 44f), 24f, FontStyles.Bold);
            feedbackText.color = new Color(1f, 1f, 1f, 0f);
            RectTransform rect = feedbackText.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
        }

        private void CreateDirectionButton(Transform parent, string name, string label, Vector2 position, Vector2Int direction)
        {
            Button button = CreateButton(parent, $"{name}Button", label, new Vector2(76f, 76f), ButtonColor);
            SetRect(button.GetComponent<RectTransform>(), position, new Vector2(76f, 76f));
            DirectionHoldButton holdButton = button.gameObject.AddComponent<DirectionHoldButton>();
            holdButton.Configure(player, direction);
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
            feedbackText.color = message == "No tile there" || message == "Blocked" ? WarnColor : AccentColor;
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 0.9f);
        }

        private void HideFeedback()
        {
            if (feedbackText != null)
            {
                feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 0f);
            }
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Color color)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonAnimator));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            buttonObject.GetComponent<UIButtonAnimator>().SetColors(color, Color.Lerp(color, AccentColor, 0.35f), AccentColor);
            TMP_Text text = CreateText(buttonObject.transform, label, Vector2.zero, size, 28f, FontStyles.Bold);
            text.raycastTarget = false;
            return button;
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
            SetRect(rect, position, size);
            return text;
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
