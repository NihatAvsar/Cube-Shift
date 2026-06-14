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
    [DefaultExecutionOrder(-425)]
    public sealed class TutorialOverlay : MonoBehaviour
    {
        private static TutorialOverlay instance;

        private Canvas canvas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject overlayObject = new GameObject("TutorialOverlay");
            DontDestroyOnLoad(overlayObject);
            instance = overlayObject.AddComponent<TutorialOverlay>();
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

            int levelNumber = ExtractLevelNumber(scene.name);
            if (levelNumber <= 0 || levelNumber > 3 || PlayerPrefs.GetInt($"CubeShift.Tutorial.Seen.{levelNumber}", 0) == 1)
            {
                return;
            }

            BuildCanvas(levelNumber);
        }

        private void BuildCanvas(int levelNumber)
        {
            GameObject canvasObject = new GameObject("TutorialCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject panel = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -72f);
            panelRect.sizeDelta = new Vector2(760f, 190f);
            panel.GetComponent<Image>().color = new Color(0.035f, 0.052f, 0.075f, 0.94f);

            TMP_Text title = CreateText(panel.transform, $"Level {levelNumber} Tip", new Vector2(-250f, 46f), new Vector2(220f, 40f), 24f, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Left;
            TMP_Text body = CreateText(panel.transform, GetTip(levelNumber), new Vector2(-25f, -8f), new Vector2(600f, 86f), 20f, FontStyles.Normal);
            body.alignment = TextAlignmentOptions.Left;
            body.color = new Color(0.78f, 0.86f, 0.92f, 1f);

            Button gotIt = CreateButton(panel.transform, "Got it");
            RectTransform buttonRect = gotIt.GetComponent<RectTransform>();
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.anchoredPosition = new Vector2(-24f, 22f);
            buttonRect.sizeDelta = new Vector2(140f, 46f);
            gotIt.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt($"CubeShift.Tutorial.Seen.{levelNumber}", 1);
                PlayerPrefs.Save();
                if (canvas != null)
                {
                    Destroy(canvas.gameObject);
                    canvas = null;
                }
            });
        }

        private static string GetTip(int levelNumber)
        {
            return levelNumber switch
            {
                1 => "Swipe, use WASD, arrow keys, or the D-Pad to roll one tile at a time.",
                2 => "Plan your cube faces. Colored mechanics care about which face is touching the tile.",
                _ => "Ice, jump, buttons, bridges, and breakable tiles can chain effects. Watch the landing tile."
            };
        }

        private static Button CreateButton(Transform parent, string label)
        {
            GameObject buttonObject = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonAnimator));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            Color color = new Color(0.05f, 0.78f, 0.9f, 0.95f);
            image.color = color;
            buttonObject.GetComponent<UIButtonAnimator>().SetColors(color, Color.Lerp(color, Color.white, 0.2f), color);
            TMP_Text text = CreateText(buttonObject.transform, label, Vector2.zero, new Vector2(140f, 46f), 18f, FontStyles.Bold);
            text.raycastTarget = false;
            return buttonObject.GetComponent<Button>();
        }

        private static TMP_Text CreateText(Transform parent, string value, Vector2 position, Vector2 size, float fontSize, FontStyles style)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
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

        private static int ExtractLevelNumber(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || !sceneName.StartsWith("Level_"))
            {
                return 0;
            }

            string digits = string.Empty;
            foreach (char character in sceneName)
            {
                if (char.IsDigit(character))
                {
                    digits += character;
                }
            }

            return int.TryParse(digits, out int result) ? result : 0;
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
