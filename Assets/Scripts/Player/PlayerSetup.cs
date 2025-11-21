using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    public HealthBarController healthBarController;

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        if (!IsOwner) return;
        
        healthBarController.playerStats = GetComponent<PlayerStats>();
        healthBarController.InitializeHUD();
    }
}
