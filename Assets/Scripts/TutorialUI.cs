using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{ 
    [SerializeField] private Button startButton; // Add a button reference if you have one
    
    private void Start()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnBindingRebind += GameInput_OnBindingRebind;
        }
    
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.OnStateChanged += FountainGameManager_OnStateChanged;
            FountainGameManager.Instance.OnLocalPlayerReadyChanged += FountainGameManager_OnLocalPlayerReadyChanged;
        }
        else
        {
            Debug.LogWarning("[TutorialUI] FountainGameManager.Instance is null at Start!");
        }

        // Add button listener if you have a button
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        UpdateDisplay();
        Show();
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnBindingRebind -= GameInput_OnBindingRebind;
        }
        
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.OnStateChanged -= FountainGameManager_OnStateChanged;
            FountainGameManager.Instance.OnLocalPlayerReadyChanged -= FountainGameManager_OnLocalPlayerReadyChanged;
        }
        
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    private void GameInput_OnBindingRebind(object sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // TODO: Update tutorial text
    }

    private void FountainGameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        Debug.Log("[TutorialUI] Local player ready changed");
        if (FountainGameManager.Instance != null && FountainGameManager.Instance.IsLocalPlayerReady())
        {
            Hide();
        }
    }

    private void FountainGameManager_OnStateChanged(object sender, EventArgs e)
    {
        Debug.Log($"[TutorialUI] State changed");
        
        if (FountainGameManager.Instance == null) return;
        
        // Hide tutorial when game starts
        if (FountainGameManager.Instance.IsCountdownToStartActive() || 
            FountainGameManager.Instance.IsGamePlaying())
        {
            Hide();
        }
    }

    // If you have a button, this gets called
    private void OnStartButtonClicked()
    {
        Debug.Log("[TutorialUI] Start button clicked");
        ExitPanel();
    }

    public void ExitPanel()
    {
        Debug.Log("[TutorialUI] ExitPanel called");
        
        // Direct approach - just start the game
        if (FountainGameManager.Instance != null)
        {
            if (FountainGameManager.Instance.IsWaitingToStart())
            {
                Debug.Log("[TutorialUI] Calling StartCountdown");
                FountainGameManager.Instance.StartCountdown();
            }
        }
        else
        {
            Debug.LogError("[TutorialUI] FountainGameManager.Instance is null!");
        }

        Hide();
    }
    
    private void Show()
    {
        Debug.Log("[TutorialUI] Showing tutorial");
        gameObject.SetActive(true);
    }
    
    private void Hide()
    {
        Debug.Log("[TutorialUI] Hiding tutorial");
        gameObject.SetActive(false);
    }
}