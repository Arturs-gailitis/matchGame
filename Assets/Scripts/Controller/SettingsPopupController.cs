using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns all actions belonging to the Settings popup.</summary>
public sealed class SettingsPopupController : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button exitButton;

    private LayoutManager layoutManager;

    public void Initialize(LayoutManager manager)
    {
        layoutManager = manager;
    }

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitToLobby);
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettings);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitToLobby);
        }
    }

    private void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    private void ExitToLobby()
    {
        if (layoutManager == null)
        {
            Debug.LogError("SettingsPopup must be initialized by a LayoutManager.", this);
            return;
        }

        layoutManager.ShowLayout(LayoutManager.LayoutType.Lobby);
    }
}
