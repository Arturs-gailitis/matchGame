using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


/// <summary>
/// Reads car sprite configuration data from the CSV TextAsset.
/// Assign Assets/Data/Config/CarSprites.csv as a TextAsset in the caller.
/// </summary>
public static class CarSpriteReader
{
    public static List<CarSpriteConfig> Read(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            return new List<CarSpriteConfig>();
        }

        string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<CarSpriteConfig>(Mathf.Max(0, lines.Length - 1));

        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex].Trim();

            if (line.Length == 0)
            {
                continue;
            }

            string[] columns = line.Split(',');

            if (columns.Length != 3 || !int.TryParse(columns[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) || !int.TryParse(columns[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int type))
            {
                Debug.LogWarning($"CarSpriteCsvReader: Skipping invalid CSV row {lineIndex + 1}: {line}");
                continue;
            }

            result.Add(new CarSpriteConfig
            {
                id = id,
                spriteName = columns[1].Trim(),
                type = type
            });
        }

        return result;
    }
}
