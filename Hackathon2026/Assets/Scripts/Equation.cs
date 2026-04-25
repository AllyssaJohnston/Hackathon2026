using UnityEngine;

public class Equation : MonoBehaviour
{
    public Block constVar;
    public Op operation;

    public int Compute(int inputVar)
    {
        int prevVal = inputVar;
        switch (operation.op)
        {
            case EOp.EOp_ADD:
                inputVar += constVar.getVal();
                break;
            case EOp.EOp_SUB:
                inputVar -= constVar.getVal();
                break;
            case EOp.EOp_MUL:
                inputVar *= constVar.getVal();
                break;
            case EOp.EOp_DIV:
                inputVar /= constVar.getVal();
                break;
            case EOp.EOp_MOD:
                inputVar %= constVar.getVal();
                break;
            default:
                Debug.Log("unknown op" + operation);
                break;
        }
        Debug.Log(prevVal.ToString() + " to " + inputVar.ToString());
        return inputVar;
    }
}
