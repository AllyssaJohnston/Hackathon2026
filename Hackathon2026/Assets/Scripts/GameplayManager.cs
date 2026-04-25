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
    public LoopDetails[] loops;
    [HideInInspector] public List<Loop> blockLoopOpts;
    public string hint;
}

public class GameplayManager : MonoBehaviour
{
    public static bool init = false;
    [SerializeField] private Text levelNumberText;
    [SerializeField] private Text startValueText;
    [SerializeField] private Text targetValueText;
    [SerializeField] private Text currentValueText;
    [SerializeField] private Text feedbackText;

    [SerializeField] private Button runButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button backButton;

    [SerializeField] private GameObject loopPrefab;
    [SerializeField] private GameObject eqPrefab;
    [SerializeField] private Transform blockPoolZone;
    [SerializeField] private Transform codeBlockZone;

    Vector3 mousePosition;

    const int maxCodeBlocks = 8;
    int numSelectedBlocks = 0;
    int numSelectedLines = 0;
    GameObject[] selectedBlocks = new GameObject[maxCodeBlocks];
    Vector3 selectedBlockPos = Vector3.zero;

    [SerializeField] Level[] levels;
    Level currentLevel;
    int currentLevelNumber;

    int currentValue;

    float blockVertSpacing = .7f;

    private void Awake()
    {
        if (!HasRequiredReferences()) { return; }

        if (!init)
        {
            PlayerData.ResetProgress();
            init = true;
        }
        CreateLevels();
        LoadCurrentLevel();

        runButton.onClick.AddListener(RunCode);
        resetButton.onClick.AddListener(ResetLevel);
        hintButton.onClick.AddListener(ShowHint);
        backButton.onClick.AddListener(BackToLevelSelect);
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
            currentValueText == null  || feedbackText == null || runButton == null || resetButton == null ||
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
            Vector3 loc = Vector3.zero;
            // convert data struct into actual block objs
            for (int i = 0; i < l.loops.Length; i++)
            {
                Vector3 curLoc = Vector3.zero; 
                l.blockLoopOpts.Add(Instantiate(loopPrefab).GetComponent<Loop>());
                l.blockLoopOpts[i].gameObject.SetActive(false);
                l.blockLoopOpts[i].transform.parent = blockPoolZone.transform;
                l.blockLoopOpts[i].transform.localPosition = loc;
                l.blockLoopOpts[i].numRepeat = l.loops[i].numRepeat;
                int numLines = l.loops[i].eqDetails.Length;
                if (l.blockLoopOpts[i].numRepeat > 1)
                {
                    l.blockLoopOpts[i].repeatBlock.changeVal(l.blockLoopOpts[i].numRepeat);
                    l.blockLoopOpts[i].repeatBlock.gameObject.SetActive(true);
                    curLoc += Vector3.down * blockVertSpacing;
                    numLines++;
                }
                foreach(EquationDetails eq in l.loops[i].eqDetails)
                {
                    Equation eqObj = (Instantiate(eqPrefab)).GetComponent<Equation>();
                    l.blockLoopOpts[i].equations.Add(eqObj);
                    eqObj.gameObject.SetActive(true);
                    eqObj.gameObject.transform.parent = l.blockLoopOpts[i].transform;
                    eqObj.gameObject.transform.localPosition = curLoc;
                    curLoc += Vector3.down * blockVertSpacing;
                    eqObj.constVar.changeVal(eq.constVar);
                    eqObj.operation.op = eq.operation;
                }
                loc += curLoc;
                
                Vector2 oldSize = l.blockLoopOpts[i].GetComponent<BoxCollider2D>().size;
                l.blockLoopOpts[i].GetComponent<BoxCollider2D>().size *= new Vector2(1, numLines);
                l.blockLoopOpts[i].GetComponent<BoxCollider2D>().offset = Vector2.down * (l.blockLoopOpts[i].GetComponent<BoxCollider2D>().size - oldSize) / 2;
                if (numLines == 1)
                {
                    l.blockLoopOpts[i].loopBar.enabled = false;
                }
                else
                {
                    l.blockLoopOpts[i].loopBar.transform.localScale *= new Vector2(1, numLines);
                    l.blockLoopOpts[i].loopBar.transform.localPosition += Vector3.down * (l.blockLoopOpts[i].loopBar.sprite.bounds.size.y - oldSize.y) / 2;
                }
            }
        }
    }

    private void LoadCurrentLevel()
    {
        currentLevelNumber = PlayerData.CurrentLevel;
        currentLevel = levels[currentLevelNumber - 1];
        currentValue = currentLevel.startValue;

        foreach (Loop l in currentLevel.blockLoopOpts)
        {
            l.gameObject.SetActive(true);
        }

        numSelectedBlocks = 0;
        numSelectedLines = 0;
        selectedBlockPos = Vector3.zero;
        RefreshUI();

        feedbackText.text = "Click blocks, then press Run.";
    }

    private void RunCode()
    {
        currentValue = currentLevel.startValue;
        for (int i = 0; i < numSelectedBlocks; i++)
        {
            currentValue = selectedBlocks[i].GetComponent<Loop>().Compute(currentValue);
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
        if (currentLevelNumber == PlayerData.LevelsUnlocked && currentLevelNumber < levels.Length)
        {
            PlayerData.LevelsUnlocked = currentLevelNumber + 1;
            PlayerData.Save();
        }
    }

    private void ResetLevel()
    {
        numSelectedBlocks = 0;
        numSelectedLines = 0;
        selectedBlockPos = Vector3.zero;
        currentValue = currentLevel.startValue;
        feedbackText.text = "Level reset.";
        foreach (Transform child in codeBlockZone)
        {
            Destroy(child.gameObject);
        }
        RefreshUI();
    }

    private void ShowHint() { feedbackText.text = currentLevel.hint; }

    private void BackToLevelSelect() { SceneManager.LoadScene("LevelSelect"); }

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
        if (numSelectedLines >= maxCodeBlocks)
        {
            return; // no room for block
        }
        Collider2D collider = Physics2D.OverlapPoint(position);
        if (collider != null)
        {
            GameObject curObject = collider.gameObject;
            Loop loopScr = curObject.GetComponent<Loop>();
            int numLines = loopScr.equations.Count;
            if (loopScr.numRepeat > 1)
            {
                numLines++;
            }
            if (numSelectedLines + numLines > maxCodeBlocks)
            {
                return; // no room for block
            }
            GameObject newEq = Instantiate(curObject);
            newEq.transform.parent = codeBlockZone;
            newEq.transform.localPosition = selectedBlockPos;
            selectedBlockPos += numLines * (Vector3.down * blockVertSpacing);

            GameObject.Destroy(selectedBlocks[numSelectedBlocks]); // safe to call on null
            selectedBlocks[numSelectedBlocks] = newEq;
            numSelectedBlocks++;
            numSelectedLines += numLines;
        }
    }
}
