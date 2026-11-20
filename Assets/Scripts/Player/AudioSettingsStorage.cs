using UnityEngine;

/// <summary>Stores the player's audio settings between game sessions.</summary>
public static class AudioSettingsStorage
{
    private const string SoundEnabledKey = "UI.SoundEnabled";

    public static bool IsSoundEnabled => PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;

    public static void SetSoundEnabled(bool isEnabled)
    {
        PlayerPrefs.SetInt(SoundEnabledKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
