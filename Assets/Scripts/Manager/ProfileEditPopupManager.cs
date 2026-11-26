using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Manages the profile editor popup and its prepared avatar slots.</summary>
public sealed class ProfileEditPopupManager : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button dimmerButton;
    [SerializeField] private Transform avatarGrid;
    [SerializeField] private Button avatarSlotTemplate;
    [SerializeField] private Image profilePreview;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private TextAsset profileSpriteConfig;

    private readonly List<Sprite> runtimeSprites = new List<Sprite>();

    public event Action<string> ProfileSelected;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (dimmerButton != null)
        {
            dimmerButton.onClick.AddListener(Close);
        }

        if (playerNameInput != null)
        {
            playerNameInput.SetTextWithoutNotify(PlayerStorage.GetPlayerName());
            playerNameInput.onEndEdit.AddListener(SavePlayerName);
        }

        BuildAvatarPlaceholders();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }

        if (dimmerButton != null)
        {
            dimmerButton.onClick.RemoveListener(Close);
        }

        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.RemoveListener(SavePlayerName);
        }

        foreach (Sprite sprite in runtimeSprites)
        {
            if (sprite != null)
            {
                Destroy(sprite);
            }
        }

        runtimeSprites.Clear();
    }

    private void BuildAvatarPlaceholders()
    {
        if (avatarGrid == null || avatarSlotTemplate == null || profileSpriteConfig == null)
        {
            Debug.LogError("ProfileEditPopup: grid, slot template, or profile config is not assigned.", this);
            return;
        }

        avatarSlotTemplate.gameObject.SetActive(false);
        List<ProfileSpriteConfig> profiles = ProfileSpriteReader.Read(profileSpriteConfig.text);
        string selectedImageName = PlayerStorage.GetProfileImageName();
        Sprite fallbackSprite = null;
        string fallbackImageName = null;
        bool selectedProfileFound = false;

        foreach (ProfileSpriteConfig profile in profiles)
        {
            Button slot = Instantiate(avatarSlotTemplate, avatarGrid, false);
            slot.name = $"Profile_{profile.id}_{profile.imageName}";
            slot.interactable = profile.isUnlocked;

            Texture2D texture = Resources.Load<Texture2D>($"Profiles/{profile.imageName}");

            if (texture == null)
            {
                Debug.LogWarning($"ProfileEditPopup: profile image '{profile.imageName}' was not found.", this);
            }
            else
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                sprite.name = profile.imageName;
                runtimeSprites.Add(sprite);
                slot.image.sprite = sprite;
                slot.image.type = Image.Type.Simple;
                slot.image.preserveAspect = true;

                if (profile.isUnlocked)
                {
                    string imageName = profile.imageName;
                    Sprite selectedSprite = sprite;
                    slot.onClick.AddListener(() => SelectProfile(imageName, selectedSprite));

                    if (fallbackSprite == null)
                    {
                        fallbackSprite = sprite;
                        fallbackImageName = imageName;
                    }

                    if (imageName == selectedImageName)
                    {
                        SetPreview(sprite);
                        selectedProfileFound = true;
                    }
                }
            }

            slot.gameObject.SetActive(true);
        }

        if (!selectedProfileFound && fallbackSprite != null)
        {
            SetPreview(fallbackSprite);
            PlayerStorage.SetProfileImageName(fallbackImageName);
        }
    }

    private void SelectProfile(string imageName, Sprite sprite)
    {
        SetPreview(sprite);
        PlayerStorage.SetProfileImageName(imageName);
        ProfileSelected?.Invoke(imageName);
    }

    private void SetPreview(Sprite sprite)
    {
        if (profilePreview == null)
        {
            return;
        }

        profilePreview.sprite = sprite;
        profilePreview.type = Image.Type.Simple;
        profilePreview.preserveAspect = true;
    }

    private void Close()
    {
        if (playerNameInput != null)
        {
            SavePlayerName(playerNameInput.text);
        }

        gameObject.SetActive(false);
    }

    private void SavePlayerName(string playerName)
    {
        PlayerStorage.SetPlayerName(playerName);

        if (playerNameInput != null)
        {
            playerNameInput.SetTextWithoutNotify(PlayerStorage.GetPlayerName());
        }
    }
}
