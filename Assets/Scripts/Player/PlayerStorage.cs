using UnityEngine;

/// <summary>
/// Stores the player's data between game sessions.
/// PlayerPrefs persists the value locally on the player's device.
/// </summary>
public static class PlayerStorage
{
    private const string CurrentLevelKey = "Player.CurrentLevel";
    private const string CoinsKey = "Player.Coins";
    private const string ProfileImageKey = "Player.ProfileImage";
    private const string PlayerNameKey = "Player.Name";
    private const int StartingCoins = 200;
    private const string DefaultProfileImage = "cyan";
    private const string DefaultPlayerName = "Player";

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
    /// Gets the player's saved coin amount, falling back to the starting balance.
    /// </summary>
    public static int GetCoins()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(CoinsKey, StartingCoins));
    }

    /// <summary>Gets the selected profile image name.</summary>
    public static string GetProfileImageName()
    {
        return PlayerPrefs.GetString(ProfileImageKey, DefaultProfileImage);
    }

    /// <summary>Saves the selected profile image name immediately.</summary>
    public static void SetProfileImageName(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName))
        {
            Debug.LogWarning("PlayerStorage: profile image name cannot be empty.");
            return;
        }

        PlayerPrefs.SetString(ProfileImageKey, imageName);
        PlayerPrefs.Save();
    }

    /// <summary>Gets the player's saved display name.</summary>
    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, DefaultPlayerName);
    }

    /// <summary>Saves the player's display name immediately.</summary>
    public static void SetPlayerName(string playerName)
    {
        string normalizedName = playerName?.Trim();

        if (string.IsNullOrEmpty(normalizedName))
        {
            Debug.LogWarning("PlayerStorage: player name cannot be empty.");
            return;
        }

        PlayerPrefs.SetString(PlayerNameKey, normalizedName);
        PlayerPrefs.Save();
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

    /// <summary>
    /// Removes coins when the player can afford the amount and returns whether the purchase succeeded.
    /// </summary>
    public static bool TrySpendCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"PlayerStorage: coin cost cannot be negative, received {amount}.");
            return false;
        }

        int currentBalance = GetCoins();

        if (currentBalance < amount)
        {
            return false;
        }

        SetCoins(currentBalance - amount);
        return true;
    }
}
