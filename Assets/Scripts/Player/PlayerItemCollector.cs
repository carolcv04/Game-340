using Unity.Netcode;
using UnityEngine;

public class PlayerItemCollector : NetworkBehaviour
{
    private PlayerInventory _playerInventory;

    private void Start()
    {
        _playerInventory = GetComponent<PlayerInventory>();
        
        if (_playerInventory == null)
        {
            Debug.LogError("[ItemCollector] PlayerInventory component not found on player!");
        }
        else
        {
            Debug.Log("[ItemCollector] PlayerInventory found and ready");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Item")) return;
        if (!IsOwner) return;

        Item item = collision.GetComponent<Item>();
        if (item == null || item.Preset == null)
        {
            Debug.LogWarning("[ItemCollector] Invalid item!");
            return;
        }

        Debug.Log($"[ItemCollector] Attempting to pick up: {item.itemName}");

        // Check if item is networked
        bool isNetworkedItem = item.NetworkObject != null && item.NetworkObject.IsSpawned;
        
        if (isNetworkedItem)
        {
            RequestPickupServerRpc(item.NetworkObjectId);
        }
        else
        {
            PickUpItemLocal(item);
        }
    }

    private void PickUpItemLocal(Item item)
    {
        if (_playerInventory == null)
        {
            Debug.LogError("[ItemCollector] PlayerInventory is null!");
            return;
        }

        bool added = _playerInventory.TryAddItem(item.Preset, item.quantity);
        Debug.Log($"[ItemCollector] Local pickup result: {added}");
        
        if (added)
        {
            Destroy(item.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ulong itemNetworkId, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[Server] Pickup request from client {senderClientId} for item {itemNetworkId}");

        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId, out var itemNetObj))
        {
            Debug.LogWarning($"[Server] Item {itemNetworkId} not found in spawn manager");
            return;
        }

        var item = itemNetObj.GetComponent<Item>();
        if (item == null)
        {
            Debug.LogWarning("[Server] NetworkObject has no Item component");
            return;
        }

        if (!NetworkManager.ConnectedClients.TryGetValue(senderClientId, out var clientData))
        {
            Debug.LogWarning($"[Server] Client {senderClientId} not found");
            return;
        }

        var playerInventory = clientData.PlayerObject.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
            Debug.LogWarning($"[Server] No PlayerInventory on client {senderClientId}'s player");
            return;
        }

        bool added = playerInventory.TryAddItem(item.Preset, item.quantity);
        Debug.Log($"[Server] Added to inventory: {added}");

        if (added && itemNetObj.IsSpawned)
        {
            itemNetObj.Despawn(true);
        }
    }
}
