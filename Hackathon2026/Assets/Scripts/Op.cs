using TMPro;
using UnityEngine;

public enum EOp
{ 
    EOp_ADD,
    EOp_SUB,
    EOp_MUL,
    EOp_DIV,
    EOp_MOD
}


public class Op : MonoBehaviour
{
    public EOp op;
    [SerializeField] TMP_Text textObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (op)
        {
            case EOp.EOp_ADD:
                textObj.text = "+=";
                break;
            default:
                Debug.Log("unknown op" + op);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
