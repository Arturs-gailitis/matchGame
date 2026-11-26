using System;

/// <summary>A single profile avatar entry from ProfileSprites.csv.</summary>
[Serializable]
public sealed class ProfileSpriteConfig
{
    public int id;
    public string imageName;
    public bool isUnlocked;
}
