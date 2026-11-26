using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>Moves every waiting car into the extra storage row.</summary>
public sealed class ExtraStoragePowerUp : MonoBehaviour
{
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private TableManager tableManager;

    private bool isProcessing;

    private void Awake()
    {
        ResolveManager();
    }

    public bool TryActivate()
    {
        if (isProcessing)
        {
            return false;
        }

        ResolveManager();

        if (waitingAreaManager == null || tableManager == null || tableManager.IsCarRemovalInProgress || !waitingAreaManager.CanMoveWaitingCarsToExtraStorage())
        {
            return false;
        }

        MoveCars().Forget();
        return true;
    }

    private async UniTask MoveCars()
    {
        isProcessing = true;
        await waitingAreaManager.MoveWaitingCarsToExtraStorage();

        if (this != null)
        {
            isProcessing = false;
        }
    }

    private void ResolveManager()
    {
        if (waitingAreaManager != null && tableManager != null)
        {
            return;
        }

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
