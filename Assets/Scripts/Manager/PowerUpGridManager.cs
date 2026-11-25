using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and manages the power-up buttons displayed in the game board.
/// </summary>
public sealed class PowerUpGridManager : MonoBehaviour
{
    [SerializeField] private PowerUpView powerUpPrefab;
    [SerializeField] private PowerUpType[] powerUpTypes = {PowerUpType.Matches, PowerUpType.Reset};
    [SerializeField] private Sprite matchesIcon;
    [SerializeField] private Sprite resetIcon;
    [SerializeField] private MatchesPowerUp matchesPowerUp;
    [SerializeField] private ResetPowerUp resetPowerUp;
    [SerializeField] private TextAsset powerUpCostConfig;

    private readonly List<PowerUpView> spawnedPowerUps = new List<PowerUpView>();
    private float nextRefreshTime;

    private void OnEnable()
    {
        if (spawnedPowerUps.Count != powerUpTypes.Length)
        {
            RebuildPowerUps();
            return;
        }

        RefreshPowerUps();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
        {
            return;
        }

        nextRefreshTime = Time.unscaledTime + 0.25f;
        RefreshPowerUps();
    }

    public void RebuildPowerUps()
    {
        ClearPowerUps();

        if (powerUpPrefab == null)
        {
            Debug.LogError("PowerUpGridManager: PowerUp prefab is not assigned.", this);
            return;
        }

        for (int index = 0; index < powerUpTypes.Length; index++)
        {
            PowerUpView powerUp = Instantiate(powerUpPrefab, transform, false);
            PowerUpType powerUpType = powerUpTypes[index];
            powerUp.name = $"PowerUp_{powerUpType}";
            spawnedPowerUps.Add(powerUp);
            ConfigurePowerUp(powerUp, powerUpType);
        }
    }

    public void RefreshPowerUps()
    {
        for (int index = 0; index < spawnedPowerUps.Count; index++)
        {
            ConfigurePowerUp(spawnedPowerUps[index], powerUpTypes[index]);
        }
    }

    private void ConfigurePowerUp(PowerUpView powerUp, PowerUpType powerUpType)
    {
        string costText = "N/A";
        bool canAfford = false;

        if (powerUpCostConfig != null && PowerUpCostReader.TryGetCost(powerUpCostConfig.text, powerUpType, out PowerUpCostConfig costConfig))
        {
            costText = costConfig.cost.ToString();
            canAfford = PlayerStorage.GetCoins() >= costConfig.cost;
        }

        powerUp.SetContent(GetIcon(powerUpType), costText, canAfford, () => HandlePowerUpSelected(powerUpType));
    }

    private void HandlePowerUpSelected(PowerUpType powerUpType)
    {
        if (powerUpType == PowerUpType.Matches && matchesPowerUp != null && matchesPowerUp.IsSelecting)
        {
            matchesPowerUp.TryActivate();
            return;
        }

        if (powerUpCostConfig == null || !PowerUpCostReader.TryGetCost(powerUpCostConfig.text, powerUpType, out PowerUpCostConfig costConfig))
        {
            Debug.LogWarning($"PowerUpGridManager: no cost is configured for {powerUpType}.", this);
            return;
        }

        if (PlayerStorage.GetCoins() < costConfig.cost)
        {
            Debug.Log($"PowerUpGridManager: not enough coins for {costConfig.powerUpName}. Cost: {costConfig.cost}.", this);
            return;
        }

        bool activated = false;

        switch (powerUpType)
        {
            case PowerUpType.Matches:
                if (matchesPowerUp != null)
                {
                    matchesPowerUp.TryActivate(() => PlayerStorage.TrySpendCoins(costConfig.cost));
                }
                break;
            case PowerUpType.Reset:
                if (resetPowerUp != null)
                {
                    activated = resetPowerUp.TryActivate();
                }
                break;
        }

        if (activated && PlayerStorage.TrySpendCoins(costConfig.cost))
        {
            Debug.Log($"PowerUpGridManager: activated {costConfig.powerUpName} for {costConfig.cost} coins. New balance: {PlayerStorage.GetCoins()}.", this);
        }
    }

    private Sprite GetIcon(PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUpType.Matches:
                return matchesIcon;
            case PowerUpType.Reset:
                return resetIcon;
            default:
                return null;
        }
    }

    private void ClearPowerUps()
    {
        foreach (PowerUpView powerUp in spawnedPowerUps)
        {
            if (powerUp != null)
            {
                Destroy(powerUp.gameObject);
            }
        }

        spawnedPowerUps.Clear();
    }
}
