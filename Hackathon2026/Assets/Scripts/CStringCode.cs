using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CStringCode : MonoBehaviour
{
    public List<string> storage = new List<string>();
    public TextMeshProUGUI textElement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { textElement.gameObject.SetActive(false); }

    public void PrintMethod()
    {
        foreach (string eq in storage){
            textElement.text += eq + "\n";
        }
        storage.Clear();
        textElement.gameObject.SetActive(true);
    }

    public void AddEquation(string equation) { storage.Add(equation); }

    public void DeleteTextBox()
    {
        textElement.text = "";
        textElement.gameObject.SetActive(false);
        storage.Clear();        
    }
}