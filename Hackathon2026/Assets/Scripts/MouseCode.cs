using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MouseCode : MonoBehaviour
{
    private Vector3 mousePosition;
    public GameObject holdingBlock;
    public GameObject grid;
    private GridTileCode[] tiles;
    private int layer;
    private bool grabbingBlock = false;
    private bool hoveringOverTile = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layer = LayerMask.NameToLayer("Block");
        tiles = grid.GetComponentsInChildren<GridTileCode>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.Set(mousePosition.x, mousePosition.y, 0f);

        if(Mouse.current.leftButton.wasPressedThisFrame){
            if(CheckForBlock(mousePosition)){
                grabbingBlock = !grabbingBlock;
            }
            if(grabbingBlock){
                SelectBlock(mousePosition);
            }
            else {
                AdjustBlockToClosestTile();
            }
        }
        if(grabbingBlock){
            if(holdingBlock != null){
                UpdateBlockPosition();
            }
        }
    }

    bool CheckForBlock(Vector3 position){
        Collider2D collider = Physics2D.OverlapPoint(position);
        return collider != null;
    }

    void SelectBlock(Vector3 position){
        Debug.Log("Selecting");
        Collider2D collider = Physics2D.OverlapPoint(position);
        if(collider != null){
            holdingBlock = collider.gameObject;
        }
        else {
            holdingBlock = null;
        }
    }

    void AdjustBlockToClosestTile(){
        double closestPosition = double.MaxValue;
        GameObject closestTile = null;
        Vector3 tilePosition = new Vector3(0,0,0);
        foreach(GridTileCode tile in tiles){
            double tempDistance = Distance(tile);
            if(closestPosition > tempDistance){
                closestPosition = tempDistance;
                tilePosition = tile.transform.position;
                closestTile = tile.gameObject;
            }                            
        }
        Equation eq = closestTile.GetComponent<Equation>();
        if(eq != null){
            eq.inputVar = holdingBlock.GetComponent<Block>();
            eq.Compute();
        }
        holdingBlock.transform.position = new Vector3(tilePosition.x, tilePosition.y, 0f);
    }

    double Distance(GridTileCode tile){
        Vector3 blockPosition = holdingBlock.transform.position;
        float xDistance = blockPosition.x - tile.transform.position.x;
        float yDistance = blockPosition.y - tile.transform.position.y;
        double distance = Mathf.Sqrt(Mathf.Pow(xDistance, 2) + Mathf.Pow(yDistance, 2));
        return distance;
    }

    void UpdateBlockPosition(){
        //temporary object grab
        holdingBlock.transform.position = mousePosition;
    }
}
