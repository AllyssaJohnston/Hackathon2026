using UnityEngine;

public sealed class BackgroundMusicBootstrap : MonoBehaviour
{
    private const string MusicObjectName = "BackgroundMusic";
    private const string ResourceClipName = "mainmusic";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBackgroundMusic()
    {
        AudioSource existingSource = Object.FindFirstObjectByType<AudioSource>();
        if (existingSource != null && existingSource.gameObject.name == MusicObjectName)
        {
            if (PlayerData.SFXOn)
            {
                if (!existingSource.isPlaying && existingSource.clip != null)
                {
                    Debug.Log("play");
                    existingSource.Play();
                }
            }
            else
            {
                Debug.Log("stop");
                existingSource.Stop();
            }

           return;
        }

        AudioClip clip = Resources.Load<AudioClip>(ResourceClipName);
        if (clip == null)
        {
            Debug.LogWarning($"Background music clip '{ResourceClipName}' was not found in Resources.");
            return;
        }

        GameObject musicObject = new GameObject(MusicObjectName);
        Object.DontDestroyOnLoad(musicObject);

        AudioSource audioSource = musicObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.ignoreListenerPause = true;
        audioSource.ignoreListenerVolume = false;
        audioSource.Play();

        musicObject.AddComponent<BackgroundMusicBootstrap>();
    }

    public static void ToggleMusic()
    {
        Debug.Log("called");
        AudioSource existingSource = Object.FindFirstObjectByType<AudioSource>();
        if (PlayerData.SFXOn)
        {
            existingSource.Play();
        }
        else
        {
            existingSource.Stop();
        }
    }
}
