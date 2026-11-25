using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>Returns every waiting car to the table cell where it started.</summary>
public sealed class ResetPowerUp : MonoBehaviour
{
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private TableManager tableManager;

    private bool isProcessing;

    private void Awake()
    {
        ResolveManagers();
    }

    public bool TryActivate()
    {
        if (isProcessing)
        {
            return false;
        }

        ResolveManagers();

        if (waitingAreaManager == null || tableManager == null || tableManager.IsCarRemovalInProgress || waitingAreaManager.GetWaitingCars().Count == 0)
        {
            return false;
        }

        ResetCars().Forget();
        return true;
    }

    private async UniTask ResetCars()
    {
        isProcessing = true;
        await tableManager.ResetWaitingCarsForPowerUp();

        if (this != null)
        {
            isProcessing = false;
        }
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
