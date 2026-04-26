using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class ShaderToyDriver : MonoBehaviour
{
    private static readonly int ITime = Shader.PropertyToID("_iTime");
    private static readonly int IResolution = Shader.PropertyToID("_iResolution");
    private static readonly int IMouse = Shader.PropertyToID("_iMouse");

    [SerializeField] private RawImage shaderPreview;
    [SerializeField] private Material shaderMaterial;
    private Material runtimeMaterial;

    private void Awake()
    {
        if (shaderPreview == null)
        {
            shaderPreview = GetComponent<RawImage>();
        }

        if (shaderPreview != null && shaderPreview.material == null && shaderMaterial != null)
        {
            shaderPreview.material = shaderMaterial;
        }

        runtimeMaterial = shaderPreview != null ? shaderPreview.material : null;
    }

    public void SetRuntimeMaterial(Material material)
    {
        runtimeMaterial = material;

        if (shaderPreview != null && material != null)
        {
            shaderPreview.material = material;
        }
    }

    private void Update()
    {
        if (shaderPreview == null)
        {
            return;
        }

        Rect rect = shaderPreview.rectTransform.rect;
        Vector2 resolution = new Vector2(Mathf.Max(1f, rect.width), Mathf.Max(1f, rect.height));

        // ShaderToy mappings: iTime is elapsed seconds, iResolution is the preview rect size,
        // and iMouse stores pixel position plus the left-button pressed state.
        Vector2 mousePosition = GetMousePosition();
        Vector4 shaderMouse = new Vector4(
            mousePosition.x,
            mousePosition.y,
            IsMousePressed() ? 1f : 0f,
            0f);

        if (runtimeMaterial == null || shaderPreview.material != runtimeMaterial)
        {
            runtimeMaterial = shaderPreview.material;
        }

        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.SetFloat(ITime, Time.unscaledTime);
        runtimeMaterial.SetVector(IResolution, new Vector4(resolution.x, resolution.y, 0f, 0f));
        runtimeMaterial.SetVector(IMouse, shaderMouse);
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
