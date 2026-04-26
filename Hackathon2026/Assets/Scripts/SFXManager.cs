using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class SFXManager : MonoBehaviour
{
    private const string SfxObjectName = "SFXManager";
    private const string BlockPressPath = "SFX/block_press";
    private const string UiPressPath = "SFX/ui_press";
    private const string BubbleTransitionPath = "SFX/bubble_transition";

    private static SFXManager instance;

    private AudioSource sfxSource;
    private AudioSource bubbleTransitionSource;
    private AudioClip blockPressClip;
    private AudioClip uiPressClip;
    private AudioClip bubbleTransitionClip;
    private bool warnedMissingBlockPress;
    private bool warnedMissingUiPress;
    private bool warnedMissingBubbleTransition;
    private Coroutine bubbleTransitionFadeCoroutine;

    public static SFXManager Instance
    {
        get
        {
            EnsureInstance();
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            instance.Initialize();
            return;
        }

        SFXManager existingInstance = FindFirstObjectByType<SFXManager>();
        if (existingInstance != null)
        {
            instance = existingInstance;
            instance.Initialize();
            return;
        }

        GameObject sfxObject = new GameObject(SfxObjectName);
        instance = sfxObject.AddComponent<SFXManager>();
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

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }

        if (bubbleTransitionSource == null)
        {
            bubbleTransitionSource = gameObject.AddComponent<AudioSource>();
        }

        gameObject.name = SfxObjectName;
        DontDestroyOnLoad(gameObject);

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 1f;

        bubbleTransitionSource.playOnAwake = false;
        bubbleTransitionSource.loop = false;
        bubbleTransitionSource.spatialBlend = 0f;
        bubbleTransitionSource.volume = 1f;

        if (blockPressClip == null)
        {
            blockPressClip = Resources.Load<AudioClip>(BlockPressPath);
        }

        if (uiPressClip == null)
        {
            uiPressClip = Resources.Load<AudioClip>(UiPressPath);
        }

        if (bubbleTransitionClip == null)
        {
            bubbleTransitionClip = Resources.Load<AudioClip>(BubbleTransitionPath);
        }
    }

    public void PlayBlockPress()
    {
        PlayClip(blockPressClip, BlockPressPath, ref warnedMissingBlockPress);
    }

    public void PlayUIPress()
    {
        PlayClip(uiPressClip, UiPressPath, ref warnedMissingUiPress);
    }

    public void PlayBubbleTransition(float stopAfterSeconds, float fadeDurationSeconds = 0.5f)
    {
        Initialize();

        if (!PlayerData.SFXOn)
        {
            return;
        }

        if (bubbleTransitionClip == null)
        {
            if (!warnedMissingBubbleTransition)
            {
                warnedMissingBubbleTransition = true;
                Debug.LogWarning($"SFXManager could not load AudioClip at Resources path '{BubbleTransitionPath}'.");
            }

            return;
        }

        if (bubbleTransitionFadeCoroutine != null)
        {
            StopCoroutine(bubbleTransitionFadeCoroutine);
            bubbleTransitionFadeCoroutine = null;
        }

        bubbleTransitionSource.Stop();
        bubbleTransitionSource.volume = 1f;
        bubbleTransitionSource.PlayOneShot(bubbleTransitionClip);
        bubbleTransitionFadeCoroutine = StartCoroutine(FadeOutBubbleTransitionAfterDelay(stopAfterSeconds, fadeDurationSeconds));
    }

    private void PlayClip(AudioClip clip, string resourcePath, ref bool warnedMissingClip)
    {
        Initialize();

        if (!PlayerData.SFXOn)
        {
            return;
        }

        if (clip == null)
        {
            if (!warnedMissingClip)
            {
                warnedMissingClip = true;
                Debug.LogWarning($"SFXManager could not load AudioClip at Resources path '{resourcePath}'.");
            }

            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    private System.Collections.IEnumerator FadeOutBubbleTransitionAfterDelay(float stopAfterSeconds, float fadeDurationSeconds)
    {
        float safeFadeDuration = Mathf.Max(0.01f, fadeDurationSeconds);
        float fadeStartDelay = Mathf.Max(0f, stopAfterSeconds - safeFadeDuration);

        if (fadeStartDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(fadeStartDelay);
        }

        float startingVolume = bubbleTransitionSource.volume;
        float elapsed = 0f;

        while (elapsed < safeFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeFadeDuration);
            bubbleTransitionSource.volume = Mathf.Lerp(startingVolume, 0f, t);
            yield return null;
        }

        bubbleTransitionSource.Stop();
        bubbleTransitionSource.volume = 1f;
        bubbleTransitionFadeCoroutine = null;
    }
}
