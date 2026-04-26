using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(RawImage))]
public sealed class BubbleOverlayController : MonoBehaviour
{
    private static readonly int BubbleTint = Shader.PropertyToID("_BubbleTint");
    private static readonly int BubbleOpacity = Shader.PropertyToID("_BubbleOpacity");
    private static readonly int BubbleDensity = Shader.PropertyToID("_BubbleDensity");
    private static readonly int AutoMove = Shader.PropertyToID("_AutoMove");
    private static readonly int RiseSpeed = Shader.PropertyToID("_RiseSpeed");
    private static readonly int MinBubbleSpeed = Shader.PropertyToID("_MinBubbleSpeed");
    private static readonly int MaxBubbleSpeed = Shader.PropertyToID("_MaxBubbleSpeed");
    private static readonly int ITime = Shader.PropertyToID("_iTime");
    private static readonly int BubbleTime = Shader.PropertyToID("_BubbleTime");
    private static readonly int BubbleScale = Shader.PropertyToID("_BubbleScale");

    private const float DefaultRiseSpeed = 1f;
    private const float DefaultMinBubbleSpeed = 0.8f;
    private const float DefaultMaxBubbleSpeed = 1.3f;

    [SerializeField] private RawImage overlayImage;
    [SerializeField] private ShaderToyDriver shaderToyDriver;

    [Header("Transition Defaults")]
    [SerializeField] private Color bubbleTint = Color.white;
    [SerializeField] [Range(0f, 1f)] private float bubbleDensity = 1f;
    [SerializeField] private float phaseDuration = 1f;
    [SerializeField] private float holdOpacity = 1f;
    [SerializeField] private float bubbleScale = 1f;
    [SerializeField] private float autoRiseSpeed = DefaultRiseSpeed;

    private Material runtimeMaterial;
    private Coroutine transitionCoroutine;
    private bool isTransitioning;

    private float NormalRiseSpeed => Mathf.Max(autoRiseSpeed, DefaultRiseSpeed);

    private void Awake()
    {
        if (overlayImage == null)
        {
            overlayImage = GetComponent<RawImage>();
        }

        if (shaderToyDriver == null)
        {
            shaderToyDriver = GetComponent<ShaderToyDriver>();
        }
    }

    private void Start()
    {
        if (overlayImage == null || overlayImage.material == null)
        {
            return;
        }

        runtimeMaterial = Instantiate(overlayImage.material);
        runtimeMaterial.name = $"{overlayImage.material.name} (Runtime)";
        overlayImage.material = runtimeMaterial;
        shaderToyDriver?.SetRuntimeMaterial(runtimeMaterial);
        overlayImage.raycastTarget = false;
        ApplyIdleState();
    }

    private void Update()
    {
        if (!WasTriggerPressed())
        {
            return;
        }

        PlayTransition("O key");
    }

    public void PlayTransition(string trigger = "Unknown")
    {
        if (runtimeMaterial == null)
        {
            Debug.LogWarning($"[BubbleOverlayController] PlayTransition requested by {trigger}, but runtime material is not ready.", this);
            return;
        }

        if (isTransitioning || transitionCoroutine != null)
        {
            Debug.Log($"[BubbleOverlayController] PlayTransition ignored for {trigger} because transition is already running.", this);
            return;
        }

        isTransitioning = true;
        ApplyTransitionDefaults();
        Debug.Log($"[BubbleOverlayController] PlayTransition started by {trigger}.", this);
        float transitionLength = Mathf.Max(0.01f, phaseDuration) * 3f;
        SFXManager.Instance.PlayBubbleTransition(transitionLength - 0.5f, 0.5f);
        transitionCoroutine = StartCoroutine(PlayTransitionCoroutine());
    }

    private IEnumerator PlayTransitionCoroutine()
    {
        float bubbleTime = 0f;
        runtimeMaterial.SetFloat(BubbleTime, bubbleTime);
        LogMaterialState("transition start");
        Debug.Log($"[BubbleOverlayController] Starting _RiseSpeed={runtimeMaterial.GetFloat(RiseSpeed):F3}, _BubbleTime={bubbleTime:F3}.", this);

        float duration = Mathf.Max(0.01f, phaseDuration);
        Debug.Log("[BubbleOverlayController] Phase 1 start.", this);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            bubbleTime += deltaTime;
            runtimeMaterial.SetFloat(BubbleTime, bubbleTime);
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            runtimeMaterial.SetFloat(BubbleDensity, bubbleDensity);
            runtimeMaterial.SetFloat(BubbleOpacity, Mathf.Lerp(0f, holdOpacity, t));
            runtimeMaterial.SetFloat(RiseSpeed, NormalRiseSpeed);
            yield return null;
        }

        runtimeMaterial.SetFloat(BubbleDensity, bubbleDensity);
        runtimeMaterial.SetFloat(BubbleOpacity, holdOpacity);
        runtimeMaterial.SetFloat(RiseSpeed, NormalRiseSpeed);

