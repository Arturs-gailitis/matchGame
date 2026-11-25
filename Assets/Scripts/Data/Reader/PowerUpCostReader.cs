using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Reads power-up cost data from the PowerUpCost.csv TextAsset.
/// </summary>
public static class PowerUpCostReader
{
    public static List<PowerUpCostConfig> Read(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            return new List<PowerUpCostConfig>();
        }

        string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<PowerUpCostConfig>(Mathf.Max(0, lines.Length - 1));

        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex].Trim();

            if (line.Length == 0)
            {
                continue;
            }

            string[] columns = line.Split(',');

            if (columns.Length != 3 || !int.TryParse(columns[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) || string.IsNullOrWhiteSpace(columns[1]) || !int.TryParse(columns[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int cost) || id < 0 || cost < 0)
            {
                Debug.LogWarning($"PowerUpCostReader: Skipping invalid CSV row {lineIndex + 1}: {line}");
                continue;
            }

            result.Add(new PowerUpCostConfig
            {
                id = id,
                powerUpName = columns[1].Trim(),
                cost = cost
            });
        }

        return result;
    }

    public static bool TryGetCost(string csvText, PowerUpType powerUpType, out PowerUpCostConfig config)
    {
        int powerUpId = (int)powerUpType;

        foreach (PowerUpCostConfig entry in Read(csvText))
        {
            if (entry.id == powerUpId)
            {
                config = entry;
                return true;
            }
        }

        config = null;
        return false;
    }
}
