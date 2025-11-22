using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button lobbiesButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Testing")]
    [SerializeField] private bool offlineMode = true; // Toggle for testing without networking

    private void Awake()
    {
        // Start Game (Single Player or Host)
        startButton.onClick.AddListener(OnStartClick);
        
        // Lobbies (Multiplayer)
        lobbiesButton.onClick.AddListener(OnLobbiesClick);
        
        // Options
        optionsButton.onClick.AddListener(OnOptionsClick);
        
        // Quit
        quitButton.onClick.AddListener(OnExitClick);
    }

    private void OnStartClick()
    {
        Debug.Log("[StartMenuController] Start button clicked");
        
        if (offlineMode)
        {
            // Offline/Single Player mode
            Debug.Log("[StartMenuController] Starting in offline mode");
            StartCoroutine(StartOfflineGameCoroutine());
        }
        else
        {
            // Online mode - start as host with networking
            Debug.Log("[StartMenuController] Starting in online mode");
            StartOnlineGame();
        }
    }
    
    private IEnumerator StartOfflineGameCoroutine()
    {
        // Start NetworkManager as host for offline play
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("[StartMenuController] Starting NetworkManager as host...");
            NetworkManager.Singleton.StartHost();
            
            // Wait for network to initialize
            yield return new WaitUntil(() => NetworkManager.Singleton.IsListening);
            
            Debug.Log("[StartMenuController] NetworkManager ready, loading game scene");
        }
        else
        {
            Debug.LogError("[StartMenuController] NetworkManager not found!");
            yield break;
        }
        
        // Now load the game scene
        Loader.Load(Loader.Scene.GameScene);
    }
    
    private void StartOnlineGame()
    {
        // Use HostManager for proper relay/lobby setup
        if (HostManager.Instance != null)
        {
            HostManager.Instance.StartHost();
        }
        else
        {
            Debug.LogWarning("[StartMenuController] HostManager not found, falling back to offline mode");
            StartCoroutine(StartOfflineGameCoroutine());
        }
    }

    public void OnLobbiesClick()
    {
        Debug.Log("[StartMenuController] Lobbies button clicked");
        SceneManager.LoadScene("LobbyTutorial_Done");
    }
    
    private void OnOptionsClick()
    {
        Debug.Log("[StartMenuController] Options button clicked");
        // TODO: Load options scene or show options panel
        // SceneManager.LoadScene("OptionsScene");
    }

    public void OnExitClick()
    {
        Debug.Log("[StartMenuController] Exit button clicked");
        
        // Shutdown network if running
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