        Debug.Log("[BubbleOverlayController] Phase 2 start.", this);
        elapsed = 0f;
        while (elapsed < duration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            bubbleTime += deltaTime;
            runtimeMaterial.SetFloat(BubbleTime, bubbleTime);
            elapsed += deltaTime;
            runtimeMaterial.SetFloat(BubbleDensity, bubbleDensity);
            runtimeMaterial.SetFloat(BubbleOpacity, holdOpacity);
            runtimeMaterial.SetFloat(RiseSpeed, NormalRiseSpeed);
            yield return null;
        }

        Debug.Log("[BubbleOverlayController] Phase 3 start.", this);
        elapsed = 0f;
        while (elapsed < duration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            bubbleTime += deltaTime;
            runtimeMaterial.SetFloat(BubbleTime, bubbleTime);
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            runtimeMaterial.SetFloat(BubbleDensity, bubbleDensity);
            runtimeMaterial.SetFloat(BubbleOpacity, Mathf.Lerp(holdOpacity, 0f, t));
            runtimeMaterial.SetFloat(RiseSpeed, NormalRiseSpeed);
            yield return null;
        }

        Debug.Log($"[BubbleOverlayController] Ending _RiseSpeed={runtimeMaterial.GetFloat(RiseSpeed):F3}, _BubbleTime={bubbleTime:F3}.", this);
        ApplyIdleState();
        LogMaterialState("end");
        transitionCoroutine = null;
        isTransitioning = false;
        Debug.Log("[BubbleOverlayController] Transition complete. Running flag reset.", this);
    }

    private void ApplyTransitionDefaults()
    {
        runtimeMaterial.SetColor(BubbleTint, bubbleTint);
        runtimeMaterial.SetFloat(BubbleDensity, bubbleDensity);
        runtimeMaterial.SetFloat(BubbleTime, 0f);
        runtimeMaterial.SetFloat(BubbleScale, bubbleScale);
        runtimeMaterial.SetFloat(RiseSpeed, NormalRiseSpeed);
        runtimeMaterial.SetFloat(MinBubbleSpeed, DefaultMinBubbleSpeed);
        runtimeMaterial.SetFloat(MaxBubbleSpeed, DefaultMaxBubbleSpeed);
        runtimeMaterial.SetFloat(AutoMove, 1f);
        runtimeMaterial.SetFloat(BubbleOpacity, 0f);
    }

    private void ApplyIdleState()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.SetColor(BubbleTint, bubbleTint);
        runtimeMaterial.SetFloat(BubbleDensity, 0f);
        runtimeMaterial.SetFloat(BubbleTime, 0f);
        runtimeMaterial.SetFloat(BubbleScale, bubbleScale);
        runtimeMaterial.SetFloat(RiseSpeed, NormalRiseSpeed);
        runtimeMaterial.SetFloat(MinBubbleSpeed, DefaultMinBubbleSpeed);
        runtimeMaterial.SetFloat(MaxBubbleSpeed, DefaultMaxBubbleSpeed);
        runtimeMaterial.SetFloat(AutoMove, 1f);
        runtimeMaterial.SetFloat(BubbleOpacity, 0f);
    }

    private static bool WasTriggerPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.O);
#endif
    }

    private void LogMaterialState(string label)
    {
        if (runtimeMaterial == null || overlayImage == null)
        {
            return;
        }

        Canvas parentCanvas = overlayImage.canvas;
        int sortingOrder = parentCanvas != null ? parentCanvas.sortingOrder : int.MinValue;
        float minBubbleSpeed = runtimeMaterial.HasProperty(MinBubbleSpeed) ? runtimeMaterial.GetFloat(MinBubbleSpeed) : float.NaN;
        float maxBubbleSpeed = runtimeMaterial.HasProperty(MaxBubbleSpeed) ? runtimeMaterial.GetFloat(MaxBubbleSpeed) : float.NaN;
        float shaderTime = runtimeMaterial.HasProperty(ITime) ? runtimeMaterial.GetFloat(ITime) : float.NaN;
        float bubbleTime = runtimeMaterial.HasProperty(BubbleTime) ? runtimeMaterial.GetFloat(BubbleTime) : float.NaN;

        Debug.Log(
            $"[BubbleOverlayController] Material state ({label}): " +
            $"density={runtimeMaterial.GetFloat(BubbleDensity):F3}, " +
            $"opacity={runtimeMaterial.GetFloat(BubbleOpacity):F3}, " +
            $"riseSpeed={runtimeMaterial.GetFloat(RiseSpeed):F3}, " +
            $"minBubbleSpeed={minBubbleSpeed:F3}, " +
            $"maxBubbleSpeed={maxBubbleSpeed:F3}, " +
            $"iTime={shaderTime:F3}, " +
            $"bubbleTime={bubbleTime:F3}, " +
            $"rawImageEnabled={overlayImage.enabled}, " +
            $"rawImageAlpha={overlayImage.color.a:F3}, " +
            $"raycastTarget={overlayImage.raycastTarget}, " +
            $"canvasSortingOrder={sortingOrder}",
            this);
    }
}
