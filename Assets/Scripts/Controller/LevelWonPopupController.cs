using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns the actions belonging to the level-won popup.</summary>
public sealed class LevelWonPopupController : MonoBehaviour
{
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button lobbyButton;
    [SerializeField] private Text rewardAmountText;

    private LayoutManager layoutManager;
    private GameManager gameManager;

    public void Initialize(LayoutManager manager, GameManager game)
    {
        layoutManager = manager;
        gameManager = game;

        if (rewardAmountText != null && gameManager != null)
        {
            rewardAmountText.text = gameManager.GetCurrentLevelReward().ToString();
        }
    }

    private void Awake()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(ShowNextLevel);
        }

        if (lobbyButton != null)
        {
            lobbyButton.onClick.AddListener(GoToLobby);
        }
    }

    private void OnDestroy()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(ShowNextLevel);
        }

        if (lobbyButton != null)
        {
            lobbyButton.onClick.RemoveListener(GoToLobby);
        }
    }

    private void ShowNextLevel()
    {
        if (gameManager == null)
        {
            Debug.LogError("LevelWonPopup must be initialized by a GameLayoutController.", this);
            return;
        }

        gameManager.ContinueToNextLevel();
        gameObject.SetActive(false);
    }

    private void GoToLobby()
    {
        if (layoutManager == null || gameManager == null)
        {
            Debug.LogError("LevelWonPopup must be initialized by a GameLayoutController.", this);
            return;
        }

        gameManager.TryCommitNextLevel();
        layoutManager.ShowLayout(LayoutType.Lobby);
    }
}
