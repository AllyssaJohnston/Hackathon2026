using UnityEngine;

[System.Serializable]
public struct EquationStruct
{
    public Block constVar;
    public Op operation;

    public void Compute(Block inputVar)
    {
        int prevVal = inputVar.getVal();
        switch (operation.op)
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
        Debug.Log(prevVal.ToString() + " to " + inputVar.getVal().ToString());
    }
}


public class Loop : MonoBehaviour
{
    [SerializeField] EquationStruct[] equations;
    public int numRepeat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
    }

    public void Compute(Block inputVar)
    {
        for (int i = 0; i < numRepeat; i++)
        {
            foreach (EquationStruct eq in equations)
            {
                eq.Compute(inputVar);
            }
        }
       
    }
}
