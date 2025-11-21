using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FountainGameManager : NetworkBehaviour {
    public static FountainGameManager Instance { get; private set; }
    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    private PlayerStats currentPlayer;
    
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady;
    private float countdownToStartTimer = 3f;
    private float countdownToStartTimerMax = 3f;
    private bool isFountainClaimed;
    private float turnPlayingTimer;
    private float turnPlayingTimerMax = 60f;
    private int currentPlayerIndex = 0;
    private int totalPlayers;
    
    //TEMP FOR TESTING
    private PlayerStats playerStatsForThisTurn;
    private Dictionary<ulong, bool> playerReadyDictionary;
    
    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        
        //TEMP FOR TESTING
        var manager = FindObjectOfType<FountainGameManager>();
        manager.SetCurrentPlayer(playerStatsForThisTurn);
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        
        if (IsServer)
        {
            totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
            Debug.Log($"[FountainGameManager] Total players initialized: {totalPlayers}");
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        Debug.Log($"[FountainGameManager] State changed from {previousValue} to {newValue}");
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void TogglePauseGame()
    {
        //TODO
        //Pause game here 
    }
    
    public void SetCurrentPlayer(PlayerStats player)
    {
        // Unsubscribe from old player if needed
        if (currentPlayer != null)
        {
            currentPlayer.OnPlayerDied -= HandleCurrentPlayerDeath;
        }

        currentPlayer = player;

        if (currentPlayer != null)
        {
            currentPlayer.OnPlayerDied += HandleCurrentPlayerDeath;
        }
    }
    
    private void HandleCurrentPlayerDeath(PlayerStats deadPlayer)
    {
        Debug.Log($"Current player {deadPlayer.OwnerClientId} died!");
        state.Value = State.GameOver;
    }

    private void Update()
    {
        if (!IsServer) { return; }
        
        switch (state.Value)
        {
            case State.WaitingToStart:
                SoundEffectManager.Play("MainMenu", true);
                break;
            
            case State.CountdownToStart:
                SoundEffectManager.Play("MainMenu", false);

                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer <= 0f)
                {
                    Debug.Log("[FountainGameManager] Countdown finished, starting game!");
                    turnPlayingTimer = turnPlayingTimerMax;
                    state.Value = State.GamePlaying;
                }
                break;
                
            case State.GamePlaying:
                SoundEffectManager.Play("Overworld", true);

                if (isFountainClaimed)
                {
                    Debug.Log("[FountainGameManager] Fountain claimed, game over!");
                    state.Value = State.GameOver;
                }
                else
                {
                    // Check both death and timer
                    bool shouldEndTurn = IsCurrentPlayerDead() || turnPlayingTimer <= 0f;
        
                    if (!IsCurrentPlayerDead())
                    {
                        turnPlayingTimer -= Time.deltaTime;
                        
                        // Log when timer is about to expire
                        if (turnPlayingTimer <= 1f && turnPlayingTimer > 0.9f)
                        {
                            Debug.Log("[FountainGameManager] Turn timer almost expired!");
                        }
                    }
        
                    if (shouldEndTurn)
                    {
                        Debug.Log($"[FountainGameManager] Turn ended. Current player: {currentPlayerIndex}");
                        
                        currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
                        
                        // If we've cycled through all players, end the game
                        if (currentPlayerIndex == 0)
                        {
                            Debug.Log("[FountainGameManager] All players have taken their turn, game over!");
                            state.Value = State.GameOver;
                        }
                        else
                        {
                            turnPlayingTimer = turnPlayingTimerMax;
                        }
                    }
                }
                break;
                
            case State.GameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }
    
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }
    
    private bool IsCurrentPlayerDead()
    {
        if (currentPlayer == null) return false;
        return currentPlayer.IsDead();
    }
    
    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }
    
    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (turnPlayingTimer / turnPlayingTimerMax);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            Debug.Log("About to call ServerRpc");
            SetPlayerReadyServerRpc();
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            Debug.Log("After calling ServerRpc");
        }
    }
    
    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }
    
    public void StartCountdown()
    {
        if (state.Value == State.WaitingToStart)
        {
            state.Value = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("ServerRpc received!");
        Debug.Log("Sender Client ID: " + serverRpcParams.Receive.SenderClientId);

        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        // Get total connected players
        int connectedPlayersCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Debug.Log("Connected players count: " + connectedPlayersCount);

        // If single player, immediately start countdown
        if (connectedPlayersCount == 1)
        {
            Debug.Log("Single player detected - starting countdown immediately");
            countdownToStartTimer = countdownToStartTimerMax;
            state.Value = State.CountdownToStart;
            return;
        }

        // For multiplayer, check if all players are ready
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        Debug.Log("allClientsReady: " + allClientsReady);
        Debug.Log("Total players ready: " + playerReadyDictionary.Count);

        if (allClientsReady)
        {
            countdownToStartTimer = countdownToStartTimerMax;
            state.Value = State.CountdownToStart;
        }
    }
}