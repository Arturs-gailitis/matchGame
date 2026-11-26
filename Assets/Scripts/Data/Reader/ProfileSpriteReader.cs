using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>Reads profile avatar configuration data from a CSV TextAsset.</summary>
public static class ProfileSpriteReader
{
    public static List<ProfileSpriteConfig> Read(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            return new List<ProfileSpriteConfig>();
        }

        string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<ProfileSpriteConfig>(Mathf.Max(0, lines.Length - 1));

        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex].Trim();

            if (line.Length == 0)
            {
                continue;
            }

            string[] columns = line.Split(',');

            if (columns.Length != 3 || !int.TryParse(columns[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) || !TryParseUnlocked(columns[2], out bool isUnlocked))
            {
                Debug.LogWarning($"ProfileSpriteReader: Skipping invalid CSV row {lineIndex + 1}: {line}");
                continue;
            }

            result.Add(new ProfileSpriteConfig
            {
                id = id,
                imageName = columns[1].Trim(),
                isUnlocked = isUnlocked
            });
        }

        return result;
    }

    private static bool TryParseUnlocked(string value, out bool isUnlocked)
    {
        string normalizedValue = value.Trim();

        if (bool.TryParse(normalizedValue, out isUnlocked))
        {
            return true;
        }

        if (normalizedValue == "1" || normalizedValue == "0")
        {
            isUnlocked = normalizedValue == "1";
            return true;
        }

        isUnlocked = false;
        return false;
    }
}
