using UnityEngine;
using UnityEngine.UI;

/// <summary>Toggles all UI tap sounds and displays the current setting.</summary>
[DisallowMultipleComponent]
public sealed class SoundToggleButton : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private void OnEnable()
    {
        if (targetButton == null)
        {
            Debug.LogError("SoundToggleButton: Target Button is not assigned.", this);
            return;
        }

        targetButton.onClick.AddListener(ToggleSound);
        UpdateIcon();
    }

    private void OnDisable()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(ToggleSound);
        }
    }

    private void ToggleSound()
    {
        AudioSettingsStorage.SetSoundEnabled(!AudioSettingsStorage.IsSoundEnabled);
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (iconImage != null)
        {
            iconImage.sprite = AudioSettingsStorage.IsSoundEnabled ? soundOnSprite : soundOffSprite;
        }
    }
}
