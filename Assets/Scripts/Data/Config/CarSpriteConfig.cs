using System;

/// <summary>
/// A single car sprite entry from CarSprites.csv.
/// </summary>
[Serializable]
public sealed class CarSpriteConfig
{
    public int id;
    public string spriteName;
    public int type;
}