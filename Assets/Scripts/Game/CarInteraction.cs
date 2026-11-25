using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles clicking a car while it is removable.
/// </summary>
[DisallowMultipleComponent]
public sealed class CarInteraction : TableObject
{

    [SerializeField] private CarState state = CarState.Blocked;
    [SerializeField] private int carType = -1;
    [SerializeField] private Button button;

    public CarState State => state;
    public int CarType => carType;
    public event Action<CarInteraction> RemovalRequested;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleClick);
    }

    public void SetState(CarState newState)
    {
        state = newState;
    }

    public void SetCarType(int type)
    {
        carType = type;
    }

    private void HandleClick()
    {
        if (state != CarState.Removable)
        {
            return;
        }

        RemovalRequested?.Invoke(this);
    }
}
