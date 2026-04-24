using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MouseCode : MonoBehaviour
{
    private Vector3 mousePosition;
    private Vector3 oldBlockPosition;
    public GameObject holdingBlock;
    public GameObject grid;
    private GridTileCode[] tiles;
    private List<GridTileCode> usedTiles;
    private int layer;
    private bool grabbingBlock = false;

    int blockLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blockLayer = LayerMask.NameToLayer("Block");
        tiles = grid.GetComponentsInChildren<GridTileCode>();
        usedTiles = new List<GridTileCode>();
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
                if(holdingBlock != null){
                    AdjustBlockToClosestTile();
                    holdingBlock = null;
                }
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
            oldBlockPosition = holdingBlock.transform.position;
        }
        else {
            holdingBlock = null;
        }
    }

    void AdjustBlockToClosestTile(){
        if (holdingBlock == null) { return; }
        double closestPosition = double.MaxValue;
        GameObject closestTile = null;
        
        Vector3 blockPosition = holdingBlock.transform.position;
        GridTileCode newTile = null;

        foreach(GridTileCode tile in tiles){
            double tempDistance = Distance(tile);
            if (closestPosition > tempDistance){
                closestPosition = tempDistance;
                closestTile = tile.gameObject;
                newTile = tile;
            }                       
        }
        Loop loopBlock = closestTile.GetComponent<Loop>();
            Block inputVarBlock = holdingBlock.GetComponent<Block>();
            if (loopBlock != null && inputVarBlock != null)
            {
                Debug.Log("found");
                loopBlock.Compute(inputVarBlock);
            }
        if(usedTiles.Contains(newTile)){
            holdingBlock.transform.position = oldBlockPosition;
            return;
        }
        Vector3 tilePosition = closestTile.transform.position;
        if(tilePosition.x != blockPosition.x || tilePosition.y != blockPosition.y){
            
            holdingBlock.transform.position = new Vector3(tilePosition.x, tilePosition.y, 0f);
            usedTiles.Add(newTile);
            Block blockSleected = holdingBlock.GetComponent<Block>();
            if(blockSleected.pastTile != null){
                usedTiles.Remove(blockSleected.pastTile);
            }
            blockSleected.pastTile = newTile;
        }
        
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
