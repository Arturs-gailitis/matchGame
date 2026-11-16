using UnityEngine;

/// <summary>
/// Stores the player's data between game sessions.
/// PlayerPrefs persists the value locally on the player's device.
/// </summary>
public static class PlayerStorage
{
    private const string CurrentLevelKey = "Player.CurrentLevel";

    /// <summary>
    /// Gets the player's saved level, falling back to level 1.
    /// </summary>
    public static int GetCurrentLevel()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt(CurrentLevelKey, 1));
    }

    /// <summary>
    /// Saves the player's current level immediately.
    /// </summary>
    public static void SetCurrentLevel(int level)
    {
        if (level < 1)
        {
            Debug.LogWarning($"PlayerLevelStorage: level must be at least 1, received {level}.");
            level = 1;
        }

        PlayerPrefs.SetInt(CurrentLevelKey, level);
        PlayerPrefs.Save();
    }
}
