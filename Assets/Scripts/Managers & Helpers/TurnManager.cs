using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;
    [SerializeField] private Countdown countdown;
    
    private NetworkVariable<int> currentTurnIndex = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool gameStarted = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        countdown = FindObjectOfType<Countdown>();
    }

    private void Update()
    {
        if (!IsServer) return;

        // Wait until at least 2 players are connected before starting the first turn
        if (!gameStarted && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            StartNextTurn();
            gameStarted = true;
        }
    }

    private void StartNextTurn()
    {
        int nextTurn = currentTurnIndex.Value == 0 ? 1 : 0;
        currentTurnIndex.Value = nextTurn;

        // Start countdown for the current player
        StartCoroutine(TurnRoutine());
    }

    private IEnumerator TurnRoutine()
    {
        int currentPlayer = currentTurnIndex.Value;
        Debug.Log($"Starting turn for Player {currentPlayer}");

        countdown.GameTimer(10); // 10 seconds for the turn
        yield return new WaitUntil(() => countdown.isTimerFinished);

        EndTurn();
    }

    public void EndTurn()
    {
        Debug.Log("Turn ended, switching...");
        StartNextTurn();
    }

    // Expose who’s turn it is
    public bool IsMyTurn(ulong clientId)
    {
        int index = GetPlayerIndex(clientId);
        return index == currentTurnIndex.Value;
    }

    private int GetPlayerIndex(ulong clientId)
    {
        // Just a simple mapping: first joined = 0, second = 1
        var clients = new List<ulong>(NetworkManager.Singleton.ConnectedClients.Keys);
        clients.Sort();
        return clients.IndexOf(clientId);
    }
}
