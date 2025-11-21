using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public PlayerStats playerStats;
    public PlayerMovement playerMovement;
    public HealthBarController healthBarController;
    
    // private void Awake()
    // {
    //     playerStats = GetComponent<PlayerStats>();
    //     playerMovement = GetComponent<PlayerMovement>();
    // }
    //
    
    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        if (!IsOwner) return;
        
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
        
        healthBarController = FindObjectOfType<HealthBarController>();

        if (healthBarController != null)
        {
            Debug.Log("HealthBarController");
            healthBarController.playerStats = playerStats;
            healthBarController.InitializeHUD();
        }
        else
        {
            Debug.LogError("HealthBarController not found in scene!");
        }
    }

    public void EnableControl(bool enable)
    {
        playerMovement.canMove = enable;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public bool IsDead => playerStats.isDead;
    public void Revive() => playerStats.Revive();
}