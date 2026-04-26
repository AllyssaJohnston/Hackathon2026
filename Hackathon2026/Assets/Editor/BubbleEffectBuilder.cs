using System.IO;
using UnityEditor;
using UnityEngine;

public static class BubbleEffectBuilder
{
    private const string TexturePath = "Assets/Sprites/bubble_flipbook.png";
    private const string MaterialPath = "Assets/Materials/BubbleFlipbook.mat";
    private const string PrefabPath = "Assets/Prefabs/BubbleParticleEffect.prefab";

    [InitializeOnLoadMethod]
    private static void BuildOnceAfterReload()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
        {
            return;
        }

        EditorApplication.delayCall += Build;
    }

    [MenuItem("Tools/Build Bubble Particle Effect")]
    public static void Build()
    {
        ConfigureTextureImporter();
        Material material = CreateMaterial();
        CreatePrefab(material);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Built bubble particle effect prefab: {PrefabPath}");
    }

    private static void ConfigureTextureImporter()
    {
        var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"Missing bubble flipbook texture at {TexturePath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;

        TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
        defaultSettings.overridden = false;
        defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
        defaultSettings.maxTextureSize = 2048;
        importer.SetPlatformTextureSettings(defaultSettings);

        TextureImporterPlatformSettings standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
        standaloneSettings.overridden = false;
        standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
        standaloneSettings.maxTextureSize = 2048;
        importer.SetPlatformTextureSettings(standaloneSettings);

        importer.SaveAndReimport();
    }

    private static Material CreateMaterial()
    {
        Directory.CreateDirectory("Assets/Materials");

        Shader shader =
            Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
            Shader.Find("Particles/Standard Unlit") ??
            Shader.Find("Sprites/Default");

        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else
        {
            material.shader = shader;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        SetTextureIfPresent(material, "_BaseMap", texture);
        SetTextureIfPresent(material, "_MainTex", texture);
        SetColorIfPresent(material, "_BaseColor", Color.white);
        SetColorIfPresent(material, "_Color", Color.white);

        SetFloatIfPresent(material, "_Surface", 1f);
        SetFloatIfPresent(material, "_Blend", 0f);
        SetFloatIfPresent(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        SetFloatIfPresent(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        EditorUtility.SetDirty(material);

        return material;
    }

    private static void CreatePrefab(Material material)
    {
        Directory.CreateDirectory("Assets/Prefabs");

        var go = new GameObject("BubbleParticleEffect");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();

        var main = ps.main;
        main.duration = 2.0f;
        main.loop = true;
        main.prewarm = false;
        main.startDelay = 0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.05f, 1.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.22f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.46f);
        main.startRotation = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);
        main.startColor = new Color(1f, 1f, 1f, 0.95f);
        main.gravityModifier = -0.03f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.playOnAwake = true;
        main.maxParticles = 80;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 9f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.28f;
        shape.arc = 360f;
        shape.radiusThickness = 0.35f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.045f;
        noise.frequency = 0.45f;
        noise.scrollSpeed = 0.2f;
        noise.damping = true;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.08f),
                new GradientAlphaKey(1f, 0.72f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = new ParticleSystem.MinMaxGradient(gradient);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.75f),
            new Keyframe(0.18f, 1f),
            new Keyframe(0.78f, 1.08f),
            new Keyframe(1f, 0.62f));
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var textureSheet = ps.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.mode = ParticleSystemAnimationMode.Grid;
        textureSheet.numTilesX = 4;
        textureSheet.numTilesY = 4;
        textureSheet.animation = ParticleSystemAnimationType.WholeSheet;
        textureSheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        textureSheet.cycleCount = 1;
        textureSheet.startFrame = 0f;

        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = material;
        renderer.sortMode = ParticleSystemSortMode.None;
        renderer.minParticleSize = 0.02f;
        renderer.maxParticleSize = 1.0f;

        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
    }

    private static void SetTextureIfPresent(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property))
        {
            material.SetTexture(property, texture);
        }
    }

    private static void SetColorIfPresent(Material material, string property, Color color)
    {
        if (material.HasProperty(property))
        {
            material.SetColor(property, color);
        }
    }

    private static void SetFloatIfPresent(Material material, string property, float value)
    {
        if (material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }
}
