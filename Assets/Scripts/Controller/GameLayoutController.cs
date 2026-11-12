using UnityEngine;
using UnityEngine.UI;

/// <summary>Creates and owns the Settings popup instance for GameLayout.</summary>
public sealed class GameLayoutController : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject settingsPopupPrefab;

    private GameObject settingsPopupInstance;
    private LayoutManager layoutManager;

    public void Initialize(LayoutManager manager)
    {
        layoutManager = manager;
    }

    private void Awake()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(ShowSettings);
        }
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(ShowSettings);
        }
    }

    private void ShowSettings()
    {
        if (settingsPopupInstance == null && settingsPopupPrefab != null)
        {
            settingsPopupInstance = Instantiate(settingsPopupPrefab, transform, false);
            SettingsPopupController settingsController = settingsPopupInstance.GetComponent<SettingsPopupController>();

            if (settingsController != null)
            {
                settingsController.Initialize(layoutManager);
            }
        }

        if (settingsPopupInstance != null)
        {
            settingsPopupInstance.SetActive(true);
        }
    }
}
