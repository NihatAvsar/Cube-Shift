using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace CubeShift.Audio
{
    /// <summary>
    /// Persistent audio hub for music state changes, saved volume settings, mute, and UI/gameplay SFX.
    /// Existing gameplay calls through CubeShiftAudio.Instance remain supported.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class CubeShiftAudio : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float ProceduralLoopSeconds = 16f;
        private const float FallEffectSeconds = 0.55f;
        private const string MusicVolumeKey = "CubeShift.Audio.MusicVolume";
        private const string SfxVolumeKey = "CubeShift.Audio.SfxVolume";
        private const string MutedKey = "CubeShift.Audio.Muted";

        private static CubeShiftAudio instance;

        [Header("Mixer (Optional)")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private string musicVolumeParameter = "MusicVolume";
        [SerializeField] private string sfxVolumeParameter = "SFXVolume";

        [Header("Music Clips")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip levelSelectMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField, Min(0.05f)] private float crossfadeSeconds = 1.25f;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip uiBackClip;
        [SerializeField] private AudioClip uiSelectClip;
        [SerializeField] private AudioClip fallClip;

        [Header("Defaults")]
        [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.72f;
        [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.86f;

        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private AudioSource activeMusicSource;
        private AudioSource sfxSource;
        private AudioListener fallbackListener;
        private Coroutine crossfadeRoutine;
        private MusicState currentMusicState = MusicState.MainMenu;
        private float musicVolume;
        private float sfxVolume;
        private bool muted;

        public static CubeShiftAudio Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;
        public bool Muted => muted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            CubeShiftAudio existing = FindAnyObjectByType<CubeShiftAudio>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            GameObject audioObject = new GameObject("CubeShiftAudio");
            instance = audioObject.AddComponent<CubeShiftAudio>();
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
            LoadSettings();
            CreateSources();
            EnsureAudibleListener();
            ApplyVolumes();
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
            PlayMusicForScene(SceneManager.GetActiveScene());
        }

        public void PlayMusicState(MusicState state, bool instant = false)
        {
            currentMusicState = state;
            AudioClip clip = GetClipForState(state);
            if (clip == null)
            {
                clip = CreateProceduralMusicLoop(state);
            }

            CrossfadeTo(clip, instant ? 0f : crossfadeSeconds);
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            SaveSettings();
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            SaveSettings();
            ApplyVolumes();
        }

        public void SetMuted(bool value)
        {
            muted = value;
            SaveSettings();
            ApplyVolumes();
        }

        public void ToggleMute()
        {
            SetMuted(!muted);
        }

        public void PlayUIClick()
        {
            PlaySfx(uiClickClip != null ? uiClickClip : CreateToneClip("UI Click", 680f, 0.08f, 0.18f));
        }

        public void PlayUIBack()
        {
            PlaySfx(uiBackClip != null ? uiBackClip : CreateToneClip("UI Back", 360f, 0.11f, 0.16f));
        }

        public void PlayUISelect()
        {
            PlaySfx(uiSelectClip != null ? uiSelectClip : CreateToneClip("UI Select", 920f, 0.1f, 0.14f));
        }

        public void PlayFall()
        {
            PlaySfx(fallClip != null ? fallClip : CreateFallClip());
        }

        public void PlayRoll()
        {
            PlaySfx(CreateRollClip());
        }

        public void PlayWin()
        {
            PlaySfx(CreateWinClip());
        }

        private static AudioClip CreateRollClip()
        {
            float duration = 0.08f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float progress = index / (float)sampleCount;
                float envelope = Mathf.Sin(progress * Mathf.PI) * (1f - progress);
                float time = index / (float)SampleRate;
                float freq = Mathf.Lerp(120f, 60f, progress);
                samples[index] = Mathf.Sin(Mathf.PI * 2f * freq * time) * envelope * 0.55f;
            }

            AudioClip clip = AudioClip.Create("Cube Roll", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateWinClip()
        {
            float duration = 0.8f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];
            float[] frequencies = new float[] { 261.63f, 329.63f, 392.00f, 523.25f };
            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)SampleRate;
                float sum = 0f;
                for (int n = 0; n < frequencies.Length; n++)
                {
                    float noteStartTime = n * 0.12f;
                    if (time >= noteStartTime)
                    {
                        float noteTime = time - noteStartTime;
                        float noteEnvelope = Mathf.Max(0f, 1f - noteTime * 2.2f);
                        sum += Mathf.Sin(Mathf.PI * 2f * frequencies[n] * noteTime) * noteEnvelope * 0.25f;
                    }
                }
                samples[index] = sum;
            }

            AudioClip clip = AudioClip.Create("Victory Fanfare", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || muted)
            {
                return;
            }

            CreateSources();
            EnsureAudibleListener();
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * sfxVolume);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudibleListener();
            PlayMusicForScene(scene);
        }

        private void PlayMusicForScene(Scene scene)
        {
            if (scene.name.StartsWith("Level_"))
            {
                PlayMusicState(MusicState.Gameplay);
                return;
            }

            PlayMusicState(MusicState.MainMenu);
        }

        private void CreateSources()
        {
            if (musicSourceA == null)
            {
                musicSourceA = gameObject.AddComponent<AudioSource>();
                ConfigureMusicSource(musicSourceA);
            }

            if (musicSourceB == null)
            {
                musicSourceB = gameObject.AddComponent<AudioSource>();
                ConfigureMusicSource(musicSourceB);
            }

            if (activeMusicSource == null)
            {
                activeMusicSource = musicSourceA;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        private void ConfigureMusicSource(AudioSource source)
        {
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = musicMixerGroup;
        }

        private void CrossfadeTo(AudioClip clip, float duration)
        {
            CreateSources();
            if (activeMusicSource.clip == clip && activeMusicSource.isPlaying)
            {
                activeMusicSource.volume = EffectiveMusicVolume();
                return;
            }

            if (crossfadeRoutine != null)
            {
                StopCoroutine(crossfadeRoutine);
            }

            crossfadeRoutine = StartCoroutine(CrossfadeRoutine(clip, duration));
        }

        private IEnumerator CrossfadeRoutine(AudioClip nextClip, float duration)
        {
            AudioSource from = activeMusicSource;
            AudioSource to = activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;

            to.clip = nextClip;
            to.time = 0f;
            to.volume = 0f;
            to.Play();

            float startFromVolume = from.isPlaying ? from.volume : 0f;
            float targetVolume = EffectiveMusicVolume();
            if (duration <= 0f)
            {
                from.Stop();
                from.volume = 0f;
                to.volume = targetVolume;
                activeMusicSource = to;
                crossfadeRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                from.volume = Mathf.Lerp(startFromVolume, 0f, eased);
                to.volume = Mathf.Lerp(0f, targetVolume, eased);
                yield return null;
            }

            from.Stop();
            from.volume = 0f;
            to.volume = targetVolume;
            activeMusicSource = to;
            crossfadeRoutine = null;
        }

        private AudioClip GetClipForState(MusicState state)
        {
            return state switch
            {
                MusicState.MainMenu => mainMenuMusic,
                MusicState.LevelSelect => levelSelectMusic != null ? levelSelectMusic : mainMenuMusic,
                MusicState.Gameplay => gameplayMusic,
                _ => mainMenuMusic
            };
        }

        private void LoadSettings()
        {
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
            muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplyVolumes()
        {
            float music = EffectiveMusicVolume();
            if (activeMusicSource != null)
            {
                activeMusicSource.volume = music;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = muted ? 0f : sfxVolume;
            }

            SetMixerVolume(musicVolumeParameter, musicVolume);
            SetMixerVolume(sfxVolumeParameter, sfxVolume);
        }

        private float EffectiveMusicVolume()
        {
            return muted ? 0f : musicVolume;
        }

        private void SetMixerVolume(string parameter, float linearVolume)
        {
            if (audioMixer == null || string.IsNullOrWhiteSpace(parameter))
            {
                return;
            }

            float value = muted ? 0.0001f : Mathf.Clamp(linearVolume, 0.0001f, 1f);
            audioMixer.SetFloat(parameter, Mathf.Log10(value) * 20f);
        }

        private void EnsureAudibleListener()
        {
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude);
            bool hasOtherActiveListener = false;
            foreach (AudioListener listener in listeners)
            {
                if (listener != null && listener != fallbackListener && listener.enabled && listener.gameObject.activeInHierarchy)
                {
                    hasOtherActiveListener = true;
                    break;
                }
            }

            if (fallbackListener == null)
            {
                fallbackListener = gameObject.AddComponent<AudioListener>();
            }

            fallbackListener.enabled = !hasOtherActiveListener;
        }

        private static AudioClip CreateProceduralMusicLoop(MusicState state)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * ProceduralLoopSeconds);
            float[] samples = new float[sampleCount];
            int[] mainNotes = state == MusicState.Gameplay
                ? new[] { 0, 2, 7, 9, 7, 2, -3, 2 }
                : new[] { 0, 4, 7, 11, 7, 4, 2, 9 };
            float baseFrequency = state == MusicState.Gameplay ? 164.81f : 196f;
            float pulseRate = state == MusicState.LevelSelect ? 1.35f : 1.05f;

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)SampleRate;
                float beat = time * pulseRate;
                int noteIndex = Mathf.FloorToInt(beat) % mainNotes.Length;
                float noteProgress = beat - Mathf.Floor(beat);
                float frequency = baseFrequency * Mathf.Pow(2f, mainNotes[noteIndex] / 12f);
                float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Min(noteProgress * 4f, 1f))
                    * (1f - Mathf.SmoothStep(0.62f, 1f, noteProgress));

                float pad = Mathf.Sin(Mathf.PI * 2f * frequency * 0.5f * time) * 0.18f;
                float glass = Mathf.Sin(Mathf.PI * 2f * frequency * 1.5f * time) * envelope * 0.07f;
                float pulse = Mathf.Sin(Mathf.PI * 2f * frequency * time) * envelope * 0.11f;
                samples[index] = (pad + glass + pulse) * 0.72f;
            }

            AudioClip clip = AudioClip.Create($"Cube Shift {state} Procedural Loop", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateToneClip(string name, float frequency, float seconds, float gain)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * seconds);
            float[] samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float progress = index / (float)sampleCount;
                float envelope = 1f - Mathf.SmoothStep(0.15f, 1f, progress);
                float time = index / (float)SampleRate;
                samples[index] = Mathf.Sin(Mathf.PI * 2f * frequency * time) * envelope * gain;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateFallClip()
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * FallEffectSeconds);
            float[] samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float progress = index / (float)sampleCount;
                float time = index / (float)SampleRate;
                float frequency = Mathf.Lerp(520f, 85f, progress);
                float envelope = 1f - Mathf.SmoothStep(0.15f, 1f, progress);
                float wobble = Mathf.Sin(Mathf.PI * 2f * 18f * time) * 0.08f;
                samples[index] = Mathf.Sin(Mathf.PI * 2f * (frequency + wobble * frequency) * time) * envelope * 0.75f;
            }

            AudioClip clip = AudioClip.Create("Cube Fall", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
