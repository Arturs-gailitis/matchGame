using UnityEngine;

/// <summary>
/// Displays the requested application layout.
/// </summary>
[DisallowMultipleComponent]
public sealed class LayoutManager : MonoBehaviour
{

    [Header("Layout Prefabs")]
    [SerializeField] private GameObject lobbyLayoutPrefab;
    [SerializeField] private GameObject gameLayoutPrefab;
    [SerializeField] private GameObject shopLayoutPrefab;

    [Header("Settings")]
    [SerializeField] private LayoutType initialLayout = LayoutType.Lobby;

    private GameObject currentInstance;
    private FooterManager currentFooterManager;

    public LayoutType CurrentLayout { get; private set; }

    private void Awake()
    {
        ShowLayout(initialLayout);
    }

    public void ShowLayout(LayoutType layout)
    {
        if (layout == LayoutType.Game && !PlayerEnergyStorage.HasEnergy())
        {
            Debug.Log("LayoutManager: the player cannot enter a level without energy.", this);
            return;
        }

        GameObject prefab = GetLayoutPrefab(layout);

        if (prefab == null)
        {
            Debug.LogError($"LayoutManager: {layout} layout prefab is not assigned.", this);
            return;
        }

        if (currentInstance != null && CurrentLayout == layout)
        {
            return;
        }

        if (currentInstance != null)
        {
            DisconnectFooter();
            Destroy(currentInstance);
        }

        // Keep screen-space Canvas layouts as root objects so CanvasScaler can scale them.
        Transform parent = null;
        currentInstance = Instantiate(prefab, parent, false);
        currentInstance.name = prefab.name;
        CurrentLayout = layout;

        currentFooterManager = currentInstance.GetComponentInChildren<FooterManager>(true);

        if (currentFooterManager != null)
        {
            currentFooterManager.SectionSelected += HandleFooterSectionSelected;
        }

        LobbyLayoutController lobbyController = currentInstance.GetComponentInChildren<LobbyLayoutController>(true);

        if (lobbyController != null)
        {
            lobbyController.Initialize(this);
        }

        GameLayoutController gameController = currentInstance.GetComponentInChildren<GameLayoutController>(true);

        if (gameController != null)
        {
            gameController.Initialize(this);
        }
    }

    private GameObject GetLayoutPrefab(LayoutType layout)
    {
        switch (layout)
        {
            case LayoutType.Lobby:
                return lobbyLayoutPrefab;
            case LayoutType.Game:
                return gameLayoutPrefab;
            case LayoutType.Shop:
                return shopLayoutPrefab;
            default:
                return null;
        }
    }

    private void HandleFooterSectionSelected(LayoutType layout)
    {
        if (layout == LayoutType.Shop || layout == LayoutType.Lobby)
        {
            ShowLayout(layout);
        }
    }

    private void DisconnectFooter()
    {
        if (currentFooterManager != null)
        {
            currentFooterManager.SectionSelected -= HandleFooterSectionSelected;
            currentFooterManager = null;
        }
    }

    private void OnDestroy()
    {
        DisconnectFooter();
    }
}
