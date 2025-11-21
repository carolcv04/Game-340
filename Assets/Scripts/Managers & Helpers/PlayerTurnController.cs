using Unity.Netcode;
using UnityEngine;

public class PlayerTurnController : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return; // Only local player runs this

        if (TurnManager.Instance == null) return;

        bool isMyTurn = TurnManager.Instance.IsMyTurn(OwnerClientId);

        if (isMyTurn)
        {
            // Allow player input or actions
        }
        else
        {
            // Disable player actions or show “waiting” UI
        }
    }
}