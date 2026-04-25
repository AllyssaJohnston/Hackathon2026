using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Reflection;

public static class ShaderTestSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/ShaderTest.unity";
    private const string MaterialPath = "Assets/Materials/ShaderToySunRays_Mat.mat";
    private const string ShaderName = "Custom/ShaderToySunRays";

    [MenuItem("Tools/Shader Test/Exit Play Mode")]
    public static void ExitPlayMode()
    {
        EditorApplication.isPlaying = false;
    }

    [MenuItem("Tools/Shader Test/Enter Play Mode")]
    public static void EnterPlayMode()
    {
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Tools/Shader Test/Rebuild ShaderTest Scene")]
    public static void RebuildScene()
    {
        ClearConsole();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ShaderTest";

        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            throw new MissingReferenceException($"Shader '{ShaderName}' was not found.");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader)
            {
                name = "ShaderToySunRays_Mat"
            };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        material.shader = shader;
        material.SetVector("_iResolution", new Vector4(1920f, 1080f, 0f, 0f));
        material.SetVector("_iMouse", Vector4.zero);
        material.SetFloat("_iTime", 0f);
        EditorUtility.SetDirty(material);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject previewObject = new GameObject("ShaderPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(ShaderToyDriver));
        previewObject.transform.SetParent(canvasObject.transform, false);

        RectTransform previewRect = previewObject.GetComponent<RectTransform>();
        previewRect.anchorMin = Vector2.zero;
        previewRect.anchorMax = Vector2.one;
        previewRect.offsetMin = Vector2.zero;
        previewRect.offsetMax = Vector2.zero;
        previewRect.pivot = new Vector2(0.5f, 0.5f);
        previewRect.anchoredPosition = Vector2.zero;
        previewRect.sizeDelta = Vector2.zero;

        RawImage preview = previewObject.GetComponent<RawImage>();
        preview.color = Color.white;
        preview.material = material;
        preview.raycastTarget = false;

        ShaderToyDriver driver = previewObject.GetComponent<ShaderToyDriver>();
        SerializedObject serializedDriver = new SerializedObject(driver);
        serializedDriver.FindProperty("shaderPreview").objectReferenceValue = preview;
        serializedDriver.FindProperty("shaderMaterial").objectReferenceValue = material;
        serializedDriver.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ClearConsole();
    }

    private static void ClearConsole()
    {
        Type logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
        MethodInfo clearMethod = logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);
    }
}
