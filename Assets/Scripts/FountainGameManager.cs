using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FountainGameManager : NetworkBehaviour
{
    public static FountainGameManager Instance { get; private set; }
    
    [field: Header("Events")]
    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalPlayerReadyChanged;
    
    [Header("Player Spawning (Offline Mode)")]
    [SerializeField] private GameObject offlinePlayerPrefab;
    [SerializeField] private Vector2 offlineSpawnPoint = new Vector2(0, 0);
    
    [Header("Game Settings")]
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private float gameDuration = 180f; // 3 minutes
    
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }
    
    public enum GameEndReason
    {
        None,
        PlayerDied,
        TimeExpired,
        StreetCompleted
    }
    
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<GameEndReason> gameEndReason = new NetworkVariable<GameEndReason>(GameEndReason.None);
    
    private bool isLocalPlayerReady;
    private float countdownTimer;
    private float gameTimer;
    private bool isStreetCompleted;
    
    private PlayerStats currentPlayer;
    private Dictionary<ulong, bool> playerReadyDictionary;
    
    // Getters
    public GameEndReason GetGameEndReason() => gameEndReason.Value;
    public bool IsGamePlaying() => state.Value == State.GamePlaying;
    public bool IsGameOver() => state.Value == State.GameOver;
    public bool IsCountdownToStartActive() => state.Value == State.CountdownToStart;
    public bool IsWaitingToStart() => state.Value == State.WaitingToStart;
    public bool IsLocalPlayerReady() => isLocalPlayerReady;
    public float GetCountdownToStartTimer() => countdownTimer;
    public float GetGamePlayingTimerNormalized() => 1 - (gameTimer / gameDuration);
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        state.OnValueChanged += State_OnValueChanged;
        
        Debug.Log($"[FountainGameManager] OnNetworkSpawn - IsServer: {IsServer}, IsSpawned: {IsSpawned}");
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        Debug.Log($"[FountainGameManager] State: {previousValue} → {newValue}");
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        // Only server/host updates game state
        if (!IsServer) return;
        
        switch (state.Value)
        {
            case State.WaitingToStart:
                // Idle state - waiting for player to press start
                break;
            
            case State.CountdownToStart:
                countdownTimer -= Time.deltaTime;
                if (countdownTimer <= 0f)
                {
                    Debug.Log("[FountainGameManager] Countdown finished, starting game!");
                    
                    // Spawn player in offline mode
                    SpawnOfflinePlayer();
                    
                    gameTimer = gameDuration;
                    state.Value = State.GamePlaying;
                    
                    // ✅ Play game music if not already playing
                    if (AudioManager.Instance != null && AudioManager.Instance.gameMusic != null)
                    {
                        AudioManager.Instance.PlayBGM(AudioManager.Instance.gameMusic);
                    }
                }
                break;
                
            case State.GamePlaying:
                // Check win condition
                if (isStreetCompleted)
                {
                    EndGame(GameEndReason.StreetCompleted);
                    break;
                }
                
                // Check time limit
                gameTimer -= Time.deltaTime;
                if (gameTimer <= 0f)
                {
                    EndGame(GameEndReason.TimeExpired);
                }
                break;
                
            case State.GameOver:
                // Game ended - do nothing
                break;
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        // Only respond to interact in waiting state
        if (state.Value != State.WaitingToStart) return;
        
        Debug.Log("[FountainGameManager] Player pressed interact to start");
        
        isLocalPlayerReady = true;
        OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
        
        // In offline mode or as server, start immediately
        if (IsServer || !IsSpawned)
        {
            StartCountdown();
        }
        else
        {
            // Client in multiplayer - request from server
            SetPlayerReadyServerRpc();
        }
    }

    public void StartCountdown()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[FountainGameManager] Only server can start countdown");
            return;
        }
    
        if (state.Value != State.WaitingToStart)
        {
            Debug.LogWarning($"[FountainGameManager] Cannot start countdown from state: {state.Value}");
            return;
        }
    
        Debug.Log("[FountainGameManager] Starting countdown");
        countdownTimer = countdownDuration;
        state.Value = State.CountdownToStart;
    
        // ✅ Play game music when countdown starts
        if (AudioManager.Instance != null && AudioManager.Instance.gameMusic != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.gameMusic);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log($"[Server] Client {clientId} is ready");

        playerReadyDictionary[clientId] = true;

        // Check if all players are ready
        int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        
        // Single player - start immediately
        if (connectedCount == 1)
        {
            StartCountdown();
            return;
        }

        // Multiplayer - wait for all players
        bool allReady = true;
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(id) || !playerReadyDictionary[id])
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            Debug.Log("[Server] All players ready, starting countdown");
            StartCountdown();
        }
    }
    
    // Add this to FountainGameManager.cs

    public void RestartGame()
    {
        if (!IsServer) return;
    
        Debug.Log("[FountainGameManager] Restarting game...");
    
        // Reset game state
        gameEndReason.Value = GameEndReason.None;
        isStreetCompleted = false;
        isLocalPlayerReady = false;
        playerReadyDictionary.Clear();
    
        // Destroy current player if exists
        if (currentPlayer != null)
        {
            var playerNetObj = currentPlayer.GetComponent<NetworkObject>();
            if (playerNetObj != null && playerNetObj.IsSpawned)
            {
                playerNetObj.Despawn(true);
            }
            currentPlayer = null;
        }
    
        // Reset building states (if needed)
        if (BuildingManager.Instance != null)
        {
            // You'd need to add a ResetBuildings() method to BuildingManager
            // For now, buildings will stay built
        }
    
        // Start countdown again
        countdownTimer = countdownDuration;
        state.Value = State.CountdownToStart;
    
        // Play game music
        if (AudioManager.Instance != null && AudioManager.Instance.gameMusic != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.gameMusic);
        }
    }

    private void SpawnOfflinePlayer()
    {
        if (offlinePlayerPrefab == null)
        {
            Debug.LogError("[FountainGameManager] offlinePlayerPrefab not assigned!");
            return;
        }

        Vector3 spawnPosition = new Vector3(offlineSpawnPoint.x, offlineSpawnPoint.y, 0f);
        GameObject playerObj = Instantiate(offlinePlayerPrefab, spawnPosition, Quaternion.identity);
        
        var networkObject = playerObj.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(NetworkManager.ServerClientId);
            Debug.Log("[FountainGameManager] Player spawned at " + spawnPosition);
            
            // Set as current player for death tracking
            var playerStats = playerObj.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                SetCurrentPlayer(playerStats);
            }
        }
    }

    public void SetCurrentPlayer(PlayerStats player)
    {
        if (currentPlayer != null)
        {
            currentPlayer.OnPlayerDied -= HandlePlayerDeath;
        }

        currentPlayer = player;

        if (currentPlayer != null)
        {
            currentPlayer.OnPlayerDied += HandlePlayerDeath;
            Debug.Log("[FountainGameManager] Current player set");
        }
    }

    private void HandlePlayerDeath(PlayerStats deadPlayer)
    {
        if (!IsServer) return;
        
        Debug.Log($"[FountainGameManager] Player {deadPlayer.OwnerClientId} died!");
        EndGame(GameEndReason.PlayerDied);
    }

    public void SetStreetCompleted(bool completed)
    {
        if (!IsServer) return;
        
        isStreetCompleted = completed;
        Debug.Log($"[FountainGameManager] Street completed: {completed}");
    }

    private void EndGame(GameEndReason reason)
    {
        if (state.Value == State.GameOver) return; // Already ended
        
        Debug.Log($"[FountainGameManager] Game Over - Reason: {reason}");
        
        gameEndReason.Value = reason;
        state.Value = State.GameOver;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
        }
        
        if (currentPlayer != null)
        {
            currentPlayer.OnPlayerDied -= HandlePlayerDeath;
        }
    }
}