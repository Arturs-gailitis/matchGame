using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Lets the player select a waiting car and removes a three-car group of the same type.
/// Waiting cars are preferred; any missing cars are taken from the nearest level cells.
/// </summary>
public sealed class MatchesPowerUp : MonoBehaviour
{
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private TableManager tableManager;

    private readonly List<CarInteraction> selectableCars = new List<CarInteraction>();
    private bool isSelecting;
    private bool isProcessing;

    private void Awake()
    {
        ResolveManagers();
    }

    private void OnDisable()
    {
        StopSelection();
    }

    public void Activate()
    {
        if (isProcessing)
        {
            return;
        }

        if (isSelecting)
        {
            StopSelection();
            return;
        }

        ResolveManagers();

        if (waitingAreaManager == null || tableManager == null || tableManager.IsCarRemovalInProgress)
        {
            return;
        }

        selectableCars.AddRange(waitingAreaManager.GetWaitingCars());

        if (selectableCars.Count == 0)
        {
            return;
        }

        isSelecting = true;
        WaitingCarPulseAnimation.Play(selectableCars);

        foreach (CarInteraction car in selectableCars)
        {
            if (car != null)
            {
                car.WaitingSelectionRequested += HandleWaitingCarSelected;
            }
        }
    }

    private void HandleWaitingCarSelected(CarInteraction selectedCar)
    {
        if (!isSelecting || isProcessing || selectedCar == null || selectedCar.State != CarState.Waiting)
        {
            return;
        }

        StopSelection();
        RemoveMatchingCars(selectedCar).Forget();
    }

    private async UniTask RemoveMatchingCars(CarInteraction selectedCar)
    {
        isProcessing = true;

        List<CarInteraction> waitingCars = waitingAreaManager.GetWaitingCarsByType(selectedCar.CarType);
        int levelCarCount = Mathf.Max(0, 3 - waitingCars.Count);
        List<CarInteraction> levelCars = tableManager.TakeNearestCarsForPowerUp(selectedCar.CarType, levelCarCount, selectedCar.transform.position);

        var carsToRemove = new List<CarInteraction>(waitingCars.Count + levelCars.Count);
        carsToRemove.AddRange(waitingCars);
        carsToRemove.AddRange(levelCars);

        foreach (CarInteraction car in carsToRemove)
        {
            if (car != null)
            {
                car.SetState(CarState.Matched);
            }
        }

        await WaitingAreaMatchAnimation.PlayAsync(carsToRemove);

        if (this == null || waitingAreaManager == null || tableManager == null)
        {
            return;
        }

        foreach (CarInteraction levelCar in levelCars)
        {
            if (levelCar != null)
            {
                Destroy(levelCar.gameObject);
            }
        }

        await waitingAreaManager.RemoveCarsForPowerUp(waitingCars);
        tableManager.NotifyPowerUpRemovalCompleted();
        isProcessing = false;
    }

    private void StopSelection()
    {
        WaitingCarPulseAnimation.Stop(selectableCars);

        foreach (CarInteraction car in selectableCars)
        {
            if (car != null)
            {
                car.WaitingSelectionRequested -= HandleWaitingCarSelected;
            }
        }

        selectableCars.Clear();
        isSelecting = false;
    }

    private void ResolveManagers()
    {
        Transform searchRoot = transform.parent != null ? transform.parent : transform.root;

        if (waitingAreaManager == null)
        {
            waitingAreaManager = searchRoot.GetComponentInChildren<WaitingAreaManager>(true);
        }

        if (tableManager == null)
        {
            tableManager = searchRoot.GetComponentInChildren<TableManager>(true);
        }
    }
}
