using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Renderer))]
public sealed class ShaderWorldBackgroundDriver : MonoBehaviour
{
    private static readonly int ITime = Shader.PropertyToID("_iTime");
    private static readonly int IResolution = Shader.PropertyToID("_iResolution");
    private static readonly int IMouse = Shader.PropertyToID("_iMouse");

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material sourceMaterial;

    private Material runtimeMaterial;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (sourceMaterial != null)
        {
            runtimeMaterial = Instantiate(sourceMaterial);
            runtimeMaterial.name = $"{sourceMaterial.name} (Runtime)";
            runtimeMaterial.hideFlags = HideFlags.DontSave;
            targetRenderer.sharedMaterial = runtimeMaterial;
        }
    }

    private void Update()
    {
        Material material = runtimeMaterial != null ? runtimeMaterial : targetRenderer.sharedMaterial;
        if (material == null)
        {
            return;
        }

        Vector2 resolution = new Vector2(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));
        Vector2 mousePosition = GetMousePosition();

        material.SetFloat(ITime, Time.time);
        material.SetVector(IResolution, new Vector4(resolution.x, resolution.y, 0f, 0f));
        material.SetVector(IMouse, new Vector4(mousePosition.x, mousePosition.y, IsMousePressed() ? 1f : 0f, 0f));
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }

    private static Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    private static bool IsMousePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }
}
