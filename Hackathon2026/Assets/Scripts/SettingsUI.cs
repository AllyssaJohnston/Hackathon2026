using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Text sfxValueText;
    [SerializeField] private Text textSizeValueText;
    [SerializeField] private Button toggleSFXButton;
    [SerializeField] private Button smallTextButton;
    [SerializeField] private Button mediumTextButton;
    [SerializeField] private Button largeTextButton;
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        toggleSFXButton.onClick.AddListener(ToggleSFX);
        smallTextButton.onClick.AddListener(delegate { SetTextSize(0); });
        mediumTextButton.onClick.AddListener(delegate { SetTextSize(1); });
        largeTextButton.onClick.AddListener(delegate { SetTextSize(2); });
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
        if (settingsPanel == null || sfxValueText == null || textSizeValueText == null ||
            toggleSFXButton == null || smallTextButton == null || mediumTextButton == null ||
            largeTextButton == null || resetProgressButton == null || backButton == null)
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
        RefreshLabels();
    }

    private void SetTextSize(int size)
    {
        PlayerData.TextSize = size;
        PlayerData.Save();
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

        if (textSizeValueText != null)
        {
            string sizeName = "Medium";

            if (PlayerData.TextSize == 0)
            {
                sizeName = "Small";
            }
            else if (PlayerData.TextSize == 2)
            {
                sizeName = "Large";
            }

            textSizeValueText.text = "Text Size: " + sizeName;
        }
    }
}
