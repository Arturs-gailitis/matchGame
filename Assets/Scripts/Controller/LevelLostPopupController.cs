using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns the actions belonging to the level-lost popup.</summary>
public sealed class LevelLostPopupController : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button lobbyButton;

    private LayoutManager layoutManager;
    private GameManager gameManager;
    private float nextEnergyCheckTime;

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

    private void OnEnable()
    {
        RefreshRestartButton();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextEnergyCheckTime)
        {
            return;
        }

        nextEnergyCheckTime = Time.unscaledTime + 0.25f;
        RefreshRestartButton();
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

        if (!PlayerEnergyStorage.HasEnergy())
        {
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

        layoutManager.ShowLayout(LayoutType.Lobby);
    }

    private void RefreshRestartButton()
    {
        if (restartButton != null)
        {
            restartButton.interactable = PlayerEnergyStorage.HasEnergy();
        }
    }
}
