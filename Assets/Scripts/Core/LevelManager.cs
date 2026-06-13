using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        [SerializeField] private bool loadNextSceneWhenAvailable = true;
        [SerializeField, Min(0)] private int levelNumberOverride;

        [Header("Optional UI")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private GameObject levelCompletePanel;

        private bool isTransitioning;
        private bool isLoadingScene;
        private bool isLevelComplete;

        public bool IsTransitioning => isTransitioning || isLoadingScene;
        public bool IsLevelComplete => isLevelComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple LevelManager instances found. The latest scene instance will be used.", this);
            }

            Instance = this;
        }

        private void Start()
        {
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(false);
            }

            UpdateLevelText();
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

            StartCoroutine(CompleteLevelRoutine());
        }

        public void LoadNextLevel()
        {
            if (isLoadingScene)
            {
                return;
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (!CanLoadScene(nextSceneIndex))
            {
                Debug.Log("No next scene is configured in Build Settings yet.", this);
                return;
            }

            StartCoroutine(LoadSceneRoutine(nextSceneIndex, 0f));
        }

        private IEnumerator CompleteLevelRoutine()
        {
            isTransitioning = true;
            isLevelComplete = true;

            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }

            if (completeDelay > 0f)
            {
                yield return new WaitForSeconds(completeDelay);
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (loadNextSceneWhenAvailable && CanLoadScene(nextSceneIndex))
            {
                isLoadingScene = true;
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("Level complete. Add another scene to Build Settings to continue automatically.", this);
            }
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
    }
}
