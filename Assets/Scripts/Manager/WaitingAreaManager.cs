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
            MatchWaitingCars();
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

    private void MatchWaitingCars()
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
            }

            RemoveMatchedCarsAfterDelay().Forget();
        }
    }

    private async UniTask RemoveMatchedCarsAfterDelay()
    {
        await UniTask.Delay(1000);

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

            if (car.State == CarInteraction.CarState.Matched)
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
    }
}
