using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private SettingsUI settingsUI;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        playButton.onClick.AddListener(Play);
        settingsButton.onClick.AddListener(settingsUI.Show);
        quitButton.onClick.AddListener(Quit);
    }

    private bool HasRequiredReferences()
    {
        if (playButton == null || settingsButton == null || quitButton == null || settingsUI == null)
        {
            Debug.LogError("MainMenuUI is missing one or more Inspector references.", this);
            return false;
        }

        return true;
    }

    private void Play() { SceneTransitionManager.Instance.LoadScene("LevelSelect"); }

    private void Quit() { Application.Quit(); }
}
