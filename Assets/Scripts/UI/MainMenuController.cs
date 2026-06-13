using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <summary>Builds the main menu and discovers playable Level_XX scenes from Build Settings.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private readonly List<string> levelScenes = new();
        private GameObject homePanel;
        private GameObject levelPanel;
        private Sprite circleSprite;

        private void Awake()
        {
            DiscoverLevels();
            EnsureEventSystem();
            BuildMenu();
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
            Canvas canvas = CreateCanvas();
            circleSprite = CreateCircleSprite();

            Image background = CreateImage(canvas.transform, "Background", new Color(0.025f, 0.03f, 0.04f, 1f));
            Stretch(background.rectTransform);

            homePanel = CreatePanel(canvas.transform, "HomePanel");
            CreateText(homePanel.transform, "Cube Shift", new Vector2(0f, 180f), new Vector2(700f, 110f), 72f);
            CreateButton(homePanel.transform, "Başla", new Vector2(0f, 30f), new Vector2(300f, 76f),
                new Color(0.16f, 0.78f, 0.48f), ShowLevels, false);
            CreateButton(homePanel.transform, "Exit", new Vector2(0f, -75f), new Vector2(300f, 76f),
                new Color(0.76f, 0.22f, 0.24f), ExitGame, false);

            levelPanel = CreatePanel(canvas.transform, "LevelSelectionPanel");
            CreateText(levelPanel.transform, "Bölüm Seç", new Vector2(0f, 300f), new Vector2(700f, 90f), 54f);
            CreateLevelButtons(levelPanel.transform);
            CreateButton(levelPanel.transform, "Geri", new Vector2(0f, -330f), new Vector2(220f, 62f),
                new Color(0.3f, 0.34f, 0.4f), ShowHome, false);
            levelPanel.SetActive(false);
        }

        private void CreateLevelButtons(Transform parent)
        {
            GameObject gridObject = new GameObject("LevelGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(parent, false);
            RectTransform rect = gridObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(600f, 520f);

            GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(78f, 78f);
            grid.spacing = new Vector2(20f, 18f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment = TextAnchor.MiddleCenter;

            foreach (string sceneName in levelScenes)
            {
                int levelNumber = ExtractNumber(sceneName);
                CreateButton(gridObject.transform, levelNumber.ToString(), Vector2.zero, grid.cellSize,
                    new Color(0.18f, 0.72f, 0.9f), () => SceneManager.LoadScene(sceneName), true);
            }
        }

        private void ShowLevels()
        {
            homePanel.SetActive(false);
            levelPanel.SetActive(true);
        }

        private void ShowHome()
        {
            levelPanel.SetActive(false);
            homePanel.SetActive(true);
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
            GameObject canvasObject = new GameObject("MainMenuUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
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

        private void CreateButton(
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Color color,
            UnityEngine.Events.UnityAction action,
            bool circular)
        {
            GameObject buttonObject = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            if (circular)
            {
                image.sprite = circleSprite;
                image.preserveAspect = true;
            }

            buttonObject.GetComponent<Button>().onClick.AddListener(action);
            TMP_Text text = CreateText(buttonObject.transform, label, Vector2.zero, size, circular ? 40f : 30f);
            text.raycastTarget = false;
        }

        private static TMP_Text CreateText(Transform parent, string value, Vector2 position, Vector2 size, float fontSize)
        {
            GameObject textObject = new GameObject($"{value}Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return text;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                eventSystem.AddComponent<InputSystemUIInputModule>();
#else
                eventSystem.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static int ExtractNumber(string sceneName)
        {
            string digits = new string(sceneName.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int number) ? number : int.MaxValue;
        }

        private static Sprite CreateCircleSprite()
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            Vector2 center = Vector2.one * (size - 1) * 0.5f;
            float radius = size * 0.48f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = Vector2.Distance(new Vector2(x, y), center) <= radius
                        ? Color.white
                        : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
