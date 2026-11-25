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

    private readonly List<PowerUpView> spawnedPowerUps = new List<PowerUpView>();

    private void OnEnable()
    {
        if (spawnedPowerUps.Count != powerUpTypes.Length)
        {
            RebuildPowerUps();
            return;
        }

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
        powerUp.SetContent(GetIcon(powerUpType), () => HandlePowerUpSelected(powerUpType));
    }

    private void HandlePowerUpSelected(PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUpType.Matches:
                if (matchesPowerUp != null)
                {
                    matchesPowerUp.Activate();
                }
                break;
            case PowerUpType.Reset:
                if (resetPowerUp != null)
                {
                    resetPowerUp.Activate();
                }
                break;
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
