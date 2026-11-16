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
    [SerializeField, Range(0.5f, 1f)] private float carScale = 0.8f;

    public event Action BecameEmpty;
    public event Action RestartLevelRequested;

    private bool restartRequestPending;

    public bool IsEmpty => waitingArea != null && waitingArea.GetComponentInChildren<CarInteraction>(true) == null;

    public bool TryMoveCar(CarInteraction carInteraction)
    {
        if (carInteraction == null)
        {
            return false;
        }

        for (int index = 0; index < waitingArea.childCount; index++)
        {
            Transform slot = waitingArea.GetChild(index);

            if (slot.GetComponentInChildren<CarInteraction>(true) != null)
            {
                continue;
            }

            MoveCarToSlot(carInteraction, slot);
            bool hasMatch = MatchWaitingCars();

            if (!hasMatch && AreAllSlotsFilled() && !restartRequestPending)
            {
                restartRequestPending = true;
                RaiseRestartLevelRequestedNextFrame().Forget();
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
        carInteraction.SetState(CarInteraction.CarState.Waiting);
        carRect.SetAsLastSibling();
    }

    private bool MatchWaitingCars()
    {
        var carsByType = new Dictionary<int, List<CarInteraction>>();

        for (int index = 0; index < waitingArea.childCount; index++)
        {
            CarInteraction car = waitingArea.GetChild(index).GetComponentInChildren<CarInteraction>(true);

            if (car == null || car.State != CarInteraction.CarState.Waiting)
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
                car.SetState(CarInteraction.CarState.Matched);
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
        if (waitingArea == null || waitingArea.childCount == 0)
        {
            return false;
        }

        for (int index = 0; index < waitingArea.childCount; index++)
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
                Destroy(car.gameObject);
            }
        }
    }

    private async UniTask RaiseRestartLevelRequestedNextFrame()
    {
        await UniTask.Yield();

        if (this == null || !AreAllSlotsFilled())
        {
            restartRequestPending = false;
            return;
        }

        restartRequestPending = false;
        RestartLevelRequested?.Invoke();
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

        for (int index = 0; index < waitingArea.childCount; index++)
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
            else if (car.State == CarInteraction.CarState.Waiting)
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
