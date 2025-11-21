using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerInventory : InventoryController
{
    public static PlayerInventory LocalInstance { get; private set; }
    private InventoryView _inventoryView;
    [SerializeField] private string viewName = "PlayerInventory"; 
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"[PlayerInventory] OnNetworkSpawn called on {gameObject.name} (Owner: {IsOwner})");

        if (!IsOwner)
        {
            Debug.Log("[PlayerInventory] Not owner — skipping local setup.");
            return;
        }

        // Register as the local player's inventory
        LocalInstance = this;
        Debug.Log("[PlayerInventory] LocalInstance set.");

        // Wait for InventoryView to be ready
        StartCoroutine(WaitForInventoryView());
    }

    private IEnumerator WaitForInventoryView()
    {
        Debug.Log("[PlayerInventory] Waiting for InventoryView to initialize...");
        
        // Wait until InventoryView.instances exists and contains our view
        while (InventoryView.instances == null || 
               !InventoryView.instances.ContainsKey(viewName))
        {
            yield return null; // Wait one frame
        }

        Debug.Log("[PlayerInventory] InventoryView found!");

        if (!InventoryView.instances.TryGetValue(viewName, out _inventoryView))
        {
            Debug.LogError($"[PlayerInventory] Could not retrieve InventoryView with name '{viewName}'!");
            yield break;
        }

        Debug.Log($"[PlayerInventory] Successfully connected to InventoryView: {_inventoryView.name}");

        // Subscribe to inventory changes
        networkInventory.OnListChanged += OnInventoryChanged;
        Debug.Log("[PlayerInventory] Subscribed to OnListChanged event.");

        // Do initial redraw
        RefreshInventoryView();
    }

    private void OnInventoryChanged(NetworkListEvent<InventoryItemData> changeEvent)
    {
        Debug.Log($"[PlayerInventory] OnInventoryChanged called — event type: {changeEvent.Type}");

        if (!IsOwner)
        {
            Debug.Log($"[PlayerInventory] Ignoring OnInventoryChanged (not owner).");
            return;
        }

        RefreshInventoryView();
    }

    private void RefreshInventoryView()
    {
        if (_inventoryView == null)
        {
            Debug.LogWarning("[PlayerInventory] Cannot refresh - InventoryView is null!");
            return;
        }

        // Convert NetworkList to array
        InventoryItemData[] items = new InventoryItemData[networkInventory.Count];
        for (int i = 0; i < networkInventory.Count; i++)
        {
            items[i] = networkInventory[i];
        }

        Debug.Log($"[PlayerInventory] Redrawing view with {items.Length} items.");
        _inventoryView.RedrawEverything(items);
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"[PlayerInventory] OnNetworkDespawn called on {gameObject.name}");

        if (IsOwner)
        {
            // Unsubscribe from events
            networkInventory.OnListChanged -= OnInventoryChanged;
            
            if (LocalInstance == this)
            {
                LocalInstance = null;
                Debug.Log("[PlayerInventory] Cleared LocalInstance.");
            }
        }

        base.OnNetworkDespawn();
    }
}