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
        loopString = "";
        for (int i = 0; i < numRepeat; i++)
        {
            foreach(Equation eq in equations)
            {
                inputVar = eq.Compute(inputVar);
                loopString += eq.equation;
            }
        }
        return inputVar;
    }
}