using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads level layout data from a JSON TextAsset or a numbered level file.
/// </summary>
public static class LevelJsonReader
{
    public static LevelData Read(int levelNumber)
    {
        if (levelNumber <= 0)
        {
            Debug.LogError($"LevelJsonReader: level number must be greater than zero, received {levelNumber}.");
            return null;
        }

        string filePath = Path.Combine(Application.dataPath, "Data", "Levels", $"Level{levelNumber}.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"LevelJsonReader: level JSON was not found: {filePath}");
            return null;
        }

        return Read(File.ReadAllText(filePath));
    }

    private static LevelData Read(string jsonText)
    {

        LevelData level = JsonUtility.FromJson<LevelData>(jsonText);

        if (level == null)
        {
            Debug.LogError("LevelJsonReader: level JSON could not be parsed.");
            return null;
        }

        level.cars ??= Array.Empty<LevelCarData>();

        return level;
    }
}

