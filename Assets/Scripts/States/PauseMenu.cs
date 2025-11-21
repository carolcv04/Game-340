using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PauseMenu : NetworkBehaviour
{
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    
    // Add static reference
    public static PauseMenu Instance { get; private set; }
    
    public GameObject pauseMenuUI;

    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    // Add static method to check pause state
    public static bool IsGamePaused()
    {
        return Instance != null && Instance.isGamePaused.Value;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Subscribe to pause state changes
        isGamePaused.OnValueChanged += OnPauseStateChanged;
        
        // Apply current state for late joiners
        OnPauseStateChanged(false, isGamePaused.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isGamePaused.OnValueChanged -= OnPauseStateChanged;
    }

    void Update()
    {
        // Add null check for Keyboard.current
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsServer || IsHost)
            {
                TogglePause();
            }
            else
            {
                // Client requests pause from server
                RequestPauseServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPauseServerRpc()
    {
        TogglePause();
    }

    private void TogglePause()
    {
        if (!IsServer) return;
        
        isGamePaused.Value = !isGamePaused.Value;
    }

    private void OnPauseStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        
        // DON'T use Time.timeScale = 0 in multiplayer!
        // Instead, disable gameplay components
        SetGameplayEnabled(false);
    }

    public void Resume()
    {
        if (IsServer || IsHost)
        {
            isGamePaused.Value = false;
        }
        else
        {
            ResumeServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResumeServerRpc()
    {
        isGamePaused.Value = false;
    }

    private void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        SetGameplayEnabled(true);
    }

    private void SetGameplayEnabled(bool enabled)
    {
        // Disable/enable player controls
        var playerControllers = FindObjectsOfType<PlayerController>();
        foreach (var controller in playerControllers)
        {
            controller.enabled = enabled;
        }

        // // Disable/enable enemy AI
        // var enemies = FindObjectsOfType<EnemyAI>();
        // foreach (var enemy in enemies)
        // {
        //     enemy.enabled = enabled;
        // }
        //
        // // Add other gameplay components as needed
    }

    public void LoadMenu()
    {
        Debug.Log("Load menu");
        
        if (IsServer)
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("StartScene");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        
        if (IsServer)
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