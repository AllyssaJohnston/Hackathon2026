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
        textElement.gameObject.SetActive(false);
        outputAreaPosition = outputArea.transform.position;
        storage = new List<string>();
        InitializeText();
        runButton.onClick.AddListener(PrintMethod);
        deleteButton.onClick.AddListener(DeleteTextBox);
    }

    public void PrintMethod()
    {
        foreach (string eq in storage){
            textElement.text += eq + "\n";
        }
        textElement.gameObject.SetActive(true);
    }

    public void AddEquation(string equation){
        storage.Add(equation);
    }

    public void DeleteTextBox()
    {
        textElement.gameObject.SetActive(false);
        if (storage != null){
            storage.Clear();
        }
    }

    public void InitializeText()
    {
        textElement.lineSpacing = 95f;
        textElement.textWrappingMode = TextWrappingModes.NoWrap;
        textElement.transform.position = new Vector3(outputAreaPosition.x + 150, outputAreaPosition.y - 120, outputAreaPosition.z);
    }
}