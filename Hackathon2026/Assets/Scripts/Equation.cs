using UnityEngine;

public class Equation : MonoBehaviour
{
    [SerializeField] GameObject gridInputSpot;
    [SerializeField] Block inputVar;
    [SerializeField] Block constVar;
    [SerializeField] EOp operation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputVar = GetComponent<Block>();
        gridInputSpot.GetComponent<SpriteRenderer>().color = Color.green;
    }


    public void setInputVar(Block var) { inputVar = var; }

    public void Compute()
    {
        switch (operation)
        {
        case EOp.EOp_ADD:
            inputVar.changeVal(inputVar.getVal() + constVar.getVal());
            break;
        case EOp.EOp_SUB:
            inputVar.changeVal(inputVar.getVal() - constVar.getVal());
            break;
        case EOp.EOp_MUL:
            inputVar.changeVal(inputVar.getVal() * constVar.getVal());
            break;
        case EOp.EOp_DIV:
            inputVar.changeVal(inputVar.getVal() / constVar.getVal());
            break;
        case EOp.EOp_MOD:
            inputVar.changeVal(inputVar.getVal() % constVar.getVal());
            break;
        default:
            Debug.Log("unknown op" + operation);
            break;
        }

    }
}
