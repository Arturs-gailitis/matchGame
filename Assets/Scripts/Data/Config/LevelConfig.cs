using System;

/// <summary>
/// A car placement entry from a level JSON file.
/// </summary>
[Serializable]
public sealed class LevelCarData
{
    public int id;
    public int x;
    public int y;
    public int type;
}

/// <summary>
/// A table position used by level obstacles.
/// </summary>
[Serializable]
public sealed class LevelPositionData
{
    public int x;
    public int y;
}

/// <summary>
/// A pipe that hides its blocked cars until the key car is selected.
/// </summary>
[Serializable]
public sealed class LevelPipeData
{
    public string color;
    public LevelPositionData[] blockedPositions;
    public LevelPositionData keyPosition;
}

/// <summary>
/// Level layout data loaded from a level JSON file.
/// </summary>
[Serializable]
public sealed class LevelData
{
    public int level;
    public int columns;
    public int rows;
    public LevelCarData[] cars;
    public LevelPipeData[] pipes;
}
