using System;

/// <summary>
/// A level reward entry from LevelReward.csv.
/// </summary>
[Serializable]
public sealed class LevelRewardConfig
{
    public int level;
    public int coins;
}
