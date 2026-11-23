using UnityEngine;

/// <summary>
/// Stores the player's data between game sessions.
/// PlayerPrefs persists the value locally on the player's device.
/// </summary>
public static class PlayerStorage
{
    private const string CurrentLevelKey = "Player.CurrentLevel";
    private const string CoinsKey = "Player.Coins";

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

    /// <summary>
    /// Gets the player's saved coin amount, falling back to zero coins.
    /// </summary>
    public static int GetCoins()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(CoinsKey, 0));
    }

    /// <summary>
    /// Saves the player's coin amount immediately.
    /// </summary>
    private static void SetCoins(int coins)
    {
        if (coins < 0)
        {
            Debug.LogWarning($"PlayerStorage: coins cannot be negative, received {coins}.");
            coins = 0;
        }

        PlayerPrefs.SetInt(CoinsKey, coins);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Adds coins to the player's saved balance and returns the new balance.
    /// </summary>
    public static int AddCoins(int amount)
    {
        int newBalance = Mathf.Max(0, GetCoins() + amount);
        SetCoins(newBalance);
        return newBalance;
    }
}
