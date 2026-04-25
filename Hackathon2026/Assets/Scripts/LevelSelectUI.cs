using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            levelButtons[i].interactable = levelNumber <= PlayerData.LevelsUnlocked;
            levelButtons[i].onClick.AddListener(delegate { OpenLevel(levelNumber); });
        }

        backButton.onClick.AddListener(BackToStartMenu);
    }

    private bool HasRequiredReferences()
    {
        if (levelButtons == null || levelButtons.Length != 10 || backButton == null)
        {
            Debug.LogError("LevelSelectUI is missing one or more Inspector references.", this);
            return false;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null)
            {
                Debug.LogError("LevelSelectUI has a missing level button reference.", this);
                return false;
            }
        }

        return true;
    }

    private void OpenLevel(int levelNumber)
    {
        PlayerData.CurrentLevel = levelNumber;
        PlayerData.Save();
        SceneManager.LoadScene("Gameplay");
    }

    private void BackToStartMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
