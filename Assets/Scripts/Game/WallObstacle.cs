using UnityEngine;

/// <summary>
/// Marks a grid object as an immovable wall obstacle.
/// </summary>
[DisallowMultipleComponent]
public sealed class WallObstacle : MonoBehaviour
{
    private ObjectState objectState;

    public ObjectState ObjectState => objectState;

    public void SetObjectState(ObjectState newState)
    {
        objectState = newState;
    }
}
