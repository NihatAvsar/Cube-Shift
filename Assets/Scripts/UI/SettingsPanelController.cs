using CubeShift.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CubeShift.UI
{
    public sealed class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private TMP_Text musicValueText;
        [SerializeField] private TMP_Text sfxValueText;
        [SerializeField] private float animationSpeed = 12f;

        private RectTransform rectTransform;
        private bool visible;
        private float targetAlpha;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            BindControls();
            SetVisible(false, true);
        }

        private void Update()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * animationSpeed);
            float scale = Mathf.Lerp(0.94f, 1f, canvasGroup.alpha);
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * scale;
            }
        }

        public void Configure(Slider music, Slider sfx, Toggle mute, TMP_Text musicValue, TMP_Text sfxValue)
        {
            musicSlider = music;
            sfxSlider = sfx;
            muteToggle = mute;
            musicValueText = musicValue;
            sfxValueText = sfxValue;
            BindControls();
            RefreshFromAudio();
        }

        public void Show()
        {
            RefreshFromAudio();
            SetVisible(true, true);
        }

        public void Hide()
        {
            SetVisible(false, true);
        }

        public void Toggle()
        {
            SetVisible(!visible, false);
        }

        private void BindControls()
        {
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveListener(HandleMusicChanged);
                musicSlider.onValueChanged.AddListener(HandleMusicChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);
                sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
            }

            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveListener(HandleMuteChanged);
                muteToggle.onValueChanged.AddListener(HandleMuteChanged);
            }
        }

        private void RefreshFromAudio()
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

            UpdateLabels();
        }

        private void HandleMusicChanged(float value)
        {
            CubeShiftAudio.Instance.SetMusicVolume(value);
            UpdateLabels();
        }

        private void HandleSfxChanged(float value)
        {
            CubeShiftAudio.Instance.SetSfxVolume(value);
            CubeShiftAudio.Instance.PlayUISelect();
            UpdateLabels();
        }

        private void HandleMuteChanged(bool value)
        {
            CubeShiftAudio.Instance.SetMuted(value);
            UpdateLabels();
        }

        private void UpdateLabels()
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

        private void SetVisible(bool value, bool instant)
        {
            visible = value;
            targetAlpha = value ? 1f : 0f;
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = value;
                canvasGroup.interactable = value;
                if (instant)
                {
                    canvasGroup.alpha = targetAlpha;
                }
            }

            if (!value && instant)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
