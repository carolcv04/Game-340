using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverReasonText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitButton;
    
    private void Start()
    {
        FountainGameManager.Instance.OnStateChanged += FountainGameManager_OnStateChanged;
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
        
        Hide();
    }

    private void FountainGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (FountainGameManager.Instance.IsGameOver())
        {
            Show();
            DisplayGameOverReason();
        }
        else
        {
            Hide();
        }
    }

    private void DisplayGameOverReason()
    {
        var reason = FountainGameManager.Instance.GetGameEndReason();

        switch (reason)
        {
            case FountainGameManager.GameEndReason.PlayerDied:
                gameOverReasonText.text = "Game Over - Player Died!";
                break;
            case FountainGameManager.GameEndReason.TimeExpired:
                gameOverReasonText.text = "Game Over - Time Ran Out!";
                break;
            case FountainGameManager.GameEndReason.StreetCompleted:
                gameOverReasonText.text = "Victory! Street Completed!";
                break;
            default:
                gameOverReasonText.text = "Game Over!";
                break;
        }
    }

    // private void OnRestartClicked()
    // {
    //     Debug.Log("[GameOverUI] Restart clicked");
    //     
    //     // Shutdown and restart network
    //     if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
    //     {
    //         NetworkManager.Singleton.Shutdown();
    //     }
    //     
    //     // Small delay to ensure clean shutdown
    //     StartCoroutine(RestartAfterDelay());
    // }
    
    private System.Collections.IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Restart as host
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
        }
        
        // Reload game scene
        Loader.Load(Loader.Scene.GameScene);
    }
    
    private void OnMainMenuClicked()
    {
        Debug.Log("[GameOverUI] Main Menu clicked");
        
        // Shutdown network
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        // Load main menu
        Loader.Load(Loader.Scene.MainMenuScene);
    }
    
    private void OnExitClicked()
    {
        Debug.Log("[GameOverUI] Exit clicked");
        
        // Shutdown network
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
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    
    private void Show()
    {
        gameObject.SetActive(true);    
    }
    
    public void OnRestartClicked()
    {
        Debug.Log("[GameOverUI] Restart clicked");
    
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogError("[GameOverUI] FountainGameManager.Instance is null!");
        }
    }
}
