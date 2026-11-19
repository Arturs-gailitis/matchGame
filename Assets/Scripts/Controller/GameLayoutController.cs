using UnityEngine;
using UnityEngine.UI;

/// <summary>Creates and owns the GameLayout popup instances.</summary>
public sealed class GameLayoutController : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject settingsPopupPrefab;
    [SerializeField] private GameObject levelLostPopupPrefab;
    [SerializeField] private GameObject levelWonPopupPrefab;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private TableManager tableManager;
    [SerializeField] private GameManager gameManager;

    private GameObject settingsPopupInstance;
    private GameObject levelLostPopupInstance;
    private GameObject levelWonPopupInstance;
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

        if (tableManager != null)
        {
            tableManager.LevelCompleted += ShowLevelWonPopup;
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

        if (tableManager != null)
        {
            tableManager.LevelCompleted -= ShowLevelWonPopup;
        }
    }

    private void ShowSettings()
    {
        if ((levelLostPopupInstance != null && levelLostPopupInstance.activeSelf) || (levelWonPopupInstance != null && levelWonPopupInstance.activeSelf))
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

    private void ShowLevelWonPopup() 
    {
        if (levelWonPopupInstance == null && levelWonPopupPrefab != null)
        {
            levelWonPopupInstance = Instantiate(levelWonPopupPrefab, transform, false);
            LevelWonPopupController popupController = levelWonPopupInstance.GetComponent<LevelWonPopupController>();

            if (popupController != null)
            {
                popupController.Initialize(layoutManager, gameManager);
            }
        }

        if (levelWonPopupInstance != null)
        {
            levelWonPopupInstance.SetActive(true);
        }
    }
}
