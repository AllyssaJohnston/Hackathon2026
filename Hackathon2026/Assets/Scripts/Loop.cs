using UnityEngine;

public class Loop : MonoBehaviour
{
    [SerializeField] Equation[] equations;
    public int numRepeat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Compute()
    {
        for (int i = 0; i < numRepeat; i++)
        {
            foreach (Equation eq in equations)
            {
                eq.Compute();
            }
        }
       
    }
}
