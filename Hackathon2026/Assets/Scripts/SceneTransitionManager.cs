using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SceneTransitionManager : MonoBehaviour
{
    private const float FadeDuration = 0.35f;
    private const int OverlaySortOrder = 999;

    private static SceneTransitionManager instance;

    private Canvas overlayCanvas;
    private CanvasGroup overlayCanvasGroup;
    private Image overlayImage;
    private Coroutine transitionCoroutine;

    public static SceneTransitionManager Instance
    {
        get
        {
            EnsureInstance();
            return instance;
        }
    }

    public bool IsTransitioning { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindFirstObjectByType<SceneTransitionManager>();
        if (instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(nameof(SceneTransitionManager));
        instance = managerObject.AddComponent<SceneTransitionManager>();
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
        CreateOverlay();
        SetOverlayAlpha(0f);
        SetRaycastBlocking(false);
    }

    public void LoadScene(string sceneName)
    {
        LoadSceneAfterAction(sceneName, null);
    }

    public void LoadSceneAfterAction(string sceneName, Action beforeLoadAction)
    {
        if (IsTransitioning)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneTransitionManager received an empty scene name.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"SceneTransitionManager cannot load scene '{sceneName}' because it is not in Build Settings.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && string.Equals(activeScene.name, sceneName, StringComparison.Ordinal))
        {
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionRoutine(sceneName, beforeLoadAction));
    }

    private IEnumerator TransitionRoutine(string sceneName, Action beforeLoadAction)
    {
        IsTransitioning = true;
        SetRaycastBlocking(true);

        yield return FadeTo(1f);

        beforeLoadAction?.Invoke();

        AsyncOperation loadOperation = null;
        string loadErrorMessage = null;
        try
        {
            loadOperation = SceneManager.LoadSceneAsync(sceneName);
        }
        catch (Exception exception)
        {
            loadErrorMessage = exception.Message;
        }

        if (loadOperation == null)
        {
            if (!string.IsNullOrEmpty(loadErrorMessage))
            {
                Debug.LogError($"SceneTransitionManager failed to start loading scene '{sceneName}': {loadErrorMessage}");
            }
            else
            {
                Debug.LogError($"SceneTransitionManager failed to create a load operation for scene '{sceneName}'.");
            }

            yield return FadeTo(0f);
            SetRaycastBlocking(false);
            IsTransitioning = false;
            transitionCoroutine = null;
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        yield return null;

        CreateOverlay();
        yield return FadeTo(0f);

        SetRaycastBlocking(false);
        IsTransitioning = false;
        transitionCoroutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        CreateOverlay();

        float startAlpha = overlayCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / FadeDuration);
            SetOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);
    }

    private void CreateOverlay()
    {
        if (overlayCanvas != null && overlayCanvasGroup != null && overlayImage != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("SceneTransitionOverlay", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = OverlaySortOrder;

        overlayCanvasGroup = canvasObject.GetComponent<CanvasGroup>();
        overlayCanvasGroup.interactable = false;
        overlayCanvasGroup.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Fade", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        overlayImage = imageObject.GetComponent<Image>();
        overlayImage.color = Color.black;
        overlayImage.raycastTarget = true;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayCanvasGroup == null)
        {
            return;
        }

        overlayCanvasGroup.alpha = Mathf.Clamp01(alpha);
        overlayCanvasGroup.interactable = false;
    }

    private void SetRaycastBlocking(bool shouldBlock)
    {
        if (overlayCanvasGroup == null)
        {
            return;
        }

        overlayCanvasGroup.blocksRaycasts = shouldBlock;
        overlayCanvasGroup.interactable = false;
    }
}
