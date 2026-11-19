using UnityEngine;
using UnityEngine.UI;

/// <summary>Creates and owns the Settings, Level Lost popup instances for GameLayout.</summary>
public sealed class GameLayoutController : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject settingsPopupPrefab;
    [SerializeField] private GameObject levelLostPopupPrefab;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private GameManager gameManager;

    private GameObject settingsPopupInstance;
    private GameObject levelLostPopupInstance;
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

        if (waitingAreaManager != null)
        {
            waitingAreaManager.LevelLost += ShowLevelLostPopup;
        }
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(ShowSettings);
        }

        if (waitingAreaManager != null)
        {
            waitingAreaManager.LevelLost -= ShowLevelLostPopup;
        }
    }

    private void ShowSettings()
    {
        if (levelLostPopupInstance != null && levelLostPopupInstance.activeSelf)
        {
            return;
        }

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

    private void ShowLevelLostPopup()
    {
        if (levelLostPopupInstance == null && levelLostPopupPrefab != null)
        {
            levelLostPopupInstance = Instantiate(levelLostPopupPrefab, transform, false);
            LevelLostPopupController popupController = levelLostPopupInstance.GetComponent<LevelLostPopupController>();

            if (popupController != null)
            {
                popupController.Initialize(layoutManager, gameManager);
            }
        }

        if (levelLostPopupInstance != null)
        {
            levelLostPopupInstance.SetActive(true);
        }
    }
}
