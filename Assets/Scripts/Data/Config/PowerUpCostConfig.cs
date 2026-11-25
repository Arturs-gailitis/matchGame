using System;

/// <summary>
/// A power-up cost entry from PowerUpCost.csv.
/// </summary>
[Serializable]
public sealed class PowerUpCostConfig
{
    public int id;
    public string powerUpName;
    public int cost;
}
