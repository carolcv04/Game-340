using System;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{ 
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

        UpdateDisplay();
        Show();
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnBindingRebind -= GameInput_OnBindingRebind;
        }
        
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.OnStateChanged -= FountainGameManager_OnStateChanged;
            FountainGameManager.Instance.OnLocalPlayerReadyChanged -= FountainGameManager_OnLocalPlayerReadyChanged;
        }
    }

    private void GameInput_OnBindingRebind(object sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // TODO: Update your tutorial text/UI elements
        // Example: tutorialText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        // For now, empty to prevent crashes
    }

    private void FountainGameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if (FountainGameManager.Instance.IsLocalPlayerReady())
        {
            Hide();
        }
    }

    private void FountainGameManager_OnStateChanged(object sender, EventArgs e)
    {
        // Hide tutorial when game starts
        if (FountainGameManager.Instance.IsCountdownToStartActive() || 
            FountainGameManager.Instance.IsGamePlaying())
        {
            Hide();
        }
    }

    public void ExitPanel()
    {
        // Trigger the state change
        if (FountainGameManager.Instance.IsWaitingToStart())
        {
            FountainGameManager.Instance.StartCountdown();
        }

        Hide();
    }
    
    private void Show()
    {
        gameObject.SetActive(true);
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}