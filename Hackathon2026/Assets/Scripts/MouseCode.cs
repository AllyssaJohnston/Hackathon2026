using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCode : MonoBehaviour
{
    private Vector3 mousePosition;
    public GameObject holdingBlock;
    public GameObject grid;
    private GridTileCode[] tiles;
    private bool grabbingBlock = false;

    int blockLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blockLayer = LayerMask.NameToLayer("Block");
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

    bool CheckForBlock(Vector3 position) { return Physics2D.OverlapPoint(position) != null; }

    // set the input block the mouse is currently hovering over
    void SelectBlock(Vector3 position){
        Collider2D collider = Physics2D.OverlapPoint(position);
        if (collider != null && collider.gameObject.layer == blockLayer){
            holdingBlock = collider.gameObject;
        }
        else {
            holdingBlock = null;
        }
    }

    void AdjustBlockToClosestTile(){
        if (holdingBlock == null) { return; }
        double closestPosition = double.MaxValue;
        GameObject closestTile = null;
        foreach(GridTileCode tile in tiles){
            double tempDistance = Distance(tile);
            if (closestPosition > tempDistance){
                closestPosition = tempDistance;
                closestTile = tile.gameObject;
            }                            
        }
        Equation eq = closestTile.GetComponent<Equation>();
        Block inputVarBlock = holdingBlock.GetComponent<Block>();
        if (eq != null && inputVarBlock != null)
        {
            eq.setInputVar(inputVarBlock);
            eq.Compute();
        }
        holdingBlock.transform.position = new Vector3(closestTile.transform.position.x, closestTile.transform.position.y, 0f);
    }

    double Distance(GridTileCode tile){
        Vector3 blockPosition = holdingBlock.transform.position;
        float xDistance = blockPosition.x - tile.transform.position.x;
        float yDistance = blockPosition.y - tile.transform.position.y;
        return Mathf.Sqrt(Mathf.Pow(xDistance, 2) + Mathf.Pow(yDistance, 2));
    }

    //temporary object grab
    void UpdateBlockPosition() { holdingBlock.transform.position = mousePosition; }
}
