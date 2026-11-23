using System;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Stores and restores the player's level energy between game sessions.
/// All timestamps use UTC so recovery also works while the game is closed.
/// </summary>
public static class PlayerEnergyStorage
{
    public const int MaxEnergy = 5;
    public const int EnergyRecoverySeconds = 3 * 60;

    private const string CurrentEnergyKey = "Player.Energy.Current";
    private const string LastDecreaseTimeKey = "Player.Energy.LastDecreaseUtc";
    private const string RecoveryStartTimeKey = "Player.Energy.RecoveryStartUtc";

    public static int GetEnergy()
    {
        return RefreshRecoveredEnergy();
    }

    public static bool HasEnergy()
    {
        return GetEnergy() > 0;
    }

    public static bool TryDecreaseEnergy()
    {
        int currentEnergy = RefreshRecoveredEnergy();

        if (currentEnergy <= 0)
        {
            return false;
        }

        long now = GetCurrentUnixTime();

        if (currentEnergy == MaxEnergy || !PlayerPrefs.HasKey(RecoveryStartTimeKey))
        {
            SetTimestamp(RecoveryStartTimeKey, now);
        }

        PlayerPrefs.SetInt(CurrentEnergyKey, currentEnergy - 1);
        SetTimestamp(LastDecreaseTimeKey, now);
        PlayerPrefs.Save();
        return true;
    }

    public static int GetSecondsUntilNextEnergy()
    {
        int currentEnergy = RefreshRecoveredEnergy();

        if (currentEnergy >= MaxEnergy)
        {
            return 0;
        }

        long recoveryStart = GetTimestamp(RecoveryStartTimeKey, GetCurrentUnixTime());
        long elapsedSeconds = Math.Max(0L, GetCurrentUnixTime() - recoveryStart);
        return EnergyRecoverySeconds - (int)(elapsedSeconds % EnergyRecoverySeconds);
    }

    public static DateTime? GetLastEnergyDecreaseTimeUtc()
    {
        if (!PlayerPrefs.HasKey(LastDecreaseTimeKey))
        {
            return null;
        }

        long unixTime = GetTimestamp(LastDecreaseTimeKey, 0L);
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
    }

    private static int RefreshRecoveredEnergy()
    {
        int currentEnergy = Mathf.Clamp(PlayerPrefs.GetInt(CurrentEnergyKey, MaxEnergy), 0, MaxEnergy);

        if (currentEnergy >= MaxEnergy)
        {
            ClearRecoveryStartIfNeeded();
            return MaxEnergy;
        }

        long now = GetCurrentUnixTime();

        if (!PlayerPrefs.HasKey(RecoveryStartTimeKey))
        {
            SetTimestamp(RecoveryStartTimeKey, now);
            PlayerPrefs.Save();
            return currentEnergy;
        }

        long recoveryStart = GetTimestamp(RecoveryStartTimeKey, now);
        long elapsedSeconds = Math.Max(0L, now - recoveryStart);
        int recoveredEnergy = (int)(elapsedSeconds / EnergyRecoverySeconds);

        if (recoveredEnergy <= 0)
        {
            return currentEnergy;
        }

        currentEnergy = Mathf.Min(MaxEnergy, currentEnergy + recoveredEnergy);
        PlayerPrefs.SetInt(CurrentEnergyKey, currentEnergy);

        if (currentEnergy >= MaxEnergy)
        {
            PlayerPrefs.DeleteKey(RecoveryStartTimeKey);
        }
        else
        {
            SetTimestamp(RecoveryStartTimeKey, recoveryStart + (long)recoveredEnergy * EnergyRecoverySeconds);
        }

        PlayerPrefs.Save();
        return currentEnergy;
    }

    private static void ClearRecoveryStartIfNeeded()
    {
        if (!PlayerPrefs.HasKey(RecoveryStartTimeKey))
        {
            return;
        }

        PlayerPrefs.DeleteKey(RecoveryStartTimeKey);
        PlayerPrefs.Save();
    }

    private static long GetCurrentUnixTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private static long GetTimestamp(string key, long fallback)
    {
        string savedValue = PlayerPrefs.GetString(key, fallback.ToString(CultureInfo.InvariantCulture));
        return long.TryParse(savedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long timestamp)
            ? timestamp
            : fallback;
    }

    private static void SetTimestamp(string key, long timestamp)
    {
        PlayerPrefs.SetString(key, timestamp.ToString(CultureInfo.InvariantCulture));
    }
}
