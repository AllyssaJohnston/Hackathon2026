using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MouseCode : MonoBehaviour
{
    private Vector3 mousePosition;
    public GameObject holdingBlock;
    private int layer;
    private bool grabbingBlock = false;
    private bool hoveringOverTile = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layer = LayerMask.NameToLayer("Block");
    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.Set(mousePosition.x, mousePosition.y, 0f);

        if(Mouse.current.leftButton.wasPressedThisFrame){
            grabbingBlock = !grabbingBlock;
            if(grabbingBlock){
                SelectBlock(mousePosition);
            }
        }
        if(grabbingBlock){
            if(holdingBlock != null){
                UpdateBlockPosition();
            }
        }
    }

    void SelectBlock(Vector3 position){
        Debug.Log("Selecting");
        Collider2D collider = Physics2D.OverlapPoint(position);
        if(collider != null){
            holdingBlock = collider.gameObject;
        }
    }

    void UpdateBlockPosition(){
        //temporary object grab
        holdingBlock.transform.position = mousePosition;
    }
}
