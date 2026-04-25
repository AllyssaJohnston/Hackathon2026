using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct EquationDetails
{
    public int constVar;
    public EOp operation;
}

public class Equation : MonoBehaviour
{
    public Block constVar;
    public Op operation;
    private string equation;
    private string comment;
    public CStringCode logger;

    public int Compute(int inputVar)
    {
        int prevVal = inputVar;
        equation += "x" + " ";
        comment += inputVar + " ";
        switch (operation.op)
        {
            case EOp.EOp_ADD:
                equation += "+= " + constVar.getVal();
                comment += "+= " + constVar.getVal();
                inputVar += constVar.getVal();
                break;
            case EOp.EOp_SUB:
                equation += "-= " + constVar.getVal();
                comment += "-= " + constVar.getVal();
                inputVar -= constVar.getVal();
                break;
            case EOp.EOp_MUL:
                equation += "*= " + constVar.getVal();
                comment += "*= " + constVar.getVal();
                inputVar *= constVar.getVal();
                break;
            case EOp.EOp_DIV:
                equation += "/= " + constVar.getVal();
                comment += "/= " + constVar.getVal();
                inputVar /= constVar.getVal();
                break;
            case EOp.EOp_MOD:
                equation += "%= " + constVar.getVal();
                comment += "%= " + constVar.getVal();
                inputVar %= constVar.getVal();
                break;
            default:
                Debug.Log("unknown op" + operation);
                break;
        }
        Debug.Log(prevVal.ToString() + " to " + inputVar.ToString());
        equation += " //" + comment;
        logger.AddEquation(equation);
        return inputVar;
    }
}
