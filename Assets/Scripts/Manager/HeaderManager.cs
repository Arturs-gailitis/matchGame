using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and updates the configured data sections in one header row.
/// </summary>
public sealed class HeaderManager : MonoBehaviour
{
    [SerializeField] private HeaderSectionView sectionPrefab;
    [SerializeField] private HeaderSectionType[] sectionTypes = { HeaderSectionType.Coin };
    [SerializeField] private Sprite coinIcon;

    private readonly List<HeaderSectionView> spawnedSections = new List<HeaderSectionView>();

    private void OnEnable()
    {
        if (spawnedSections.Count != sectionTypes.Length)
        {
            RebuildSections();
            return;
        }

        RefreshSections();
    }

    public void RebuildSections()
    {
        ClearSections();

        if (sectionPrefab == null)
        {
            Debug.LogError("HeaderManager: HeaderSection prefab is not assigned.", this);
            return;
        }

        foreach (HeaderSectionType sectionType in sectionTypes)
        {
            HeaderSectionView section = Instantiate(sectionPrefab, transform, false);
            section.name = "HeaderSection";
            spawnedSections.Add(section);
            ConfigureSection(section, sectionType);
        }
    }

    public void RefreshSections()
    {
        for (int index = 0; index < spawnedSections.Count; index++)
        {
            ConfigureSection(spawnedSections[index], sectionTypes[index]);
        }
    }

    private void ConfigureSection(HeaderSectionView section, HeaderSectionType sectionType)
    {
        switch (sectionType)
        {
            case HeaderSectionType.Coin:
                section.SetContent(coinIcon, PlayerStorage.GetCoins());
                break;
            default:
                Debug.LogWarning($"HeaderManager: unsupported section type {sectionType}.", this);
                section.SetContent(null, 0);
                break;
        }
    }

    private void ClearSections()
    {
        foreach (HeaderSectionView section in spawnedSections)
        {
            if (section != null)
            {
                Destroy(section.gameObject);
            }
        }

        spawnedSections.Clear();
    }
}
