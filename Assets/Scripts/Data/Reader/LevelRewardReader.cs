using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Reads level reward data from the LevelReward.csv TextAsset.
/// Assign Assets/Data/Config/LevelReward.csv as a TextAsset in the caller.
/// </summary>
public static class LevelRewardReader
{
    public static List<LevelRewardConfig> Read(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            return new List<LevelRewardConfig>();
        }

        string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<LevelRewardConfig>(Mathf.Max(0, lines.Length - 1));

        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex].Trim();

            if (line.Length == 0)
            {
                continue;
            }

            string[] columns = line.Split(',');

            if (columns.Length != 2 || !int.TryParse(columns[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) || !int.TryParse(columns[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int coins) || level < 1 || coins < 0)
            {
                Debug.LogWarning($"LevelRewardReader: Skipping invalid CSV row {lineIndex + 1}: {line}");
                continue;
            }

            result.Add(new LevelRewardConfig
            {
                level = level,
                coins = coins
            });
        }

        return result;
    }

    public static bool TryGetReward(string csvText, int level, out LevelRewardConfig reward)
    {
        foreach (LevelRewardConfig entry in Read(csvText))
        {
            if (entry.level == level)
            {
                reward = entry;
                return true;
            }
        }

        reward = null;
        return false;
    }
}
