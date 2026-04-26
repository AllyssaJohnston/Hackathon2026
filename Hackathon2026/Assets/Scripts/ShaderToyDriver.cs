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

    private void Awake()
    {
        if (shaderPreview != null && shaderMaterial != null)
        {
            shaderPreview.material = shaderMaterial;
        }
    }

    private void Update()
    {
        if (shaderPreview == null || shaderPreview.material == null)
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

        Material material = shaderPreview.material;
        material.SetFloat(ITime, Time.time);
        material.SetVector(IResolution, new Vector4(resolution.x, resolution.y, 0f, 0f));
        material.SetVector(IMouse, shaderMouse);
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
