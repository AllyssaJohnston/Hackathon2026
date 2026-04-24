using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    private struct Level
    {
        public int startValue;
        public int targetValue;
        public string[] blocks;
        public string hint;
    }

    [SerializeField] private Text levelNumberText;
    [SerializeField] private Text startValueText;
    [SerializeField] private Text targetValueText;
    [SerializeField] private Text currentValueText;
    [SerializeField] private Transform availableBlockArea;
    [SerializeField] private Transform codeArea;
    [SerializeField] private Text codeAreaText;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Button runButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button blockButtonPrefab;

    private readonly List<string> selectedBlocks = new List<string>();
    private Level[] levels;
    private Level currentLevel;
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

    private bool HasRequiredReferences()
    {
        if (levelNumberText == null || startValueText == null || targetValueText == null ||
            currentValueText == null || availableBlockArea == null || codeArea == null ||
            codeAreaText == null || feedbackText == null || runButton == null || resetButton == null ||
            hintButton == null || backButton == null || blockButtonPrefab == null)
        {
            Debug.LogError("GameplayManager is missing one or more Inspector references.", this);
            return false;
        }

        return true;
    }

    private void CreateLevels()
    {
        levels = new Level[]
        {
            NewLevel(0, 5, new string[] { "= 5" }, "Use = 5 to set x to the target."),
            NewLevel(1, 3, new string[] { "+= 2" }, "Add 2 to x."),
            NewLevel(2, 6, new string[] { "+= 4" }, "Add 4 to x."),
            NewLevel(5, 2, new string[] { "-= 3" }, "Subtract 3 from x."),
            NewLevel(10, 4, new string[] { "-= 6" }, "Subtract 6 from x."),
            NewLevel(3, 9, new string[] { "*= 3" }, "Multiply x by 3."),
            NewLevel(2, 10, new string[] { "+= 3", "*= 2" }, "Try += 3, then *= 2."),
            NewLevel(4, 7, new string[] { "*= 2", "-= 1" }, "Try *= 2, then -= 1."),
            NewLevel(1, 8, new string[] { "+= 3", "*= 2" }, "Try += 3, then *= 2."),
            NewLevel(6, 10, new string[] { "-= 1", "*= 2" }, "Try -= 1, then *= 2.")
        };
    }

    private Level NewLevel(int startValue, int targetValue, string[] blocks, string hint)
    {
        Level level = new Level();
        level.startValue = startValue;
        level.targetValue = targetValue;
        level.blocks = blocks;
        level.hint = hint;
        return level;
    }

    private void LoadCurrentLevel()
    {
        currentLevelNumber = PlayerData.CurrentLevel;
        currentLevel = levels[currentLevelNumber - 1];
        currentValue = currentLevel.startValue;

        selectedBlocks.Clear();
        ClearAvailableBlockArea();
        CreateAvailableBlockButtons();
        RefreshUI();

        feedbackText.text = "Click blocks, then press Run.";
    }

    private void ClearAvailableBlockArea()
    {
        foreach (Transform child in availableBlockArea)
        {
            if (child.gameObject != blockButtonPrefab.gameObject)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CreateAvailableBlockButtons()
    {
        foreach (string block in currentLevel.blocks)
        {
            string selectedBlock = block;
            Button button = Instantiate(blockButtonPrefab, availableBlockArea);
            button.gameObject.SetActive(true);
            button.interactable = true;
            button.onClick.AddListener(delegate { AddBlock(selectedBlock); });

            Text label = button.GetComponentInChildren<Text>();

            if (label != null)
            {
                label.text = selectedBlock;
            }
        }
    }

    private void AddBlock(string block)
    {
        selectedBlocks.Add(block);
        feedbackText.text = "Added " + block + ".";
        RefreshUI();
    }

    private void RunCode()
    {
        currentValue = currentLevel.startValue;

        foreach (string block in selectedBlocks)
        {
            ApplyBlock(block);
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

    private void ApplyBlock(string block)
    {
        string[] parts = block.Split(' ');
        string operation = parts[0];
        int number = int.Parse(parts[1]);

        if (operation == "=")
        {
            currentValue = number;
        }
        else if (operation == "+=")
        {
            currentValue += number;
        }
        else if (operation == "-=")
        {
            currentValue -= number;
        }
        else if (operation == "*=")
        {
            currentValue *= number;
        }
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
        feedbackText.text = currentLevel.hint;
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
        codeAreaText.text = selectedBlocks.Count == 0 ? "No blocks selected." : string.Join("\n", selectedBlocks.ToArray());
    }
}
