using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and manages the power-up buttons displayed in the game board.
/// </summary>
public sealed class PowerUpGridManager : MonoBehaviour
{
    [SerializeField] private PowerUpView powerUpPrefab;
    [SerializeField] private PowerUpType[] powerUpTypes = {PowerUpType.Matches};
    [SerializeField] private Sprite matchesIcon;
    [SerializeField] private MatchesPowerUp matchesPowerUp;

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
        powerUp.SetContent(GetIcon(), () => HandlePowerUpSelected(powerUpType));
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
        }
    }

    private Sprite GetIcon()
    {
        return matchesIcon;
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
