using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Displays and controls one navigation button in the footer.
/// </summary>
public sealed class FooterSectionView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text label;
    [SerializeField] private Button button;

    private UnityAction clickHandler;

    public void SetContent(Sprite sectionIcon, string sectionLabel, UnityAction onClick)
    {
        if (icon != null)
        {
            icon.sprite = sectionIcon;
            icon.enabled = sectionIcon != null;
        }

        if (label != null)
        {
            label.text = sectionLabel;
        }

        if (button == null)
        {
            return;
        }

        if (clickHandler != null)
        {
            button.onClick.RemoveListener(clickHandler);
        }

        clickHandler = onClick;

        if (clickHandler != null)
        {
            button.onClick.AddListener(clickHandler);
        }
    }

    private void OnDestroy()
    {
        if (button != null && clickHandler != null)
        {
            button.onClick.RemoveListener(clickHandler);
        }
    }
}
