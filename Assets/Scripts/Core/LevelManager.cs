using System.Collections;
using CubeShift.LevelDesign;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CubeShift.Core
{
    /// <summary>
    /// Handles basic scene-based level flow for the MVP: restart, completion, and next-scene loading.
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Flow")]
        [SerializeField, Min(0f)] private float completeDelay = 0.35f;
        [SerializeField] private bool loopToFirstLevelAfterLast = true;
        [SerializeField] private string firstLevelSceneName = "Level_01";
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField, Min(0)] private int levelNumberOverride;

        [Header("Optional UI")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultStarsText;
        [SerializeField] private TMP_Text resultTimeText;

        private bool isTransitioning;
        private bool isLoadingScene;
        private bool isLevelComplete;
        [SerializeField] private LevelData activeLevel;
        private LevelObjective completedObjectives;
        private float levelStartTime;

        public bool IsTransitioning => isTransitioning || isLoadingScene;
        public bool IsLevelComplete => isLevelComplete;
        public LevelData ActiveLevel => activeLevel;
        public LevelObjective CompletedObjectives => completedObjectives;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple LevelManager instances found. The latest scene instance will be used.", this);
            }

            Instance = this;
            EnsureResultMenu();
        }

        private void Start()
        {
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(false);
            }

            UpdateLevelText();
            levelStartTime = Time.time;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RestartLevel()
        {
            if (isLoadingScene)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex >= 0)
            {
                StartCoroutine(LoadSceneRoutine(activeScene.buildIndex, 0f));
                return;
            }

            if (!string.IsNullOrEmpty(activeScene.name))
            {
                StartCoroutine(LoadSceneRoutine(activeScene.name, 0f));
                return;
            }

            Debug.LogWarning("Current scene cannot be restarted because it has no build index or scene name.", this);
        }

        public void CompleteLevel()
        {
            if (isLevelComplete || isTransitioning || isLoadingScene)
            {
                return;
            }

            if (!CanCompleteActiveLevel())
            {
                Debug.Log($"Goal locked. Missing objectives: {activeLevel.RequiredObjectives & ~completedObjectives}", this);
                return;
            }

            StartCoroutine(CompleteLevelRoutine());
        }

        public void ConfigureLevel(LevelData levelData)
        {
            activeLevel = levelData;
            completedObjectives = LevelObjective.None;
            isTransitioning = false;
            isLoadingScene = false;
            isLevelComplete = false;
            levelStartTime = Time.time;
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(false);
            }
            levelNumberOverride = levelData != null ? ExtractLevelNumber(levelData.LevelName) : levelNumberOverride;
            UpdateLevelText();
        }

        public void RegisterObjective(LevelObjective objective)
        {
            completedObjectives |= objective;
        }

        public bool CanCompleteActiveLevel()
        {
            return activeLevel == null
                || (completedObjectives & activeLevel.RequiredObjectives) == activeLevel.RequiredObjectives;
        }

        public void LoadNextLevel()
        {
            if (isLoadingScene)
            {
                return;
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (!CanLoadScene(nextSceneIndex) || !IsLevelScene(nextSceneIndex))
            {
                if (CanLoadScene(mainMenuSceneName))
                {
                    StartCoroutine(LoadSceneRoutine(mainMenuSceneName, 0f));
                }
                else if (loopToFirstLevelAfterLast && CanLoadScene(firstLevelSceneName))
                {
                    StartCoroutine(LoadSceneRoutine(firstLevelSceneName, 0f));
                }
                else
                {
                    Debug.Log("No next scene is configured in Build Settings yet.", this);
                }
                return;
            }

            StartCoroutine(LoadSceneRoutine(nextSceneIndex, 0f));
        }

        private IEnumerator CompleteLevelRoutine()
        {
            isTransitioning = true;
            isLevelComplete = true;
            float completionTime = Mathf.Max(0f, Time.time - levelStartTime);
            int stars = CalculateStars(completionTime);

            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }

            UpdateResultMenu(completionTime, stars);

            if (completeDelay > 0f)
            {
                yield return new WaitForSeconds(completeDelay);
            }

            // The result menu now owns the next-level transition.
        }

        private IEnumerator LoadSceneRoutine(int sceneIndex, float delay)
        {
            isTransitioning = true;
            isLoadingScene = true;

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            SceneManager.LoadScene(sceneIndex);
        }

        private IEnumerator LoadSceneRoutine(string sceneName, float delay)
        {
            isTransitioning = true;
            isLoadingScene = true;

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            SceneManager.LoadScene(sceneName);
        }

        private void UpdateLevelText()
        {
            if (levelText == null)
            {
                return;
            }

            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            int displayLevel = levelNumberOverride > 0
                ? levelNumberOverride
                : buildIndex >= 0
                    ? buildIndex + 1
                    : 1;

            levelText.text = $"Level {displayLevel}";
        }

        private static bool CanLoadScene(int sceneIndex)
        {
            return sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings;
        }

        private static bool CanLoadScene(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
        }

        private static bool IsLevelScene(int sceneIndex)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            return sceneName.StartsWith("Level_");
        }

        private static int ExtractLevelNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            string digits = string.Empty;
            foreach (char character in value)
            {
                if (char.IsDigit(character))
                {
                    digits += character;
                }
            }

            return int.TryParse(digits, out int result) ? result : 0;
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadMainMenu()
        {
            if (!isLoadingScene && CanLoadScene(mainMenuSceneName))
            {
                StartCoroutine(LoadSceneRoutine(mainMenuSceneName, 0f));
            }
        }

        private int CalculateStars(float completionTime)
        {
            float baseTime = activeLevel != null
                ? Mathf.Max(45f, activeLevel.Width * 3f + activeLevel.Height * 2f)
                : 60f;

            if (completionTime <= baseTime)
            {
                return 3;
            }

            return completionTime <= baseTime * 1.75f ? 2 : 1;
        }

        private void UpdateResultMenu(float completionTime, int stars)
        {
            if (resultTitleText != null)
            {
                resultTitleText.text = "Level Complete";
            }

            if (resultStarsText != null)
            {
                resultStarsText.text = new string('★', stars) + new string('☆', 3 - stars);
            }

            if (resultTimeText != null)
            {
                resultTimeText.text = $"Time: {completionTime:0.0}s";
            }
        }

        private void EnsureResultMenu()
        {
            if (levelCompletePanel != null && resultStarsText != null)
            {
                return;
            }

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            levelCompletePanel = new GameObject("LevelResultMenu", typeof(RectTransform), typeof(Image));
            levelCompletePanel.transform.SetParent(canvas.transform, false);
            RectTransform panel = levelCompletePanel.GetComponent<RectTransform>();
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            levelCompletePanel.GetComponent<Image>().color = new Color(0.025f, 0.03f, 0.04f, 0.94f);

            resultTitleText = CreateResultText("ResultTitle", new Vector2(0f, 180f), 58f);
            resultStarsText = CreateResultText("Stars", new Vector2(0f, 70f), 82f);
            resultStarsText.color = new Color(1f, 0.82f, 0.15f);
            resultTimeText = CreateResultText("ResultTime", new Vector2(0f, -25f), 34f);
            CreateResultButton("Next Level", new Vector2(-250f, -145f), LoadNextLevel);
            CreateResultButton("Main Menu", new Vector2(0f, -145f), LoadMainMenu);
            CreateResultButton("Exit", new Vector2(250f, -145f), ExitGame);
            levelCompletePanel.SetActive(false);
        }

        private TMP_Text CreateResultText(string objectName, Vector2 position, float fontSize)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(levelCompletePanel.transform, false);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(700f, 100f);
            return text;
        }

        private void CreateResultButton(string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(levelCompletePanel.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(220f, 64f);
            buttonObject.GetComponent<Image>().color = label == "Next Level"
                ? new Color(0.2f, 0.78f, 0.48f)
                : new Color(0.72f, 0.24f, 0.24f);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);

            TMP_Text text = CreateResultText($"{label}Label", Vector2.zero, 28f);
            text.transform.SetParent(buttonObject.transform, false);
            text.text = label;
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.rectTransform.sizeDelta = rect.sizeDelta;
        }
    }
}
