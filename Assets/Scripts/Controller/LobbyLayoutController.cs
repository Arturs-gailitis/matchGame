using UnityEngine;
using UnityEngine.UI;

/// <summary>Forwards Lobby UI actions to the parent LayoutManager.</summary>
public sealed class LobbyLayoutController : MonoBehaviour
{
    [SerializeField] private Button startButton;

    private LayoutManager layoutManager;

    public void Initialize(LayoutManager manager)
    {
        layoutManager = manager;
    }

    private void Awake()
    {
        if (startButton == null)
        {
            Debug.LogError("LobbyLayoutController: Start Button is not assigned.", this);
            return;
        }

        startButton.onClick.AddListener(StartGame);
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }
    }

    private void StartGame()
    {
        if (layoutManager == null)
        {
            Debug.LogError("LobbyLayout must be instantiated below a LayoutManager.", this);
            return;
        }

        layoutManager.ShowLayout(LayoutManager.LayoutType.Game);
    }
}
