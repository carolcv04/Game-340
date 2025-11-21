using Unity.Netcode;
using UnityEngine;

public class PlayerItemCollector : NetworkBehaviour
{
    private PlayerInventory _playerInventory;

    void Start()
    {
        _playerInventory = GetComponent<PlayerInventory>();
        if (_playerInventory == null)
            Debug.LogError("PlayerInventoryController not found on this player!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Local client detects the collision
        if (!IsOwner) return;

        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item == null || item.NetworkObject == null) return;

            Debug.Log($"[Client] Attempting to pick up: {item.itemName}");
            RequestPickupServerRpc(item.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ulong itemNetworkId, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId, out NetworkObject itemNetObj))
            return;

        var item = itemNetObj.GetComponent<Item>();
        if (item == null) return;

        // Find the player who sent the request
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        var playerObject = NetworkManager.ConnectedClients[senderClientId].PlayerObject;
        var playerInventory = playerObject.GetComponent<PlayerInventory>();

        if (playerInventory != null)
        {
            Debug.Log($"[Server] Player {senderClientId} picking up {item.itemName}");
            playerInventory.TryAddItem(item.Preset, item.quantity);

            // Despawn the item on the server (propagates to all clients)
            if (itemNetObj.IsSpawned)
                itemNetObj.Despawn(true);
        }
    }
}
