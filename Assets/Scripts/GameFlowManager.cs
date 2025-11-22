using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string characterSelectScene = "CharacterSelect";
    [SerializeField] private string gameplayScene = "Gameplay";
    
    [Header("Testing")]
    [SerializeField] private bool offlineMode = false; // Toggle this in Inspector for testing
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Call this when player clicks "Start/Host" button
    /// </summary>
    public void StartGame()
    {
        if (offlineMode || !Application.isEditor)
        {
            // Offline/single-player mode
            Debug.Log("[GameFlowManager] Starting in offline mode");
            StartOfflineGame();
        }
        else
        {
            // Online multiplayer mode
            Debug.Log("[GameFlowManager] Starting in online mode");
            if (HostManager.Instance != null)
            {
                HostManager.Instance.StartHost();
            }
        }
    }
    
    private void StartOfflineGame()
    {
        // Start NetworkManager as host for single player
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            
            // Wait a frame, then load character select or go straight to gameplay
            StartCoroutine(LoadSceneAfterFrame());
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] No NetworkManager found, loading scene directly");
            SceneManager.LoadScene(gameplayScene);
        }
    }
    
    private System.Collections.IEnumerator LoadSceneAfterFrame()
    {
        yield return null; // Wait one frame for NetworkManager to initialize
        
        // Skip character select and go straight to gameplay for testing
        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(gameplayScene);
        }
    }
    
    /// <summary>
    /// Load main menu
    /// </summary>
    public void LoadMainMenu()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        SceneManager.LoadScene(mainMenuScene);
    }
    
    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        StartGame();
    }
}