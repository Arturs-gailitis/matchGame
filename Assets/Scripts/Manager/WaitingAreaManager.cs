using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Owns the waiting slots and moves removable cars into the first free slot.
/// </summary>
public sealed class WaitingAreaManager : MonoBehaviour
{
    [SerializeField] private Transform waitingArea;
    [SerializeField, Min(1)] private int regularSlotCount = 7;
    [SerializeField, Range(0.5f, 1f)] private float carScale = 0.8f;

    public event Action BecameEmpty;
    public event Action LevelLost;

    private bool restartRequestPending;
    private bool isStorageMoveInProgress;

    public bool IsEmpty => waitingArea != null && waitingArea.GetComponentInChildren<CarInteraction>(true) == null;
    private int RegularSlotCount => waitingArea == null ? 0 : Mathf.Min(regularSlotCount, waitingArea.childCount);
    public bool HasFreeSlot
    {
        get
        {
            if (waitingArea == null || isStorageMoveInProgress)
            {
                return false;
            }

            for (int index = 0; index < RegularSlotCount; index++)
            {
                if (waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true) == null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public List<CarInteraction> GetWaitingCars()
    {
        var waitingCars = new List<CarInteraction>();

        if (waitingArea == null)
        {
            return waitingCars;
        }

        for (int index = 0; index < RegularSlotCount; index++)
        {
            CarInteraction car = waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true);

            if (car != null && car.State == CarState.Waiting)
            {
                waitingCars.Add(car);
            }
        }

        return waitingCars;
    }

    public List<CarInteraction> GetWaitingCarsByType(int carType)
    {
        List<CarInteraction> waitingCars = GetWaitingCars();
        waitingCars.RemoveAll(car => car == null || car.CarType != carType);
        return waitingCars;
    }

    public async UniTask RemoveCarsForPowerUp(IReadOnlyList<CarInteraction> carsToRemove)
    {
        if (waitingArea == null || carsToRemove == null || carsToRemove.Count == 0)
        {
            return;
        }

        var remainingCars = new List<CarInteraction>();
        var removedCars = new HashSet<CarInteraction>(carsToRemove);

        for (int index = 0; index < RegularSlotCount; index++)
        {
            CarInteraction car = waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true);

            if (car == null)
            {
                continue;
            }

            if (removedCars.Contains(car))
            {
                Destroy(car.gameObject);
            }
            else if (car.State == CarState.Waiting)
            {
                remainingCars.Add(car);
            }
        }

        await UniTask.Yield();

        for (int index = 0; index < remainingCars.Count; index++)
        {
            MoveCarToSlot(remainingCars[index], waitingArea.GetChild(index));
        }

        if (IsEmpty)
        {
            BecameEmpty?.Invoke();
        }
    }

    public void CompactAfterReset()
    {
        restartRequestPending = false;

        if (waitingArea == null)
        {
            return;
        }

        List<CarInteraction> remainingCars = GetWaitingCars();

        for (int index = 0; index < remainingCars.Count; index++)
        {
            MoveCarToSlot(remainingCars[index], waitingArea.GetChild(index));
        }

        if (IsEmpty)
        {
            BecameEmpty?.Invoke();
        }
    }

    public async UniTask<bool> TryMoveCar(CarInteraction carInteraction)
    {
        if (carInteraction == null || isStorageMoveInProgress)
        {
            return false;
        }

        for (int index = 0; index < RegularSlotCount; index++)
        {
            Transform slot = waitingArea.GetChild(index);

            if (slot.GetComponentInChildren<CarInteraction>(true) != null)
            {
                continue;
            }

            RectTransform carRect = carInteraction.GetComponent<RectTransform>();
            RectTransform slotRect = slot as RectTransform;
            await WaitingSlotMoveAnimation.PlayAsync(carRect, slotRect);

            if (this == null || carInteraction == null || slot == null)
            {
                return false;
            }

            MoveCarToSlot(carInteraction, slot);
            bool hasMatch = MatchWaitingCars();

            if (!hasMatch && AreAllSlotsFilled() && !restartRequestPending)
            {
                restartRequestPending = true;
                RaiseLevelLostNextFrame().Forget();
            }

            return true;
        }

        return false;
    }

    private void MoveCarToSlot(CarInteraction carInteraction, Transform slot)
    {
        RectTransform carRect = carInteraction.GetComponent<RectTransform>();
        RectTransform slotRect = slot as RectTransform;

        carRect.SetParent(slot, false);
        carRect.anchorMin = new Vector2(0.5f, 0.5f);
        carRect.anchorMax = new Vector2(0.5f, 0.5f);
        carRect.pivot = new Vector2(0.5f, 0.5f);
        carRect.anchoredPosition = Vector2.zero;
        carRect.sizeDelta = slotRect.rect.size;
        carRect.localScale = Vector3.one * carScale;
        carInteraction.SetState(CarState.Waiting);
        carRect.SetAsLastSibling();
    }

    public bool CanMoveWaitingCarsToExtraStorage()
    {
        if (waitingArea == null || isStorageMoveInProgress)
        {
            return false;
        }

        int waitingCarCount = GetWaitingCars().Count;
        return waitingCarCount > 0 && GetFreeExtraStorageSlotCount() >= waitingCarCount;
    }

    public async UniTask MoveWaitingCarsToExtraStorage()
    {
        if (!CanMoveWaitingCarsToExtraStorage())
        {
            return;
        }

        isStorageMoveInProgress = true;
        List<CarInteraction> waitingCars = GetWaitingCars();
        int extraSlotIndex = RegularSlotCount;

        foreach (CarInteraction car in waitingCars)
        {
            while (extraSlotIndex < waitingArea.childCount && waitingArea.GetChild(extraSlotIndex).GetComponentInChildren<CarInteraction>(true) != null)
            {
                extraSlotIndex++;
            }

            if (extraSlotIndex >= waitingArea.childCount)
            {
                break;
            }

            Transform slot = waitingArea.GetChild(extraSlotIndex++);
            await WaitingSlotMoveAnimation.PlayAsync(car.GetComponent<RectTransform>(), slot as RectTransform);

            if (this == null || car == null || slot == null)
            {
                return;
            }

            MoveCarToSlot(car, slot);
            car.SetState(CarState.ExtraStorage);
            car.ExtraStorageSelectionRequested -= HandleExtraStorageCarSelected;
            car.ExtraStorageSelectionRequested += HandleExtraStorageCarSelected;
        }

        restartRequestPending = false;
        isStorageMoveInProgress = false;
    }

    private void HandleExtraStorageCarSelected(CarInteraction car)
    {
        if (isStorageMoveInProgress || car == null || car.State != CarState.ExtraStorage)
        {
            return;
        }

        Transform freeSlot = GetFirstFreeRegularSlot();

        if (freeSlot != null)
        {
            MoveExtraStorageCarToWaiting(car, freeSlot).Forget();
        }
    }

    private async UniTask MoveExtraStorageCarToWaiting(CarInteraction car, Transform slot)
    {
        isStorageMoveInProgress = true;
        int storageSlotIndex = car.transform.parent.GetSiblingIndex();
        car.SetState(CarState.Blocked);
        await WaitingAreaMatchAnimation.PlayAsync(new[] {car});

        if (this == null || car == null || slot == null)
        {
            return;
        }

        car.ExtraStorageSelectionRequested -= HandleExtraStorageCarSelected;
        MoveCarToSlot(car, slot);
        await CompactExtraStorageFromSlot(storageSlotIndex);

        if (this == null || car == null)
        {
            return;
        }

        await WaitingAreaMatchAnimation.PlayReverseAsync(new[] {car}, Vector3.one * carScale);

        if (this == null || car == null)
        {
            return;
        }

        MatchWaitingCars();
        isStorageMoveInProgress = false;
    }

    private async UniTask CompactExtraStorageFromSlot(int emptySlotIndex)
    {
        for (int index = emptySlotIndex + 1; index < waitingArea.childCount; index++)
        {
            Transform sourceSlot = waitingArea.GetChild(index);
            CarInteraction storedCar = sourceSlot.GetComponentInChildren<CarInteraction>(true);

            if (storedCar == null || storedCar.State != CarState.ExtraStorage)
            {
                continue;
            }

            Transform targetSlot = waitingArea.GetChild(index - 1);
            await WaitingSlotMoveAnimation.PlayAsync(storedCar.GetComponent<RectTransform>(), targetSlot as RectTransform);

            if (this == null || storedCar == null || targetSlot == null)
            {
                return;
            }

            MoveCarToSlot(storedCar, targetSlot);
            storedCar.SetState(CarState.ExtraStorage);
        }
    }

    private Transform GetFirstFreeRegularSlot()
    {
        for (int index = 0; index < RegularSlotCount; index++)
        {
            Transform slot = waitingArea.GetChild(index);

            if (slot.GetComponentInChildren<CarInteraction>(true) == null)
            {
                return slot;
            }
        }

        return null;
    }

    private int GetFreeExtraStorageSlotCount()
    {
        int freeSlotCount = 0;

        for (int index = RegularSlotCount; index < waitingArea.childCount; index++)
        {
            if (waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true) == null)
            {
                freeSlotCount++;
            }
        }

        return freeSlotCount;
    }

