using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class CStringCode : MonoBehaviour
{
    public List<string> storage;
    public TextMeshProUGUI textElement;
    public GameObject outputArea;
    public Button runButton;
    public Button deleteButton;
    private Vector3 outputAreaPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outputAreaPosition = outputArea.transform.position;
        storage = new List<string>();
        InitializeText();
        runButton.onClick.AddListener(PrintMethod);
        deleteButton.onClick.AddListener(DeleteTextBox);
    }

    public void PrintMethod(){
        foreach(string eq in storage){
            textElement.text += eq + "\n";
        }
    }

    public void AddEquation(string equation){
        storage.Add(equation);
    }

    public void DeleteTextBox(){
        if(textElement != null){
            Destroy(textElement);
        }
        if(storage != null){
            storage.Clear();
        }
        InitializeText();
    }

    public void InitializeText(){
        GameObject textObj = new GameObject("MyTMPText");
        Canvas canvas = FindObjectOfType<Canvas>();
        if(canvas != null){
            textObj.transform.SetParent(canvas.transform, false);
        }
        textElement = textObj.AddComponent<TextMeshProUGUI>();
        textElement.lineSpacing = 95f;
        textElement.enableWordWrapping = false;
        textElement.transform.position = new Vector3(outputAreaPosition.x + 350, outputAreaPosition.y - 155, outputAreaPosition.z);
    }
}