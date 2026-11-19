using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns the actions belonging to the level-lost popup.</summary>
public sealed class LevelLostPopupController : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button lobbyButton;

    private LayoutManager layoutManager;
    private GameManager gameManager;

    public void Initialize(LayoutManager manager, GameManager game)
    {
        layoutManager = manager;
        gameManager = game;
    }

    private void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (lobbyButton != null)
        {
            lobbyButton.onClick.AddListener(GoToLobby);
        }
    }

    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartLevel);
        }

        if (lobbyButton != null)
        {
            lobbyButton.onClick.RemoveListener(GoToLobby);
        }
    }

    private void RestartLevel()
    {
        if (gameManager == null)
        {
            Debug.LogError("LevelLostPopup must be initialized by a GameLayoutController.", this);
            return;
        }

        gameManager.RestartLevel();
        gameObject.SetActive(false);
    }

    private void GoToLobby()
    {
        if (layoutManager == null)
        {
            Debug.LogError("LevelLostPopup must be initialized by a GameLayoutController.", this);
            return;
        }

        layoutManager.ShowLayout(LayoutManager.LayoutType.Lobby);
    }
}
