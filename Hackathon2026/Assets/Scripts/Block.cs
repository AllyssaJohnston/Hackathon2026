using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    [SerializeField] int dataValue;
    [SerializeField] TMP_Text textObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textObj.text = dataValue.ToString();
    }

    public int getVal()
    {
        return dataValue;
    }

    public void changeVal(int newValue)
    {
        dataValue = newValue;
        textObj.text = dataValue.ToString();
    }
}
