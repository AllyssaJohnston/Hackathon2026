using UnityEngine;

public class TileSelectCode : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool selected;
    public MouseCode mouse;
    void Start()
    {
        selected = false;
    }

    void HoverEnter(){
        selected = true;
    }

    void Selected(){
        mouse.holdingBlock = gameObject;
    }

    void Deselected(){
        mouse.holdingBlock = null;
    }

    void HoverExit(){
        selected = false;
    }
}
