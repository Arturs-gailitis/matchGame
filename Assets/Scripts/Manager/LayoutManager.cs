using UnityEngine;

/// <summary>
/// Displays either the lobby layout or the game layout.
/// </summary>
[DisallowMultipleComponent]
public sealed class LayoutManager : MonoBehaviour
{

    [Header("Layout Prefabs")]
    [SerializeField] private GameObject lobbyLayoutPrefab;
    [SerializeField] private GameObject gameLayoutPrefab;

    [Header("Settings")]
    [SerializeField] private LayoutType initialLayout = LayoutType.Lobby;

    private GameObject currentInstance;

    public LayoutType CurrentLayout { get; private set; }

    private void Awake()
    {
        ShowLayout(initialLayout);
    }

    public void ShowLayout(LayoutType layout)
    {
        GameObject prefab = layout == LayoutType.Lobby ? lobbyLayoutPrefab : gameLayoutPrefab;

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
            Destroy(currentInstance);
        }

        // Keep screen-space Canvas layouts as root objects so CanvasScaler can scale them.
        Transform parent = null;
        currentInstance = Instantiate(prefab, parent, false);
        currentInstance.name = prefab.name;
        CurrentLayout = layout;

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

    public enum LayoutType
    {
        Lobby,
        Game
    }
}
