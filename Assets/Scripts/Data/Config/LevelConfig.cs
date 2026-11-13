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
/// Level layout data loaded from a level JSON file.
/// </summary>
[Serializable]
public sealed class LevelData
{
    public int level;
    public int columns;
    public int rows;
    public LevelCarData[] cars;
}
