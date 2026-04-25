using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public static class GameplayShaderBackgroundFixer
{
    private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
    private const string MaterialPath = "Assets/Materials/ShaderToySunRays_Mat.mat";

    [MenuItem("Tools/Shader Background/Fix Gameplay Background Layering")]
    public static void FixGameplayBackgroundLayering()
    {
        ClearConsole();

        Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            throw new MissingReferenceException($"Required material was not found at '{MaterialPath}'.");
        }

        GameObject overlayBackground = GameObject.Find("GameplayCanvas/ShaderBackground");
        if (overlayBackground != null)
        {
            overlayBackground.SetActive(false);
        }

        GameObject worldBackground = GameObject.Find("ShaderWorldBackground");
        if (worldBackground == null)
        {
            worldBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            worldBackground.name = "ShaderWorldBackground";
        }

        ConfigureWorldBackground(worldBackground, material);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        ClearConsole();
    }

    private static void ConfigureWorldBackground(GameObject worldBackground, Material material)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new MissingReferenceException("Main Camera was not found in Gameplay.");
        }

        float height = camera.orthographicSize * 2f;
        float width = height * camera.aspect;

        worldBackground.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 20f);
        worldBackground.transform.rotation = Quaternion.identity;
        worldBackground.transform.localScale = new Vector3(width, height, 1f);

        Collider collider = worldBackground.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }

        Renderer renderer = worldBackground.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.sortingOrder = -1000;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        ShaderWorldBackgroundDriver driver = worldBackground.GetComponent<ShaderWorldBackgroundDriver>();
        if (driver == null)
        {
            driver = worldBackground.AddComponent<ShaderWorldBackgroundDriver>();
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
