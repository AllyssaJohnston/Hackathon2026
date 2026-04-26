using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(RawImage))]
public sealed class ShaderBackgroundDriver : MonoBehaviour
{
    private static readonly int ITime = Shader.PropertyToID("_iTime");
    private static readonly int IResolution = Shader.PropertyToID("_iResolution");
    private static readonly int IMouse = Shader.PropertyToID("_iMouse");

    [SerializeField] private RawImage targetImage;
    [SerializeField] private Material sourceMaterial;

    private Material runtimeMaterial;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<RawImage>();
        }

        if (sourceMaterial != null)
        {
            runtimeMaterial = Instantiate(sourceMaterial);
            runtimeMaterial.name = $"{sourceMaterial.name} (Runtime)";
            runtimeMaterial.hideFlags = HideFlags.DontSave;
            targetImage.material = runtimeMaterial;
        }
    }

    private void Update()
    {
        Material material = runtimeMaterial != null ? runtimeMaterial : targetImage.material;
        if (targetImage == null || material == null)
        {
            return;
        }

        Rect rect = targetImage.rectTransform.rect;
        Vector2 resolution = new Vector2(Mathf.Max(1f, rect.width), Mathf.Max(1f, rect.height));
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
