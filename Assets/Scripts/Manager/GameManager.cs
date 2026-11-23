using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loads the player's saved level, passes it to the TableManager, and awards
/// the configured coin reward when a level is completed.
/// </summary>
[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private TableManager tableManager;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private Text levelText;
    [SerializeField] private TextAsset levelRewardConfig;

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
        AddLevelReward();
        pendingNextLevel = currentLevel + 1;
    }

    private void AddLevelReward()
    {

        if (!LevelRewardReader.TryGetReward(levelRewardConfig.text, currentLevel, out LevelRewardConfig reward))
        {
            Debug.LogWarning($"GameManager: no reward is configured for level {currentLevel}.", this);
            return;
        }

        int newBalance = PlayerStorage.AddCoins(reward.coins);
        Debug.Log($"GameManager: level {currentLevel} completed. Added {reward.coins} coins. New balance: {newBalance}.", this);
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
