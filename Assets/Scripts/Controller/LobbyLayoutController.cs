using UnityEngine;
using UnityEngine.UI;

/// <summary>Forwards Lobby UI actions to the parent LayoutManager.</summary>
public sealed class LobbyLayoutController : MonoBehaviour
{
    [SerializeField] private Button startButton;

    private LayoutManager layoutManager;
    private float nextEnergyCheckTime;

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
        RefreshStartButton();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextEnergyCheckTime)
        {
            return;
        }

        nextEnergyCheckTime = Time.unscaledTime + 0.25f;
        RefreshStartButton();
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

        if (!PlayerEnergyStorage.HasEnergy())
        {
            RefreshStartButton();
            return;
        }

        layoutManager.ShowLayout(LayoutType.Game);
    }

    private void RefreshStartButton()
    {
        if (startButton != null)
        {
            startButton.interactable = PlayerEnergyStorage.HasEnergy();
        }
    }
}
