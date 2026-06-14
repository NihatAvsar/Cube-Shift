using System.Collections;
using CubeShift.Audio;
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
        [SerializeField] private bool logDebugMessages;
        [Tooltip("When enabled, the goal tile only completes after the LevelData required objectives are registered. Leave disabled for normal goal-based completion.")]
        [SerializeField] private bool requireObjectivesBeforeGoal;

        [Header("Optional UI")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultStarsText;
        [SerializeField] private TMP_Text resultTimeText;
        [SerializeField] private TMP_Text resultBestText;

        private bool isTransitioning;
        private bool isLoadingScene;
        private bool isLevelComplete;
        [SerializeField] private LevelData activeLevel;
        private LevelObjective completedObjectives;
        private float levelStartTime;
        private int moveCount;

        public bool IsTransitioning => isTransitioning || isLoadingScene;
        public bool IsLevelComplete => isLevelComplete;
        public LevelData ActiveLevel => activeLevel;
        public LevelObjective CompletedObjectives => completedObjectives;
        public int MoveCount => moveCount;

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
            isTransitioning = false;
            isLoadingScene = false;
            isLevelComplete = false;
            completedObjectives = LevelObjective.None;
            Time.timeScale = 1f;

            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(false);
            }

            UpdateLevelText();
            ProgressManager.MarkLevelStarted(GetDisplayLevelNumber());
            levelStartTime = Time.time;
            moveCount = 0;
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

            Time.timeScale = 1f;
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

            if (logDebugMessages)
            {
                Debug.Log($"[LevelManager] CompleteLevel called. Required={activeLevel?.RequiredObjectives} | Completed={completedObjectives}", this);
            }

            if (!CanCompleteActiveLevel())
            {
                LevelObjective missing = activeLevel != null ? (activeLevel.RequiredObjectives & ~completedObjectives) : LevelObjective.None;
                if (logDebugMessages)
                {
                    Debug.Log($"[LevelManager] Goal locked. Missing={missing}", this);
                }

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
            moveCount = 0;
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

        public void RegisterMove()
        {
            if (!isLevelComplete && !isLoadingScene)
            {
                moveCount++;
            }
        }

        public bool CanCompleteActiveLevel()
        {
            return !requireObjectivesBeforeGoal
                || activeLevel == null
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
                    if (logDebugMessages)
                    {
                        Debug.Log("No next scene is configured in Build Settings yet.", this);
                    }
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
            int stars = CalculateStars(completionTime, moveCount);
            int levelNumber = GetDisplayLevelNumber();
            ProgressManager.MarkLevelComplete(levelNumber, stars, completionTime, moveCount);

            CubeShiftAudio.Instance.PlayWin();
            JuiceEffectsManager.Instance.SpawnVictoryConfetti();

            if (AdManager.Instance != null)
            {
                AdManager.Instance.ShowInterstitialAdWithInterval();
            }

            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }

            UpdateResultMenu(completionTime, stars);
            StartCoroutine(ResultPulseRoutine());

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

        private int GetDisplayLevelNumber()
        {
            if (levelNumberOverride > 0)
            {
                return levelNumberOverride;
            }

            int extracted = ExtractLevelNumber(SceneManager.GetActiveScene().name);
            if (extracted > 0)
            {
                return extracted;
            }

            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            return buildIndex >= 0 ? buildIndex + 1 : 1;
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

        private int CalculateStars(float completionTime, int moves)
        {
            float sTime = CalculateRankTargetSeconds();
            int sMoves = CalculateRankTargetMoves();
            bool fastEnough = completionTime <= sTime;
            bool cleanEnough = moves <= sMoves;

            if (fastEnough && cleanEnough)
            {
                return 3;
            }

            bool acceptableTime = completionTime <= sTime * 1.7f;
            bool acceptableMoves = moves <= Mathf.CeilToInt(sMoves * 1.6f);
            return (fastEnough || cleanEnough || (acceptableTime && acceptableMoves)) ? 2 : 1;
        }

        private float CalculateRankTargetSeconds()
        {
            if (activeLevel == null)
            {
                return 30f;
            }

            int objectiveCount = CountObjectives(activeLevel.RequiredObjectives);
            return Mathf.Max(18f, (activeLevel.Width + activeLevel.Height) * 0.7f + objectiveCount * 4f);
        }

        private int CalculateRankTargetMoves()
        {
            if (activeLevel == null)
            {
                return 20;
            }

            int objectiveCount = CountObjectives(activeLevel.RequiredObjectives);
            return Mathf.Max(8, Mathf.CeilToInt((activeLevel.Width + activeLevel.Height) * 0.45f + objectiveCount * 3f));
        }

        private static int CountObjectives(LevelObjective objectives)
        {
            int bits = (int)objectives;
            int count = 0;
            while (bits != 0)
            {
                count += bits & 1;
                bits >>= 1;
            }

            return count;
        }

        private static string FormatRank(int stars)
        {
            return Mathf.Clamp(stars, 1, 3) switch
            {
                3 => "S",
                2 => "A",
                _ => "B"
            };
        }

        private void UpdateResultMenu(float completionTime, int stars)
        {
            if (resultTitleText != null)
            {
                resultTitleText.text = "Level Complete";
            }

            if (resultStarsText != null)
            {
                resultStarsText.text = $"Rank {FormatRank(stars)}";
            }

            if (resultTimeText != null)
            {
                float sTarget = CalculateRankTargetSeconds();
                int moveTarget = CalculateRankTargetMoves();
                resultTimeText.text = $"Time {completionTime:0.0}s / S {sTarget:0}s   Moves {moveCount} / S {moveTarget}";
            }

            if (resultBestText != null)
            {
                int levelNumber = GetDisplayLevelNumber();
                float bestTime = ProgressManager.GetBestTime(levelNumber);
                int bestMoves = ProgressManager.GetBestMoves(levelNumber);
                string bestTimeText = bestTime > 0f ? $"{bestTime:0.0}s" : "--";
                string bestMovesText = bestMoves > 0 ? bestMoves.ToString() : "--";
                resultBestText.text = $"Best Time {bestTimeText}   Best Moves {bestMovesText}";
            }
        }

        private void EnsureResultMenu()
        {
            Canvas canvas = null;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.scene == gameObject.scene)
                {
                    canvas = c;
                    break;
                }
            }

            if (canvas == null)
            {
                foreach (Canvas c in canvases)
                {
                    if (c.gameObject.scene.name != "DontDestroyOnLoad")
                    {
                        canvas = c;
                        break;
                    }
                }
            }

            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            if (canvas.transform.localScale != Vector3.one)
            {
                canvas.transform.localScale = Vector3.one;
            }

            if (levelCompletePanel != null && resultStarsText != null)
            {
                return;
            }

            levelCompletePanel = new GameObject("LevelResultMenu", typeof(RectTransform), typeof(Image));
            levelCompletePanel.transform.SetParent(canvas.transform, false);
            levelCompletePanel.transform.localScale = Vector3.one;
            RectTransform panel = levelCompletePanel.GetComponent<RectTransform>();
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            levelCompletePanel.GetComponent<Image>().color = new Color(0.025f, 0.03f, 0.04f, 0.94f);

            resultTitleText = CreateResultText("ResultTitle", new Vector2(0f, 230f), 54f);
            resultStarsText = CreateResultText("Rank", new Vector2(0f, 120f), 88f);
            resultStarsText.color = new Color(1f, 0.82f, 0.15f);
            resultTimeText = CreateResultText("ResultStats", new Vector2(0f, 30f), 30f);
            resultBestText = CreateResultText("ResultBest", new Vector2(0f, -25f), 26f);
            resultBestText.color = new Color(0.7f, 0.78f, 0.86f);
            CreateResultButton("Retry", new Vector2(-330f, -155f), RestartLevel);
            CreateResultButton("Next Level", new Vector2(-110f, -155f), LoadNextLevel);
            CreateResultButton("Level Select", new Vector2(110f, -155f), LoadMainMenu);
            CreateResultButton("Exit", new Vector2(330f, -155f), ExitGame);
            levelCompletePanel.SetActive(false);
        }

        private IEnumerator ResultPulseRoutine()
        {
            if (resultStarsText == null)
            {
                yield break;
            }

            RectTransform rect = resultStarsText.rectTransform;
            float elapsed = 0f;
            const float duration = 0.35f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = 1f + Mathf.Sin(t * Mathf.PI) * 0.16f;
                rect.localScale = Vector3.one * pulse;
                yield return null;
            }

            rect.localScale = Vector3.one;
        }

        private TMP_Text CreateResultText(string objectName, Vector2 position, float fontSize)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(levelCompletePanel.transform, false);
            textObject.transform.localScale = Vector3.one;
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(900f, 100f);
            return text;
        }

        private void CreateResultButton(string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(levelCompletePanel.transform, false);
            buttonObject.transform.localScale = Vector3.one;
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200f, 64f);
            buttonObject.GetComponent<Image>().color = label == "Next Level" || label == "Retry"
                ? new Color(0.2f, 0.78f, 0.48f)
                : new Color(0.18f, 0.22f, 0.28f);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);

            TMP_Text text = CreateResultText($"{label}Label", Vector2.zero, 28f);
            text.transform.SetParent(buttonObject.transform, false);
            text.transform.localScale = Vector3.one;
            text.text = label;
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.rectTransform.sizeDelta = rect.sizeDelta;
        }
    }
}
