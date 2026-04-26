using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundMusicBootstrap : MonoBehaviour
{
    private const string MusicObjectName = "BackgroundMusic";
    private const string ResourceClipPath = "Audio/current_background_music";
    private const float LoopFadeOutDuration = 0.5f;
    private const float LoopFadeInDuration = 0.5f;

    private static BackgroundMusicBootstrap instance;

    private AudioSource musicSource;
    private float normalVolume = 1f;
    private bool isLoopFading;
    private Coroutine loopFadeCoroutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureBackgroundMusic()
    {
        if (instance != null)
        {
            instance.ApplyPlaybackState();
            return;
        }

        BackgroundMusicBootstrap existingInstance = FindFirstObjectByType<BackgroundMusicBootstrap>();

        if (existingInstance != null)
        {
            instance = existingInstance;
            instance.Initialize();
            instance.ApplyPlaybackState();
            return;
        }

        GameObject musicObject = new GameObject(MusicObjectName);
        instance = musicObject.AddComponent<BackgroundMusicBootstrap>();
        instance.Initialize();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!CanManageMusicLoop() || isLoopFading)
        {
            return;
        }

        if (!PlayerData.SFXOn)
        {
            return;
        }

        float clipLength = musicSource.clip.length;
        if (clipLength <= 0f)
        {
            return;
        }

        if (clipLength < 1.5f)
        {
            if (musicSource.isPlaying && musicSource.time >= Mathf.Max(clipLength - 0.05f, 0f))
            {
                RestartClipImmediately();
            }

            return;
        }

        if (!musicSource.isPlaying)
        {
            return;
        }

        float fadeStartTime = clipLength - LoopFadeOutDuration;
        if (musicSource.time >= fadeStartTime)
        {
            loopFadeCoroutine = StartCoroutine(LoopWithFadeCoroutine());
        }
    }

    private void Initialize()
    {
        if (instance != null && instance != this)
        {
            return;
        }

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        gameObject.name = MusicObjectName;
        DontDestroyOnLoad(gameObject);

        AudioClip clip = Resources.Load<AudioClip>(ResourceClipPath);

        if (clip == null)
        {
            Debug.LogWarning($"Background music clip '{ResourceClipPath}' was not found in Resources.");
            return;
        }

        Debug.Log(
            $"[BackgroundMusicBootstrap] Loading Resources clip path='{ResourceClipPath}', " +
            $"name='{clip.name}', length={clip.length:F3}, samples={clip.samples}, " +
            $"frequency={clip.frequency}, instanceID={clip.GetInstanceID()}");

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.time = 0f;
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = normalVolume;
        musicSource.ignoreListenerPause = true;
        musicSource.ignoreListenerVolume = false;
        musicSource.enabled = true;

        Debug.Log(
            $"[BackgroundMusicBootstrap] musicSource gameObject='{musicSource.gameObject.name}', " +
            $"clip='{musicSource.clip?.name}', clipLength={musicSource.clip?.length ?? 0f:F3}");

        musicSource.Play();

        StopAllCoroutines();
        StartCoroutine(LogAudioSourcesAfterDelay());

        ApplyPlaybackState();
    }

    public static void ToggleMusic()
    {
        if (instance == null)
        {
            EnsureBackgroundMusic();
        }

        if (instance == null)
        {
            Debug.LogError("BackgroundMusicBootstrap could not find or create a music instance.");
            return;
        }

        instance.ApplyPlaybackState();
    }

    private void ApplyPlaybackState()
    {
        if (!CanManageMusicLoop())
        {
            return;
        }

        musicSource.loop = false;

        if (PlayerData.SFXOn)
        {
            if (loopFadeCoroutine != null)
            {
                StopCoroutine(loopFadeCoroutine);
                loopFadeCoroutine = null;
            }

            isLoopFading = false;
            musicSource.loop = false;

            if (!musicSource.isPlaying)
            {
                musicSource.volume = normalVolume;
                musicSource.time = 0f;
                musicSource.Play();
            }
        }
        else
        {
            if (loopFadeCoroutine != null)
            {
                StopCoroutine(loopFadeCoroutine);
                loopFadeCoroutine = null;
            }

            isLoopFading = false;
            musicSource.volume = 0f;
            musicSource.Stop();
        }
    }

    private bool CanManageMusicLoop()
    {
        return musicSource != null && musicSource.clip != null;
    }

    private void RestartClipImmediately()
    {
        if (!CanManageMusicLoop() || !PlayerData.SFXOn)
        {
            return;
        }

        musicSource.volume = normalVolume;
        musicSource.time = 0f;
        musicSource.Play();
    }

    private IEnumerator LoopWithFadeCoroutine()
    {
        if (!CanManageMusicLoop() || !PlayerData.SFXOn)
        {
            yield break;
        }

        isLoopFading = true;

        yield return FadeVolume(normalVolume, 0f, LoopFadeOutDuration);

        if (!CanManageMusicLoop() || !PlayerData.SFXOn)
        {
            isLoopFading = false;
            loopFadeCoroutine = null;
            yield break;
        }

        musicSource.time = 0f;
        musicSource.Play();

        yield return FadeVolume(0f, normalVolume, LoopFadeInDuration);

        isLoopFading = false;
        loopFadeCoroutine = null;
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (!CanManageMusicLoop())
        {
            yield break;
        }

        if (duration <= 0f)
        {
            musicSource.volume = to;
            yield break;
        }

        musicSource.volume = from;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!CanManageMusicLoop())
            {
                yield break;
            }

            if (!PlayerData.SFXOn)
            {
                musicSource.volume = 0f;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicSource.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }

        musicSource.volume = to;
    }

    private IEnumerator LogAudioSourcesAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2f);

        AudioSource[] sources = FindObjectsByType<AudioSource>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (AudioSource source in sources)
        {
            AudioClip sourceClip = source.clip;
            Scene scene = source.gameObject.scene;
            string sceneName = string.IsNullOrEmpty(scene.name) ? "<no scene>" : scene.name;

            Debug.Log(
                $"[BackgroundMusicBootstrap] Runtime AudioSource gameObject='{source.gameObject.name}', " +
                $"clip='{sourceClip?.name ?? "<null>"}', clipLength={(sourceClip != null ? sourceClip.length : 0f):F3}, " +
                $"isPlaying={source.isPlaying}, loop={source.loop}, scene='{sceneName}'");
        }
    }
}
