using UnityEngine;
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
            levelButtons[i].interactable = PlayerData.LevelsUnlocked >= levelNumber;
            levelButtons[i].onClick.AddListener(delegate { LoadLevel(levelNumber); });
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

    public void LoadLevel(int levelNumber)
    {
        if (SceneTransitionManager.Instance.IsTransitioning)
        {
            return;
        }

        if (PlayerData.LevelsUnlocked < levelNumber)
        {
            return;
        }

        SFXManager.Instance.PlayUIPress();
        SceneTransitionManager.Instance.LoadSceneAfterAction(
            "Gameplay",
            () =>
            {
                PlayerData.CurrentLevel = levelNumber;
                PlayerData.Save();
            });
    }

    private void BackToStartMenu()
    {
        if (SceneTransitionManager.Instance.IsTransitioning)
        {
            return;
        }

        SFXManager.Instance.PlayUIPress();
        SceneTransitionManager.Instance.LoadScene("StartMenu");
    }
}
