using UnityEngine;

/// <summary>Randomly rearranges only the cars that are currently on the table.</summary>
public sealed class ReshufflePowerUp : MonoBehaviour
{
    [SerializeField] private TableManager tableManager;

    private void Awake()
    {
        ResolveManager();
    }

    public bool TryActivate()
    {
        ResolveManager();
        return tableManager != null && tableManager.TryReshuffleCarsForPowerUp();
    }

    private void ResolveManager()
    {
        if (tableManager != null)
        {
            return;
        }

        Transform searchRoot = transform.parent != null ? transform.parent : transform.root;
        tableManager = searchRoot.GetComponentInChildren<TableManager>(true);
    }
}
