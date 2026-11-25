using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Displays one power-up entry in the power-up grid.
/// </summary>
public sealed class PowerUpView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text costText;
    [SerializeField] private Button button;

    private UnityAction clickHandler;

    public void SetContent(Sprite powerUpIcon, UnityAction onClick)
    {
        if (icon != null)
        {
            icon.sprite = powerUpIcon;
            icon.enabled = powerUpIcon != null;
        }

        if (button != null)
        {
            button.interactable = true;

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
    }

    private void OnDestroy()
    {
        if (button != null && clickHandler != null)
        {
            button.onClick.RemoveListener(clickHandler);
        }
    }
}
