using UnityEngine;

public enum ObjectState
{
    Car,
    Wall
}

/// <summary>
/// Base component for every object occupying a table cell.
/// </summary>
public abstract class TableObject : MonoBehaviour
{
    [SerializeField] private ObjectState objectState;

    public ObjectState ObjectState => objectState;

    public void SetObjectState(ObjectState newState)
    {
        objectState = newState;
    }
}
