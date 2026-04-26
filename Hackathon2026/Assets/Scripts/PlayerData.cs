using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private static bool init = false;
    private const string LevelsUnlockedKey = "LevelsUnlocked";
    private const string CurrentLevelKey = "CurrentLevel";
    private const string SFXOnKey = "SFXOn";

    public void Start()
    {
        if (!init)
        {
            SFXOn = true;
            ResetProgress();
            init = true;
        }
    }

    public static int LevelsUnlocked
    {
        get { return PlayerPrefs.GetInt(LevelsUnlockedKey, 1); }
        set { PlayerPrefs.SetInt(LevelsUnlockedKey, Mathf.Clamp(value, 1, 10)); }
    }

    public static int CurrentLevel
    {
        get { return PlayerPrefs.GetInt(CurrentLevelKey, 1); }
        set { PlayerPrefs.SetInt(CurrentLevelKey, Mathf.Clamp(value, 1, 10)); }
    }

    public static bool SFXOn
    {
        get { return PlayerPrefs.GetInt(SFXOnKey, 1) == 1; }
        set { PlayerPrefs.SetInt(SFXOnKey, value ? 1 : 0); }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        LevelsUnlocked = 1;
        CurrentLevel = 1;
        Save();
    }
}
