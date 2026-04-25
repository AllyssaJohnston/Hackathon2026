using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BitmapWaterDistortionBuilder
{
    private const string ShaderName = "Custom/ShaderToyBitmapWaterDistortion";
    private const string MaterialPath = "Assets/Materials/BitmapWaterDistortion_Mat.mat";
    private const string TextureFolder = "Assets/Textures";
    private const string PlaceholderTexturePath = "Assets/Textures/BitmapWaterPlaceholder.png";
    private const string TestScenePath = "Assets/Scenes/ShaderBitmapTest.unity";
    private const string PrefabPath = "Assets/Prefabs/BitmapShaderBackground.prefab";
    private const string StartMenuPath = "Assets/Scenes/StartMenu.unity";

    [MenuItem("Tools/Shader Background/Build Bitmap Water Distortion")]
    public static void Build()
    {
        ClearConsole();
        Directory.CreateDirectory(TextureFolder);
        Directory.CreateDirectory("Assets/Materials");

        string texturePath = FindOrCreateTexture();
        ConfigureTextureImporter(texturePath);
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        Material material = CreateOrUpdateMaterial(texture);

        CreateTestScene(texture, material);
        CreatePrefab(texture, material);
        AddToStartMenu(texture, material);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ClearConsole();
    }

    [MenuItem("Tools/Shader Background/Test StartMenu Play Button")]
    public static void TestStartMenuPlayButton()
    {
        Button playButton = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
            .FirstOrDefault(button => button.name == "PlayButton");

        if (playButton == null)
        {
            throw new MissingReferenceException("PlayButton was not found.");
        }

        playButton.onClick.Invoke();
    }

    private static string FindOrCreateTexture()
    {
        string existing = Directory.EnumerateFiles(TextureFolder, "*.*", SearchOption.TopDirectoryOnly)
            .Where(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path)
            .Select(path => path.Replace('\\', '/'))
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(existing))
        {
            return existing;
        }

        Texture2D texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float u = x / (float)(texture.width - 1);
                float v = y / (float)(texture.height - 1);
                float wave = 0.5f + 0.5f * Mathf.Sin((u * 18f) + Mathf.Cos(v * 11f) * 2f);
                Color deep = new Color(0.05f, 0.28f, 0.48f, 1f);
                Color bright = new Color(0.15f, 0.78f, 0.85f, 1f);
                texture.SetPixel(x, y, Color.Lerp(deep, bright, wave * 0.65f + v * 0.35f));
            }
        }

        texture.Apply();
        File.WriteAllBytes(PlaceholderTexturePath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(PlaceholderTexturePath, ImportAssetOptions.ForceUpdate);
        return PlaceholderTexturePath;
    }

    private static void ConfigureTextureImporter(string texturePath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
        {
            name = "Standalone",
            overridden = true,
            maxTextureSize = 2048,
            format = TextureImporterFormat.RGBA32,
            textureCompression = TextureImporterCompression.Uncompressed,
            compressionQuality = 100
        });
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private static Material CreateOrUpdateMaterial(Texture2D texture)
    {
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
                name = "BitmapWaterDistortion_Mat"
            };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        material.shader = shader;
        material.mainTexture = texture;
        material.SetTexture("_MainTex", texture);
        material.SetVector("_iResolution", new Vector4(1920f, 1080f, 0f, 0f));
        material.SetFloat("_iTime", 0f);
        material.SetFloat("_DistortionStrength", 0.015f);
        material.SetFloat("_LightStrength", 0.35f);
        material.SetFloat("_FlowSpeed", 0.18f);
        material.SetFloat("_TextureTiling", 1f);
        material.SetFloat("_NormalStrength", 0.35f);
        material.SetFloat("_WaveScale", 1.5f);
        material.SetFloat("_RippleStrength", 1f);
        material.SetFloat("_DebugRipples", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateTestScene(Texture2D texture, Material material)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ShaderBitmapTest";

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";

        GameObject canvasObject = CreateCanvas("Canvas");
        GameObject preview = CreateBitmapRawImage("BitmapShaderPreview", texture, material);
        preview.transform.SetParent(canvasObject.transform, false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, TestScenePath);
    }

    private static void CreatePrefab(Texture2D texture, Material material)
    {
        GameObject prefabRoot = CreateBitmapRawImage("BitmapShaderBackground", texture, material);
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        UnityEngine.Object.DestroyImmediate(prefabRoot);
    }

    private static void AddToStartMenu(Texture2D texture, Material material)
    {
        Scene scene = EditorSceneManager.OpenScene(StartMenuPath, OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .FirstOrDefault(item => item.name == "StartMenuCanvas");

        if (canvas == null)
        {
            throw new MissingReferenceException("StartMenuCanvas was not found.");
        }

        EnsureCanvasSettings(canvas);

        Transform oldSunRays = canvas.transform.Find("ShaderBackground");
        if (oldSunRays != null)
        {
            oldSunRays.gameObject.SetActive(false);
        }

        Transform existing = canvas.transform.Find("BitmapShaderBackground");
        GameObject background = existing != null ? existing.gameObject : null;
        if (background == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            background = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform)
                : CreateBitmapRawImage("BitmapShaderBackground", texture, material);
            background.name = "BitmapShaderBackground";
            background.transform.SetParent(canvas.transform, false);
        }

        ConfigureBitmapRawImage(background, texture, material);
        background.transform.SetAsFirstSibling();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        EnsureCanvasSettings(canvas);
        return canvasObject;
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

    private static GameObject CreateBitmapRawImage(string name, Texture2D texture, Material material)
    {
        GameObject rawImageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(ShaderToyDriver));
        ConfigureBitmapRawImage(rawImageObject, texture, material);
        return rawImageObject;
    }

    private static void ConfigureBitmapRawImage(GameObject rawImageObject, Texture2D texture, Material material)
    {
        RectTransform rectTransform = rawImageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        RawImage rawImage = rawImageObject.GetComponent<RawImage>();
        rawImage.texture = texture;
        rawImage.material = material;
        rawImage.color = Color.white;
        rawImage.raycastTarget = false;

        ShaderToyDriver driver = rawImageObject.GetComponent<ShaderToyDriver>();
        SerializedObject serializedDriver = new SerializedObject(driver);
        serializedDriver.FindProperty("shaderPreview").objectReferenceValue = rawImage;
        serializedDriver.FindProperty("shaderMaterial").objectReferenceValue = material;
        serializedDriver.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ClearConsole()
    {
        Type logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
        MethodInfo clearMethod = logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);
    }
}
