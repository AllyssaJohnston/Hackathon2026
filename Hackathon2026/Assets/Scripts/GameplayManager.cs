using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct Level
{
    public int startValue;
    public int targetValue;
    public EquationDetails[] equations;
    public List<Equation> blockEquations;
}

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Text levelNumberText;
    [SerializeField] private Text startValueText;
    [SerializeField] private Text targetValueText;
    [SerializeField] private Text currentValueText;
    [SerializeField] private Transform availableBlockArea;
    [SerializeField] private Transform codeArea;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Button runButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button backButton;

    [SerializeField] private GameObject eqPrefab;
    [SerializeField] private Transform codeBlockZone;

    private Vector3 mousePosition;
    private int blockLayer;

    private readonly List<GameObject> selectedBlocks = new List<GameObject>();
    [SerializeField] Level[] levels;
    [SerializeField] Level currentLevel;
    private int currentLevelNumber;
    private int currentValue;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        CreateLevels();
        LoadCurrentLevel();

        runButton.onClick.AddListener(RunCode);
        resetButton.onClick.AddListener(ResetLevel);
        hintButton.onClick.AddListener(ShowHint);
        backButton.onClick.AddListener(BackToLevelSelect);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blockLayer = LayerMask.NameToLayer("Block");
    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.Set(mousePosition.x, mousePosition.y, 0f);
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SelectBlock(mousePosition);
        }
    }

    private bool HasRequiredReferences()
    {
        if (levelNumberText == null || startValueText == null || targetValueText == null ||
            currentValueText == null || availableBlockArea == null || codeArea == null ||
            feedbackText == null || runButton == null || resetButton == null ||
            hintButton == null || backButton == null)
        {
            Debug.LogError("GameplayManager is missing one or more Inspector references.", this);
            return false;
        }

        return true;
    }

    private void CreateLevels()
    {
        foreach(Level l in levels)
        {
            // convert data struct into actual block objs
            for (int i = 0; i < l.equations.Length; i++)
            {
                l.blockEquations[i] = Instantiate(eqPrefab).GetComponent<Equation>();
                l.blockEquations[i].gameObject.SetActive(true);
                l.blockEquations[i].constVar.changeVal(l.equations[i].constVar);
                l.blockEquations[i].operation.op = l.equations[i].operation;
            }
        }
    }

    //private void CreateLevels()
    //{
    //    levels = new Level[]
    //    {
    //        Level(0, 5, new EquationStruct[] { EquationStruct(blockPrefab.Clone(), } //, "Use = 5 to set x to the target."),
    //        Level(1, 3, new EquationStruct[] { "+= 2" }, "Add 2 to x."),
    //        Level(2, 6, new EquationStruct[] { "+= 4" }, "Add 4 to x."),
    //        Level(5, 2, new EquationStruct[] { "-= 3" }, "Subtract 3 from x."),
    //        Level(10, 4, new EquationStruct[] { "-= 6" }, "Subtract 6 from x."),
    //        Level(3, 9, new EquationStruct[] { "*= 3" }, "Multiply x by 3."),
    //        Level(2, 10, new EquationStruct[] { "+= 3", "*= 2" }, "Try += 3, then *= 2."),
    //        Level(4, 7, new EquationStruct[] { "*= 2", "-= 1" }, "Try *= 2, then -= 1."),
    //        Level(1, 8, new EquationStruct[] { "+= 3", "*= 2" }, "Try += 3, then *= 2."),
    //        Level(6, 10, new EquationStruct[] { "-= 1", "*= 2" }, "Try -= 1, then *= 2.")
    //    };
    //}

    private void LoadCurrentLevel()
    {
        currentLevelNumber = PlayerData.CurrentLevel;
        currentLevel = levels[currentLevelNumber - 1];
        currentValue = currentLevel.startValue;

        selectedBlocks.Clear();
        ClearAvailableBlockArea();
        RefreshUI();

        feedbackText.text = "Click blocks, then press Run.";
    }

    private void ClearAvailableBlockArea()
    {
        foreach (Transform child in availableBlockArea)
        {
            Destroy(child.gameObject);
        }
    }

    private void RunCode()
    {
        currentValue = currentLevel.startValue;

        foreach (GameObject block in selectedBlocks)
        {
            currentValue = block.GetComponent<Equation>().Compute(currentValue);
        }

        if (currentValue == currentLevel.targetValue)
        {
            feedbackText.text = "Correct. x reached the target.";
            UnlockNextLevel();
        }
        else
        {
            feedbackText.text = "x did not reach the target.";
        }

        RefreshUI();
    }

    private void UnlockNextLevel()
    {
        if (currentLevelNumber == PlayerData.LevelsUnlocked && currentLevelNumber < 10)
        {
            PlayerData.LevelsUnlocked = currentLevelNumber + 1;
            PlayerData.Save();
        }
    }

    private void ResetLevel()
    {
        selectedBlocks.Clear();
        currentValue = currentLevel.startValue;
        feedbackText.text = "Level reset.";
        RefreshUI();
    }

    private void ShowHint()
    {
        //feedbackText.text = currentLevel.hint;
    }

    private void BackToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    private void RefreshUI()
    {
        levelNumberText.text = "Level " + currentLevelNumber;
        startValueText.text = "Start Value: " + currentLevel.startValue;
        targetValueText.text = "Target Value: " + currentLevel.targetValue;
        currentValueText.text = "Current Value: " + currentValue;
    }

    // set the input block the mouse is currently hovering over
    void SelectBlock(Vector3 position)
    {
        Collider2D collider = Physics2D.OverlapPoint(position);
        if (collider != null)
        {
            GameObject curObject = collider.gameObject;
            GameObject newEq = Instantiate(curObject);
            newEq.transform.parent = codeBlockZone;
            newEq.transform.localPosition = Vector3.zero + Vector3.up * -0.7f * (codeBlockZone.childCount - 1);
            selectedBlocks.Add(newEq);
        }
    }

}
