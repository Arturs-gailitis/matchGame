using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates and updates the configured data sections in one header row.
/// </summary>
public sealed class HeaderManager : MonoBehaviour
{
    [SerializeField] private HeaderSectionView sectionPrefab;
    [SerializeField] private Button profilePrefab;
    [SerializeField] private GameObject profileEditPopupPrefab;
    [SerializeField] private HeaderSectionType[] sectionTypes = {HeaderSectionType.Profile, HeaderSectionType.Coin, HeaderSectionType.Energy};
    [SerializeField] private Sprite coinIcon;
    [SerializeField] private Sprite energyIcon;

    private readonly List<GameObject> spawnedSections = new List<GameObject>();
    private GameObject profileEditPopupInstance;
    private ProfileEditPopupManager profileEditPopupManager;
    private Image profileButtonImage;
    private Sprite profileButtonSprite;
    private float nextRefreshTime;

    private void OnEnable()
    {
        if (spawnedSections.Count != sectionTypes.Length)
        {
            RebuildSections();
            return;
        }

        RefreshSections();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
        {
            return;
        }

        nextRefreshTime = Time.unscaledTime + 0.25f;
        RefreshSections();
    }

    public void RebuildSections()
    {
        ClearSections();

        foreach (HeaderSectionType sectionType in sectionTypes)
        {
            spawnedSections.Add(CreateSection(sectionType));
        }
    }

    public void RefreshSections()
    {
        for (int index = 0; index < spawnedSections.Count; index++)
        {
            GameObject sectionObject = spawnedSections[index];
            HeaderSectionType sectionType = sectionTypes[index];

            if (sectionObject == null || sectionType == HeaderSectionType.Profile)
            {
                continue;
            }

            HeaderSectionView section = sectionObject.GetComponent<HeaderSectionView>();
            
            if (section != null)
            {
                ConfigureSection(section, sectionType);
            }
        }
    }

    private GameObject CreateSection(HeaderSectionType sectionType)
    {
        if (sectionType == HeaderSectionType.Profile)
        {
            if (profilePrefab == null)
            {
                Debug.LogError("HeaderManager: Profile prefab is not assigned.", this);
                return null;
            }

            Button profile = Instantiate(profilePrefab, transform, false);
            profile.name = "profile";
            profile.onClick.AddListener(ShowProfileEditPopup);
            profileButtonImage = profile.image;
            SetProfileButtonImage(PlayerStorage.GetProfileImageName());
            return profile.gameObject;
        }

        if (sectionPrefab == null)
        {
            Debug.LogError("HeaderManager: HeaderSection prefab is not assigned.", this);
            return null;
        }

        HeaderSectionView section = Instantiate(sectionPrefab, transform, false);
        section.name = "HeaderSection";
        ConfigureSection(section, sectionType);
        return section.gameObject;
    }

    private void ShowProfileEditPopup()
    {
        if (profileEditPopupInstance == null)
        {
            if (profileEditPopupPrefab == null)
            {
                Debug.LogError("HeaderManager: ProfileEditPopup prefab is not assigned.", this);
                return;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            Transform popupParent = parentCanvas != null ? parentCanvas.transform : transform.root;
            profileEditPopupInstance = Instantiate(profileEditPopupPrefab, popupParent, false);
            profileEditPopupInstance.name = profileEditPopupPrefab.name;
            profileEditPopupManager = profileEditPopupInstance.GetComponent<ProfileEditPopupManager>();

            if (profileEditPopupManager != null)
            {
                profileEditPopupManager.ProfileSelected += SetProfileButtonImage;
            }
        }

        profileEditPopupInstance.SetActive(true);
        profileEditPopupInstance.transform.SetAsLastSibling();
    }

    private void SetProfileButtonImage(string imageName)
    {
        if (profileButtonImage == null)
        {
            return;
        }

        Texture2D texture = Resources.Load<Texture2D>($"Profiles/{imageName}");

        if (texture == null)
        {
            Debug.LogWarning($"HeaderManager: profile image '{imageName}' was not found.", this);
            return;
        }

        if (profileButtonSprite != null)
        {
            Destroy(profileButtonSprite);
        }

        profileButtonSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        profileButtonSprite.name = imageName;
        profileButtonImage.sprite = profileButtonSprite;
        profileButtonImage.type = Image.Type.Simple;
        profileButtonImage.preserveAspect = true;
    }

    private void ConfigureSection(HeaderSectionView section, HeaderSectionType sectionType)
    {
        switch (sectionType)
        {
            case HeaderSectionType.Coin:
                section.SetContent(coinIcon, PlayerStorage.GetCoins());
                break;
            case HeaderSectionType.Energy:
                int energy = PlayerEnergyStorage.GetEnergy();
                int seconds = PlayerEnergyStorage.GetSecondsUntilNextEnergy();
                string recoveryTime = energy >= PlayerEnergyStorage.MaxEnergy ? "MAX" : $"{seconds / 60:00}:{seconds % 60:00}";
                section.SetEnergyContent(energyIcon, energy, recoveryTime);
                break;
            default:
                Debug.LogWarning($"HeaderManager: unsupported section type {sectionType}.", this);
                section.SetContent(null, 0);
                break;
        }
    }

    private void ClearSections()
    {
        if (profileButtonSprite != null)
        {
            Destroy(profileButtonSprite);
            profileButtonSprite = null;
        }

        profileButtonImage = null;

        foreach (GameObject section in spawnedSections)
        {
            if (section != null)
            {
                Destroy(section);
            }
        }

        spawnedSections.Clear();
    }

    private void OnDestroy()
    {
        if (profileEditPopupManager != null)
        {
            profileEditPopupManager.ProfileSelected -= SetProfileButtonImage;
        }

        if (profileButtonSprite != null)
        {
            Destroy(profileButtonSprite);
        }
    }
}
