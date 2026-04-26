using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundMusicBootstrap : MonoBehaviour
{
    private const string MusicObjectName = "BackgroundMusic";
    private const string ResourceClipName = "mainmusic_v2";

    private static BackgroundMusicBootstrap instance;

    private AudioSource musicSource;

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

        AudioClip clip = Resources.Load<AudioClip>(ResourceClipName);

        if (clip == null)
        {
            Debug.LogWarning($"Background music clip '{ResourceClipName}' was not found in Resources.");
            return;
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.time = 0f;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 1f;
        musicSource.ignoreListenerPause = true;
        musicSource.ignoreListenerVolume = false;

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
        if (musicSource == null || musicSource.clip == null)
        {
            return;
        }

        musicSource.loop = true;

        if (PlayerData.SFXOn)
        {
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
    }
}
