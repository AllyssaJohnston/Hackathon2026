using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class GameplayBitmapRippleBackgroundInstaller
{
    private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
    private const string BitmapMaterialPath = "Assets/Materials/BitmapWaterDistortion_Mat.mat";
    private const string BitmapTexturePath = "Assets/Textures/BitmapWaterPlaceholder.png";
    private const string BackgroundName = "BitmapRippleBackground";

    [MenuItem("Tools/Shader Background/Install Gameplay Bitmap Ripple Background")]
    public static void InstallGameplayBitmapRippleBackground()
    {
        ClearConsole();

        Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(BitmapMaterialPath);
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(BitmapTexturePath);

        if (material == null)
        {
            throw new MissingReferenceException($"Required material was not found at '{BitmapMaterialPath}'.");
        }

        if (texture == null)
        {
            throw new MissingReferenceException($"Required texture was not found at '{BitmapTexturePath}'.");
        }

        material.SetTexture("_MainTex", texture);

        DisableOldBackgrounds();

        GameObject bitmapBackground = GameObject.Find(BackgroundName);
        if (bitmapBackground == null)
        {
            bitmapBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bitmapBackground.name = BackgroundName;
        }

        bitmapBackground.SetActive(true);
        ConfigureWorldBackground(bitmapBackground, material);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        ClearConsole();
    }

    private static void DisableOldBackgrounds()
    {
        DisableIfFound("ShaderWorldBackground");
        DisableIfFound("SunRaysBackground");
        DisableIfFound("GameplayCanvas/ShaderBackground");
        DisableIfFound("GameplayCanvas/BitmapShaderBackground");
        DisableIfFound("GameplayCanvas/Background");
    }

    private static void DisableIfFound(string path)
    {
        GameObject background = GameObject.Find(path);
        if (background != null && background.name != BackgroundName)
        {
            background.SetActive(false);
        }
    }

    private static void ConfigureWorldBackground(GameObject background, Material material)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new MissingReferenceException("Main Camera was not found in Gameplay.");
        }

        float height = camera.orthographicSize * 2f;
        float width = height * camera.aspect;

        background.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 20f);
        background.transform.rotation = Quaternion.identity;
        background.transform.localScale = new Vector3(width, height, 1f);

        Collider collider = background.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }

        Renderer renderer = background.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.sortingOrder = -1000;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        ShaderWorldBackgroundDriver driver = background.GetComponent<ShaderWorldBackgroundDriver>();
        if (driver == null)
        {
            driver = background.AddComponent<ShaderWorldBackgroundDriver>();
        }

        SerializedObject serializedDriver = new SerializedObject(driver);
        serializedDriver.FindProperty("targetRenderer").objectReferenceValue = renderer;
        serializedDriver.FindProperty("sourceMaterial").objectReferenceValue = material;
        serializedDriver.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ClearConsole()
    {
        Type logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
        MethodInfo clearMethod = logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);
    }
}
