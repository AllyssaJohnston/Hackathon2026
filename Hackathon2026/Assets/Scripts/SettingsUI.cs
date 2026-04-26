using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Text sfxValueText;
    [SerializeField] private Button toggleSFXButton;
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        toggleSFXButton.onClick.AddListener(ToggleSFX);
        resetProgressButton.onClick.AddListener(ResetProgress);
        backButton.onClick.AddListener(Hide);

        settingsPanel.SetActive(false);
        RefreshLabels();
    }

    public void Show()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("SettingsUI cannot show because the Settings Panel reference is missing.", this);
            return;
        }

        RefreshLabels();
        settingsPanel.SetActive(true);
    }

    public void Hide()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("SettingsUI cannot hide because the Settings Panel reference is missing.", this);
            return;
        }

        settingsPanel.SetActive(false);
    }

    private bool HasRequiredReferences()
    {
        if (settingsPanel == null || sfxValueText == null || toggleSFXButton == null || 
            resetProgressButton == null || backButton == null)
        {
            Debug.LogError("SettingsUI is missing one or more Inspector references.", this);
            return false;
        }

        return true;
    }

    private void ToggleSFX()
    {
        PlayerData.SFXOn = !PlayerData.SFXOn;
        PlayerData.Save();
        BackgroundMusicBootstrap.ToggleMusic();
        RefreshLabels();
    }

    private void ResetProgress()
    {
        PlayerData.ResetProgress();
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        if (sfxValueText != null)
        {
            sfxValueText.text = "Game SFX: " + (PlayerData.SFXOn ? "On" : "Off");
        }
    }
}
