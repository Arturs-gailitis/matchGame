using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loads the player's saved level and passes it to the TableManager.
/// </summary>
[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private TableManager tableManager;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private Text levelText;

    private int currentLevel;

    private void Start()
    {
        tableManager.LevelCompleted += HandleLevelCompleted;
        waitingAreaManager.RestartLevelRequested += HandleRestartLevelRequested;

        currentLevel = PlayerStorage.GetCurrentLevel();
        UpdateLevelText();
        tableManager.SetLevel(currentLevel);
    }

    private void OnDestroy()
    {
        if (tableManager != null)
        {
            tableManager.LevelCompleted -= HandleLevelCompleted;
            waitingAreaManager.RestartLevelRequested -= HandleRestartLevelRequested;
        }
    }

    private void HandleLevelCompleted()
    {
        currentLevel++;
        PlayerStorage.SetCurrentLevel(currentLevel);
        UpdateLevelText();
        tableManager.SetLevel(currentLevel);
    }

    private void HandleRestartLevelRequested()
    {
        waitingAreaManager.ClearCars();
        tableManager.SetLevel(currentLevel);
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
    }
}
