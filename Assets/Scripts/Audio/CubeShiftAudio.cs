using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeShift.Audio
{
    /// <summary>
    /// Provides lightweight procedural music and effects so every scene has audio without extra imported assets.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class CubeShiftAudio : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float MusicLoopSeconds = 12f;
        private const float FallEffectSeconds = 0.55f;

        private static CubeShiftAudio instance;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.75f;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.85f;

        private AudioSource musicSource;
        private AudioSource effectsSource;
        private AudioListener fallbackListener;
        private float nextMusicCheckTime;

        public static CubeShiftAudio Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

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
            CreateSources();
            EnsureAudibleListener();
            StartMusic();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public void PlayFall()
        {
            if (effectsSource == null)
            {
                CreateSources();
            }

            EnsureAudibleListener();
            effectsSource.PlayOneShot(CreateFallClip(), effectsVolume);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextMusicCheckTime)
            {
                return;
            }

            nextMusicCheckTime = Time.unscaledTime + 1f;
            EnsureAudibleListener();
            StartMusic();
        }

        private void CreateSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
                musicSource.spatialBlend = 0f;
            }

            if (effectsSource == null)
            {
                effectsSource = gameObject.AddComponent<AudioSource>();
                effectsSource.loop = false;
                effectsSource.playOnAwake = false;
                effectsSource.spatialBlend = 0f;
            }
        }

        private void StartMusic()
        {
            if (musicSource == null)
            {
                CreateSources();
            }

            if (musicSource.isPlaying)
            {
                return;
            }

            if (musicSource.clip == null)
            {
                musicSource.clip = CreateMusicLoop();
            }

            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudibleListener();
            StartMusic();
        }

        private void EnsureAudibleListener()
        {
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
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

        private static AudioClip CreateMusicLoop()
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * MusicLoopSeconds);
            float[] samples = new float[sampleCount];
            int[] notes = { 0, 3, 7, 10, 7, 3, 5, 8, 12, 8, 5, 3 };
            float baseFrequency = 196f;

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)SampleRate;
                float beat = time * 2f;
                int noteIndex = Mathf.FloorToInt(beat) % notes.Length;
                float noteProgress = beat - Mathf.Floor(beat);
                float frequency = baseFrequency * Mathf.Pow(2f, notes[noteIndex] / 12f);
                float padFrequency = frequency * 0.5f;
                float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Min(noteProgress * 5f, 1f))
                    * (1f - Mathf.SmoothStep(0.72f, 1f, noteProgress));

                float lead = Mathf.Sign(Mathf.Sin(Mathf.PI * 2f * frequency * time)) * envelope * 0.22f;
                float pad = Mathf.Sin(Mathf.PI * 2f * padFrequency * time) * 0.18f;
                float shimmer = Mathf.Sin(Mathf.PI * 2f * frequency * 2f * time) * envelope * 0.06f;

                samples[index] = (lead + pad + shimmer) * 0.7f;
            }

            AudioClip clip = AudioClip.Create("Cube Shift Music Loop", sampleCount, 1, SampleRate, false);
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
                float tone = Mathf.Sin(Mathf.PI * 2f * (frequency + wobble * frequency) * time);

                samples[index] = tone * envelope * 0.75f;
            }

            AudioClip clip = AudioClip.Create("Cube Fall", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
