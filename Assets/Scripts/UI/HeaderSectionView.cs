using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays one icon and numeric value inside the header.
/// </summary>
public sealed class HeaderSectionView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text valueText;

    public void SetContent(Sprite sectionIcon, int value)
    {
        if (icon != null)
        {
            icon.sprite = sectionIcon;
            icon.enabled = sectionIcon != null;
        }

        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
    }
}