    private bool MatchWaitingCars()
    {
        var carsByType = new Dictionary<int, List<CarInteraction>>();

        for (int index = 0; index < RegularSlotCount; index++)
        {
            CarInteraction car = waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true);

            if (car == null || car.State != CarState.Waiting)
            {
                continue;
            }

            if (!carsByType.TryGetValue(car.CarType, out List<CarInteraction> cars))
            {
                cars = new List<CarInteraction>();
                carsByType.Add(car.CarType, cars);
            }

            cars.Add(car);
        }

        bool hasMatch = false;

        var matchedCars = new List<CarInteraction>();

        foreach (List<CarInteraction> cars in carsByType.Values)
        {
            if (cars.Count < 3)
            {
                continue;
            }

            for (int index = 0; index < 3; index++)
            {
                CarInteraction car = cars[index];
                car.SetState(CarState.Matched);
                matchedCars.Add(car);
            }

            hasMatch = true;
        }

        if (matchedCars.Count > 0)
        {
            RemoveMatchedCarsAfterAnimation(matchedCars).Forget();
        }

        return hasMatch;
    }

    private bool AreAllSlotsFilled()
    {
        if (waitingArea == null || RegularSlotCount == 0)
        {
            return false;
        }

        for (int index = 0; index < RegularSlotCount; index++)
        {
            if (waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true) == null)
            {
                return false;
            }
        }

        return true;
    }

    public void ClearCars()
    {
        restartRequestPending = false;

        if (waitingArea == null)
        {
            return;
        }

        for (int index = 0; index < waitingArea.childCount; index++)
        {
            CarInteraction car = waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true);

            if (car != null)
            {
                car.ExtraStorageSelectionRequested -= HandleExtraStorageCarSelected;
                Destroy(car.gameObject);
            }
        }
    }

    private async UniTask RaiseLevelLostNextFrame()
    {
        await UniTask.Yield();

        if (this == null || isStorageMoveInProgress || !AreAllSlotsFilled())
        {
            restartRequestPending = false;
            return;
        }

        restartRequestPending = false;
        LevelLost?.Invoke();
    }

    private async UniTask RemoveMatchedCarsAfterAnimation(List<CarInteraction> matchedCars)
    {
        if (this == null)
        {
            return;
        }

        await WaitingAreaMatchAnimation.PlayAsync(matchedCars);

        if (this == null)
        {
            return;
        }

        var waitingCars = new List<CarInteraction>();

        for (int index = 0; index < RegularSlotCount; index++)
        {
            CarInteraction car = waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true);

            if (car == null)
            {
                continue;
            }

            if (matchedCars.Contains(car))
            {
                Destroy(car.gameObject);
            }
            else if (car.State == CarState.Waiting)
            {
                waitingCars.Add(car);
            }
        }

        await UniTask.Yield();

        for (int index = 0; index < waitingCars.Count; index++)
        {
            MoveCarToSlot(waitingCars[index], waitingArea.GetChild(index));
        }

        if (IsEmpty)
        {
            BecameEmpty?.Invoke();
        }
    }
}
