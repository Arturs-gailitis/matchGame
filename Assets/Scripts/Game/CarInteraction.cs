using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles clicking a car and removes it only while it is removable.
/// </summary>
[DisallowMultipleComponent]
public sealed class CarInteraction : MonoBehaviour
{

    [SerializeField] private CarState state = CarState.Blocked;
    [SerializeField] private Button button;

    public CarState State => state;
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

    private void HandleClick()
    {
        if (state != CarState.Removable)
        {
            return;
        }

        RemovalRequested?.Invoke(this);
        Destroy(gameObject);
    }

    public enum CarState
    {
        Blocked,
        Removable
    }
}
