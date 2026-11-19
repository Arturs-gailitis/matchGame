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
    private int pendingNextLevel;

    private void Start()
    {
        tableManager.LevelCompleted += HandleLevelCompleted;

        currentLevel = PlayerStorage.GetCurrentLevel();
        UpdateLevelText();
        tableManager.SetLevel(currentLevel);
    }

    private void OnDestroy()
    {
        if (tableManager != null)
        {
            tableManager.LevelCompleted -= HandleLevelCompleted;
        }
    }

    private void HandleLevelCompleted()
    {
        pendingNextLevel = currentLevel + 1;
    }

    public void ContinueToNextLevel()
    {
        if (!TryCommitNextLevel())
        {
            return;
        }

        UpdateLevelText();
        tableManager.SetLevel(currentLevel);
    }

    public bool TryCommitNextLevel()
    {
        if (pendingNextLevel <= currentLevel)
        {
            Debug.LogWarning("GameManager: no completed level is waiting to be continued.", this);
            return false;
        }

        currentLevel = pendingNextLevel;
        pendingNextLevel = 0;
        PlayerStorage.SetCurrentLevel(currentLevel);
        return true;
    }

    public void RestartLevel()
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
