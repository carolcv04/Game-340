using System;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Start()
    {
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.OnLocalPlayerReadyChanged += FountainGameManager_OnLocalPlayerReadyChanged;
            FountainGameManager.Instance.OnStateChanged += FountainGameManager_OnStateChanged;
        }
        
        Hide();
    }

    private void OnDestroy()
    {
        if (FountainGameManager.Instance != null)
        {
            FountainGameManager.Instance.OnLocalPlayerReadyChanged -= FountainGameManager_OnLocalPlayerReadyChanged;
            FountainGameManager.Instance.OnStateChanged -= FountainGameManager_OnStateChanged;
        }
    }

    private void FountainGameManager_OnStateChanged(object sender, EventArgs e)
    {
        // Hide when countdown starts OR when no longer waiting
        if (!FountainGameManager.Instance.IsWaitingToStart())
        {
            Hide();
        }
    }
    private void FountainGameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        // Only show if ready AND still waiting AND multiplayer
        if (FountainGameManager.Instance.IsLocalPlayerReady() && 
            FountainGameManager.Instance.IsWaitingToStart())
        {
            // Check if multiplayer (more than 1 connected client)
            if (Unity.Netcode.NetworkManager.Singleton != null && 
                Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
            {
                Show();
            }
        }
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