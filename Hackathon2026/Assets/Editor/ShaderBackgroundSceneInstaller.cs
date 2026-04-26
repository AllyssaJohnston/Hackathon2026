using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShaderBackgroundSceneInstaller
{
    private const string PrefabPath = "Assets/Prefabs/ShaderBackground.prefab";
    private const string MaterialPath = "Assets/Materials/ShaderToySunRays_Mat.mat";

    private static readonly (string scenePath, string canvasName)[] TargetScenes =
    {
        ("Assets/Scenes/StartMenu.unity", "StartMenuCanvas"),
        ("Assets/Scenes/LevelSelect.unity", "LevelSelectCanvas")
    };

    [MenuItem("Tools/Shader Background/Create Prefab")]
    public static void CreatePrefab()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            throw new MissingReferenceException($"Required material was not found at '{MaterialPath}'.");
        }

        GameObject root = new GameObject("ShaderBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(ShaderBackgroundDriver));
        ConfigureBackground(root, material);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Shader Background/Install In Main UI Scenes")]
    public static void InstallInMainScenes()
    {
        ClearConsole();
        CreatePrefab();

        foreach ((string scenePath, string canvasName) in TargetScenes)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Canvas canvas = FindCanvas(canvasName);
            if (canvas == null)
            {
                throw new MissingReferenceException($"Canvas '{canvasName}' was not found in '{scenePath}'.");
            }

            EnsureCanvasSettings(canvas);
            GameObject background = EnsureBackground(canvas);
            background.transform.SetAsFirstSibling();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        GameplayShaderBackgroundFixer.FixGameplayBackgroundLayering();
        AssetDatabase.SaveAssets();
        ClearConsole();
    }

    private static GameObject EnsureBackground(Canvas canvas)
    {
        Transform existing = canvas.transform.Find("ShaderBackground");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

        if (existing != null)
        {
            ConfigureBackground(existing.gameObject, material);
            return existing.gameObject;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject instance = prefab != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform)
            : new GameObject("ShaderBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(ShaderBackgroundDriver));

        instance.name = "ShaderBackground";
        instance.transform.SetParent(canvas.transform, false);
        ConfigureBackground(instance, material);
        return instance;
    }

    private static void ConfigureBackground(GameObject background, Material material)
    {
        RectTransform rectTransform = background.GetComponent<RectTransform>();
        RawImage rawImage = background.GetComponent<RawImage>();
        ShaderBackgroundDriver driver = background.GetComponent<ShaderBackgroundDriver>();

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rawImage.material = material;
        rawImage.color = Color.white;
        rawImage.raycastTarget = false;

        SerializedObject serializedDriver = new SerializedObject(driver);
        serializedDriver.FindProperty("targetImage").objectReferenceValue = rawImage;
        serializedDriver.FindProperty("sourceMaterial").objectReferenceValue = material;
        serializedDriver.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureCanvasSettings(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static Canvas FindCanvas(string canvasName)
    {
        return UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .FirstOrDefault(canvas => canvas.name == canvasName);
    }

    private static void ClearConsole()
    {
        Type logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
        MethodInfo clearMethod = logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);
    }
}
