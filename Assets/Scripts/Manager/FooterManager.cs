using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates the configured navigation buttons in one equally sized footer row.
/// </summary>
public sealed class FooterManager : MonoBehaviour
{
    [SerializeField] private FooterSectionView sectionPrefab;
    [SerializeField] private LayoutType[] sectionTypes = {LayoutType.Shop, LayoutType.Lobby, LayoutType.Locked};
    [SerializeField] private Sprite shopIcon;
    [SerializeField] private Sprite lobbyIcon;
    [SerializeField] private Sprite lockedIcon;

    private readonly List<FooterSectionView> spawnedSections = new List<FooterSectionView>();

    public event Action<LayoutType> SectionSelected;

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
            Debug.LogError("FooterManager: FooterSection prefab is not assigned.", this);
            return;
        }

        foreach (LayoutType sectionType in sectionTypes)
        {
            FooterSectionView section = Instantiate(sectionPrefab, transform, false);
            section.name = $"FooterSection_{sectionType}";
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

    private void ConfigureSection(FooterSectionView section, LayoutType sectionType)
    {
        section.SetContent(GetIcon(sectionType), sectionType.ToString(), () => SelectSection(sectionType));
    }

    private Sprite GetIcon(LayoutType sectionType)
    {
        switch (sectionType)
        {
            case LayoutType.Shop:
                return shopIcon;
            case LayoutType.Lobby:
                return lobbyIcon;
            case LayoutType.Locked:
                return lockedIcon;
            default:
                Debug.LogWarning($"FooterManager: unsupported section type {sectionType}.", this);
                return null;
        }
    }

    private void SelectSection(LayoutType sectionType)
    {
        SectionSelected?.Invoke(sectionType);
    }

    private void ClearSections()
    {
        foreach (FooterSectionView section in spawnedSections)
        {
            if (section != null)
            {
                Destroy(section.gameObject);
            }
        }

        spawnedSections.Clear();
    }
}
