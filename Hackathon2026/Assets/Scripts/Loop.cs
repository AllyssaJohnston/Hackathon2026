using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LoopDetails
{
    public int numRepeat;
    public  EquationDetails[] eqDetails;
}

public class Loop : MonoBehaviour
{
    public List<Equation> equations;
    public int numRepeat = 1;
    public SpriteRenderer loopBar;
    public Block repeatBlock;
    public Vector2 size;
    public string loopString;

    public int Compute(int inputVar)
    {
        for (int i = 0; i < numRepeat; i++)
        {
            if (numRepeat == 1)
            {
                loopString = "";
            }
            else
            {
                loopString = "for (int i = 0; i < " + numRepeat.ToString() + "; i++) {\n";
            }
            for (int j = 0; j < equations.Count; j++)
            {
                inputVar = equations[j].Compute(inputVar);
                loopString += equations[j].equation;
                if (j < equations.Count - 1)
                {
                    loopString += "\n";
                }
            }
            if (numRepeat != 1)
            {
                loopString += "}";
            }
        }
        return inputVar;
    }
}