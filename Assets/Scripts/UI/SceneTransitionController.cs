using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CubeShift.UI
{
    public sealed class SceneTransitionController : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        [SerializeField, Min(0f)] private float fadeSeconds = 0.32f;
        [SerializeField] private Color fadeColor = new(0.01f, 0.015f, 0.025f, 1f);

        private bool loading;

        private void Awake()
        {
            if (fadeImage == null)
            {
                fadeImage = GetComponent<Image>();
            }

            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                fadeImage.raycastTarget = false;
            }
        }

        public void LoadScene(string sceneName)
        {
            if (loading || string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            loading = true;
            if (fadeImage != null)
            {
                fadeImage.raycastTarget = true;
                yield return Fade(0f, 1f);
            }

            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = fadeSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeSeconds);
                float alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, to);
        }
    }
}
