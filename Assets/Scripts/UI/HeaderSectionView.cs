using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays one icon and numeric value inside the header.
/// </summary>
public sealed class HeaderSectionView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text valueText;
    [SerializeField] private Text energyCountText;

    public void SetContent(Sprite sectionIcon, int value)
    {
        SetContent(sectionIcon, value.ToString());
    }

    public void SetContent(Sprite sectionIcon, string value)
    {
        if (icon != null)
        {
            icon.sprite = sectionIcon;
            icon.enabled = sectionIcon != null;
        }

        if (valueText != null)
        {
            valueText.text = value;
        }

        if (energyCountText != null)
        {
            energyCountText.gameObject.SetActive(false);
        }
    }

    public void SetEnergyContent(Sprite sectionIcon, int energy, string recoveryTime)
    {
        SetContent(sectionIcon, recoveryTime);

        if (energyCountText != null)
        {
            energyCountText.text = energy.ToString();
            energyCountText.gameObject.SetActive(true);
        }
    }
}
