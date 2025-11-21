using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour {

    [SerializeField] private Button authenticateButton;
    [SerializeField] private GameObject lobbyUI; // Add reference to your lobby UI

    private void Awake() {
        authenticateButton.onClick.AddListener(async () => {
            authenticateButton.interactable = false; // Disable button during authentication
            
            await LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());
            
            Hide();
            
            // Enable lobby UI after authentication completes
            if (lobbyUI != null) {
                lobbyUI.SetActive(true);
            }
        });
    }

    private void Start() {
        // Make sure lobby UI is disabled at start
        if (lobbyUI != null) {
            lobbyUI.SetActive(false);
        }
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}