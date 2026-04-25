using UnityEngine;

[System.Serializable]
public struct EquationDetails
{
    public int constVar;
    public EOp operation;

    public EquationDetails(int constVar, EOp operation)
    {
        this.constVar = constVar;
        this.operation = operation;
    }
}

public class Loop : MonoBehaviour
{
    //[SerializeField] EquationStruct[] equations;
    public int numRepeat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
    }

    public void Compute(int inputVar)
    {
        for (int i = 0; i < numRepeat; i++)
        {
            //foreach (EquationStruct eq in equations)
            //{
            //    eq.Compute(inputVar);
            //}
        }
       
    }
}
