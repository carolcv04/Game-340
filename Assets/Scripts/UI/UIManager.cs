using UnityEngine;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance { get; private set; }
    public static event System.Action OnUIReady;
    [SerializeField] private GameObject gameOverPanel;

    public GameObject InventoryPage;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        // Subscribe in Start instead of Awake to ensure FountainGameManager is ready
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.OnStateChanged += HandleGameStateChanged;
        }
        else
        {
            Debug.LogWarning("FountainGameManager.Instance is null in UIManager.Start()");
        }
        
        OnUIReady?.Invoke();
    }

    private void OnDestroy()
    {
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(object sender, System.EventArgs e)
    {
        if (FountainGameManager.Instance && FountainGameManager.Instance.IsGameOver())
        {
            ShowGameOver();
        }
    }
    
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over panel activated!");
        }
        else
        {
            Debug.LogError("gameOverPanel is not assigned in UIManager!");
        }
    }
}