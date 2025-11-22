using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class InventoryController : NetworkBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;

    [Header("Testing")]
    [SerializeField] private ItemPreset testItem;

    protected NetworkList<InventoryItemData> networkInventory;
    protected bool isInitialized = false;

    protected virtual void Awake()
    {
        // Initialize NetworkList in Awake
        if (networkInventory == null)
        {
            networkInventory = new NetworkList<InventoryItemData>();
        }
    }

    [ContextMenu("Add test item")]
    private void AddTestItem()
    {
        if (testItem != null)
        {
            TryAddItem(testItem, 1);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Debug.Log($"[InventoryController] OnNetworkSpawn - IsServer: {IsServer}, IsOwner: {IsOwner}");
        
        // Only initialize UI for the owner (local player)
        if (IsOwner)
        {
            // Check if UIManager is ready
            if (UIManager.Instance != null && UIManager.Instance.InventoryPage != null)
            {
                InitializeInventory();
            }
            else
            {
                // Wait for UIManager to be ready
                Debug.Log("[InventoryController] Waiting for UIManager...");
                UIManager.OnUIReady += InitializeInventory;
            }
        }
    }

    protected virtual void InitializeInventory()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[InventoryController] UIManager.Instance is null!");
            return;
        }

        inventoryPanel = UIManager.Instance.InventoryPage;
        
        if (inventoryPanel == null)
        {
            Debug.LogError("[InventoryController] InventoryPage not assigned in UIManager!");
            return;
        }

        isInitialized = true;
        Debug.Log("[InventoryController] Inventory UI initialized successfully!");
        
        // Unsubscribe from the event
        UIManager.OnUIReady -= InitializeInventory;
    }

    public virtual bool TryAddItem(ItemPreset preset, int quantity)
    {
        if (preset == null)
        {
            Debug.LogWarning("[InventoryController] Preset is null!");
            return false;
        }

        Debug.Log($"[InventoryController] TryAddItem: {preset.itemName} x{quantity}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        string itemID = preset.itemID;

        // Add directly if we're the server/host
        if (IsServer)
        {
            bool added = TryStack(itemID, quantity) || TryAddNewItem(itemID, quantity);
            
            if (added)
            {
                OnItemAdded(itemID, quantity);
            }
            
            return added;
        }
        // Client needs to request from server
        else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            AddItemServerRpc(itemID, quantity);
            return true;
        }
        // Single player fallback
        else
        {
            Debug.LogWarning("[InventoryController] Not connected, adding locally");
            bool added = TryStack(itemID, quantity) || TryAddNewItem(itemID, quantity);
            if (added)
            {
                OnItemAdded(itemID, quantity);
            }
            return added;
        }
    }

    protected virtual void OnItemAdded(string itemID, int quantity)
    {
        Debug.Log($"[InventoryController] Item added: {itemID} x{quantity}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddItemServerRpc(string itemID, int quantity)
    {
        if (ItemDatabase.itemDatabaseInstance != null && 
            ItemDatabase.itemDatabaseInstance.TryGetItem(itemID, out var preset))
        {
            TryAddItem(preset, quantity);
        }
        else
        {
            Debug.LogWarning($"[InventoryController] ItemPreset not found for ID: {itemID}");
        }
    }

    protected bool TryStack(string itemID, int quantity)
    {
        // ✅ Convert to FixedString64Bytes for comparison
        FixedString64Bytes fixedItemId = itemID;
        
        for (int i = 0; i < networkInventory.Count; i++)
        {
            var inventoryItem = networkInventory[i];
            
            // ✅ Compare FixedString64Bytes to FixedString64Bytes
            if (inventoryItem.itemId != fixedItemId) continue;

            inventoryItem.quantity += quantity;
            networkInventory[i] = inventoryItem;
            Debug.Log($"[InventoryController] Stacked {quantity}x {itemID}. New quantity: {inventoryItem.quantity}");
            return true;
        }
        return false;
    }

    protected bool TryAddNewItem(string itemID, int quantity)
    {
        networkInventory.Add(new InventoryItemData
        {
            itemId = itemID, // Implicit conversion from string to FixedString64Bytes
            quantity = quantity
        });
        Debug.Log($"[InventoryController] Added new item: {itemID} x{quantity}");
        return true;
    }

    public bool HasItem(string itemID, int requiredAmount)
    {
        // ✅ Convert to FixedString64Bytes for comparison
        FixedString64Bytes fixedItemId = itemID;
        
        foreach (var item in networkInventory)
        {
            if (item.itemId == fixedItemId && item.quantity >= requiredAmount)
                return true;
        }
        return false;
    }

    public int GetItemCount(string itemID)
    {
        // ✅ Convert to FixedString64Bytes for comparison
        FixedString64Bytes fixedItemId = itemID;
        
        foreach (var item in networkInventory)
        {
            if (item.itemId == fixedItemId)
                return item.quantity;
        }
        return 0;
    }
    

    public virtual void RemoveItem(string itemID, int quantity)
    {
        if (IsServer)
        {
            RemoveItemInternal(itemID, quantity);
        }
        else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            RemoveItemServerRpc(itemID, quantity);
        }
        else
        {
            RemoveItemInternal(itemID, quantity);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveItemServerRpc(string itemID, int amount)
    {
        RemoveItemInternal(itemID, amount);
    }

    protected void RemoveItemInternal(string itemID, int amount)
    {
        // ✅ Convert to FixedString64Bytes
        FixedString64Bytes fixedItemId = itemID;
        
        for (int i = 0; i < networkInventory.Count; i++)
        {
            var entry = networkInventory[i];
            
            // ✅ Compare FixedString64Bytes to FixedString64Bytes
            if (entry.itemId != fixedItemId) continue;

            entry.quantity -= amount;
            if (entry.quantity <= 0)
                networkInventory.RemoveAt(i);
            else
                networkInventory[i] = entry;

            Debug.Log($"[InventoryController] Removed {amount}x {itemID}. Remaining: {entry.quantity}");
            
            OnItemRemoved(itemID, amount);
            break;
        }
    }

    protected virtual void OnItemRemoved(string itemID, int quantity)
    {
        // Override in child classes
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe if still subscribed
        UIManager.OnUIReady -= InitializeInventory;
        
        base.OnNetworkDespawn();
    }
}

// using Unity.Netcode;
// using UnityEngine;
//
// public class InventoryController : NetworkBehaviour
// {
//     public GameObject inventoryPanel;
//
//     public GameObject slotPrefab;
//     public static event System.Action OnUIReady;
//
//     public int slotCount;
//     public static InventoryController Instance { get; private set; }
//     protected NetworkList<InventoryItemData> networkInventory = new NetworkList<InventoryItemData>();
//
//     [SerializeField] private ItemPreset testItem;
//     [ContextMenu("Add test item")]
//     private void AddTestItem()
//     {
//         TryAddItem(testItem, 1);
//     }
//
//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         
//         Instance = this;
//     }
//     
//     private void Start()
//     {
//         // Invoke the event when UI is ready
//         OnUIReady?.Invoke();
//     }
//     
//     // public override void OnNetworkSpawn()
//     // {
//     //     inventoryPanel = UIManager.Instance?.InventoryPage; 
//     //     
//     //     if (inventoryPanel == null)
//     //     {
//     //         Debug.LogError("[InventoryController] Could not find Inventory UI panel for player!");
//     //         return;
//     //     }
//     //     // InitializeInventoryUI();
//     // }
//     public GameObject[] itemPrefabs;
//
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     // protected virtual void Awake()
//     // {
//     //     if (Instance != null && Instance != this)
//     //     {
//     //         Destroy(gameObject);
//     //         return;
//     //     }
//     //
//     //     Instance = this;
//     //     
//     //     // MUST initialize NetworkList here
//     //     networkInventory = new NetworkList<InventoryItemData>(
//     //         readPerm: NetworkVariableReadPermission.Owner,
//     //         writePerm: NetworkVariableWritePermission.Server
//     //     );
//     // }
//     // void Start()
//     // {
//     //     for (int i = 0; i < slotCount; i++) //Initializing all slots.
//     //     {
//     //         Slot slot = Instantiate(slotPrefab, inventoryPanel.transform)
//     //             .GetComponent<Slot>(); //Grabbing the game slot objec
//     //         if (i < itemPrefabs.Length) //Check if an item fits in the slot.
//     //         {
//     //             GameObject item = Instantiate(itemPrefabs[i], slot.transform); //Places item on the slot.
//     //             item.GetComponent<RectTransform>().anchoredPosition =
//     //                 Vector2.zero; //Ensures the item is centered to the slot.
//     //             slot.currentItem = item;
//     //         }
//     //     }
//     // }
//
//     public bool TryAddItem(ItemPreset preset, int quantity)
//     {
//         if (preset == null)
//             return false;
//
//         // Get the itemId from the preset
//         string itemID = preset.itemID;
//
//         if (TryStack(itemID, quantity))
//         {
//             return true;
//         }
//
//         return TryAddNewItem(itemID, quantity);
//     }
//
//     private bool TryStack(string itemID, int quantity)
//     {
//         for (int i = 0; i < networkInventory.Count; i++)
//         {
//             var inventoryItem = networkInventory[i];
//             if (inventoryitem.itemID.ToString().ToString() != itemID)
//                 continue;
//
//             inventoryItem.quantity += quantity;
//             networkInventory[i] = inventoryItem;
//             Debug.Log($"Added {inventoryitem.itemID.ToString().ToString()} to inventory as a stack");
//             return true;
//         }
//         Debug.LogError($"Failed to add {itemID} to inventory");
//         return false;
//     }
//
//     private bool TryAddNewItem(string itemID, int quantity)
//     {
//         networkInventory.Add(new InventoryItemData
//         {
//             itemId = itemID, // Direct assignment, no parsing
//             quantity = quantity
//         });
//         Debug.Log($"Added {itemID} to as a new item in inventory");
//         return true;
//     }
//     
//     public override void OnNetworkSpawn()
//     {
//         if (UIManager.Instance != null && UIManager.Instance.InventoryPage != null)
//         {
//             InitializeInventory();
//         }
//         else
//         {
//             // Subscribe to UI ready event
//             UIManager.OnUIReady += InitializeInventory;
//         }
//     }
//
//     private void InitializeInventory()
//     {
//         inventoryPanel = UIManager.Instance?.InventoryPage;
//     
//         if (inventoryPanel == null)
//         {
//             Debug.LogError("[InventoryController] Could not find Inventory UI panel!");
//             return;
//         }
//
//         isInitialized = true;
//         Debug.Log("[InventoryController] Inventory initialized!");
//     
//         // Unsubscribe if we subscribed
//         UIManager.OnUIReady -= InitializeInventory;
//     }
//
//     private void OnDestroy()
//     {
//         // Clean up subscription
//         if (UIManager.Instance != null)
//         {
//             UIManager.OnUIReady -= InitializeInventory;
//         }
//     }
// }

// public bool AddItem(GameObject itemPrefab)
    // {
    //     Item itemToAdd = itemPrefab.GetComponent<Item>();
    //     if (itemToAdd == null) return false;
    //     
    //     //Check if we have this item
    //     foreach (Transform slotTransform in inventoryPanel.transform)
    //     {
    //         Slot slot = slotTransform.GetComponent<Slot>();
    //         if (slot != null && slot.currentItem != null)
    //         {
    //            Item slotItem = slot.currentItem.GetComponent<Item>();
    //            if (slotItem != null && slotItem.itemName == itemToAdd.itemName)
    //            {
    //                //same item & stack 
    //                slotItem.AddToSTack();
    //                return true;
    //            }
    //         }
    //     }
    //     
    //     //Look for an empty slot
    //     foreach (Transform slotTransform in inventoryPanel.transform)
    //     {
    //         Slot slot = slotTransform.GetComponent<Slot>();
    //         if (slot != null && slot.currentItem == null)
    //         {
    //             GameObject newItem = Instantiate(itemPrefab, slotTransform); //Create item in the inventory
    //             newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Item is centered in the middle of the slot
    //             slot.currentItem = newItem;
    //             return true;
    //         }
    //     }
    //     Debug.Log("Inventory is full!");
    //     return false;
    // }


// using System;
// using System.Collections;
// using Unity.Netcode;
// using UnityEngine;
// using System.Collections.Generic;
// using Random = UnityEngine.Random;
//
// // Attach this to your PLAYER prefab, NOT a scene object
//
//
// // [System.Serializable]
// // public struct InventoryItem : INetworkSerializable, System.IEquatable<InventoryItem>, ScriptableObject
// // {
// //     
// // }
//
// public class InventoryController : NetworkBehaviour
// {
//     //NEW
//     //old
//     public GameObject inventoryPanel;
//     public GameObject slotPrefab;
//     public int slotCount = 20;
//     
//     // ADD THIS: Array of item prefabs indexed by itemId
//     public GameObject[] itemPrefabs;
//     
//     private List<Slot> slots = new List<Slot>();
//     private NetworkList<InventoryItemData> networkInventory;
//     
//     private void Awake()
//     {
//         networkInventory = new NetworkList<InventoryItemData>(
//             readPerm: NetworkVariableReadPermission.Owner,
//             writePerm: NetworkVariableWritePermission.Server
//         );
//     }
//
//     public bool TryAddItem(ItemPreset preset, int quantity)
//     {
//         
//     }
//     public override void OnNetworkSpawn()
//     {
//         Debug.Log($"[OnNetworkSpawn] IsOwner: {IsOwner}");
//
//         if (!IsOwner) return;
//         
//         inventoryPanel = UIManager.Instance?.InventoryPage;
//
//         if (inventoryPanel == null)
//         {
//             Debug.LogError("InventoryPanel not found via UIManager!");
//             return;
//         }
//
//         InitializeInventoryUI();
//         networkInventory.OnListChanged += OnInventoryChanged;
//         inventoryPanel.SetActive(true);
//     }
//     private void InitializeInventoryUI()
//     {
//         Debug.Log("[InitializeInventoryUI] Starting initialization");
//
//         if (inventoryPanel == null)
//         {
//             Debug.LogError("[InitializeInventoryUI] inventoryPanel is NULL!");
//             return;
//         }
//     
//         if (slotPrefab == null)
//         {
//             Debug.LogError("[InitializeInventoryUI] slotPrefab is NULL!");
//             return;
//         }
//     
//         Debug.Log($"[InitializeInventoryUI] inventoryPanel: {inventoryPanel.name}, slotPrefab: {slotPrefab.name}");
//     
//         // Clear existing slots
//         foreach (Transform child in inventoryPanel.transform)
//         {
//             Destroy(child.gameObject);
//         }
//         slots.Clear();
//     
//         Debug.Log($"[InitializeInventoryUI] Creating {slotCount} slots...");
//     
//         // Create slots
//         for (int i = 0; i < slotCount; i++)
//         {
//             GameObject slotObj = Instantiate(slotPrefab, inventoryPanel.transform);
//             Slot slot = slotObj.GetComponent<Slot>();
//         
//             if (slot == null)
//             {
//                 Debug.LogError($"[InitializeInventoryUI] Slot {i} has no Slot component!");
//             }
//         
//             slots.Add(slot);
//         }
//     
//         Debug.Log($"[InitializeInventoryUI] Created {slots.Count} slots");
//     
//         // Sync with network inventory
//         RefreshUI();
//     }
//     
//     private void OnInventoryChanged(NetworkListEvent<InventoryItemData> changeEvent)
//     {
//         Debug.Log($"[OnInventoryChanged] Inventory changed! Event type: {changeEvent.Type}");
//         if (IsOwner)
//         {
//             Debug.Log("[OnInventoryChanged] Is owner - refreshing UI");
//             RefreshUI();
//         }
//         else
//         {
//             Debug.Log("[OnInventoryChanged] NOT owner - skipping refresh");
//         }
//     }
//     
//     private void RefreshUI()
//     {
//         Debug.Log($"[RefreshUI] Starting refresh. NetworkInventory count: {networkInventory.Count}, Slots count: {slots.Count}");
//     
//         // Clear all slots
//         foreach (Slot slot in slots)
//         {
//             if (slot.currentItem != null)
//             {
//                 Destroy(slot.currentItem);
//                 slot.currentItem = null;
//             }
//         }
//     
//         // Populate slots with items from network inventory
//         for (int i = 0; i < networkInventory.Count && i < slots.Count; i++)
//         {
//             InventoryItemData itemData = networkInventory[i];
//             Debug.Log($"[RefreshUI] Slot {i}: itemId={itemData.itemId}, quantity={itemData.quantity}");
//         
//             if (itemData.itemId >= 0)
//             {
//                 CreateItemInSlot(slots[i], itemData);
//             }
//         }
//     }
//     
//     
//     private void CreateItemInSlot(Slot slot, InventoryItemData itemData)
//     {
//         Debug.Log($"[CreateItemInSlot] Creating item {itemData.itemId} in slot");
//     
//         GameObject itemPrefab = GetItemPrefabById(itemData.itemId);
//         if (itemPrefab == null)
//         {
//             Debug.LogError($"[CreateItemInSlot] Item prefab is NULL for itemId: {itemData.itemId}");
//             return;
//         }
//     
//         Debug.Log($"[CreateItemInSlot] Found prefab: {itemPrefab.name}");
//     
//         GameObject newItem = Instantiate(itemPrefab, slot.transform);
//         newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//     
//         Debug.Log($"[CreateItemInSlot] Instantiated {newItem.name}");
//     
//         Item itemComponent = newItem.GetComponent<Item>();
//         if (itemComponent != null)
//         {
//             itemComponent.quantity = itemData.quantity;
//             Debug.Log($"[CreateItemInSlot] Set quantity to {itemData.quantity}");
//         
//             // Check if UpdateQuantityDisplay exists
//             if (itemComponent.GetType().GetMethod("UpdateQuantityDisplay") != null)
//             {
//                 itemComponent.UpdateQuantityDisplay();
//             }
//         }
//         else
//         {
//             Debug.LogError($"[CreateItemInSlot] No Item component on {newItem.name}");
//         }
//     
//         slot.currentItem = newItem;
//         Debug.Log($"[CreateItemInSlot] Item successfully added to slot");
//     }
//     
//     public void AddItem(int itemId, int quantity = 1)
//     {
//         if (!IsOwner) return;
//         Debug.Log("Adding " + itemId);
//         AddItemServerRpc(itemId, quantity);
//     }
//     
//     [ServerRpc]
//     private void AddItemServerRpc(int itemId, int quantity)
//     {
//         // Try to stack with existing item
//         for (int i = 0; i < networkInventory.Count; i++)
//         {
//             InventoryItemData itemData = networkInventory[i];
//             if (itemData.itemId == itemId)
//             {
//                 Debug.Log("Stacking " + itemId);
//
//                 itemData.quantity += quantity;
//                 networkInventory[i] = itemData;
//                 
//                 NotifyPickupClientRpc(itemId);
//                 return;
//             }
//         }
//         
//         // Add to empty slot
//         if (networkInventory.Count < slotCount)
//         {
//             Debug.Log("Adding to empty slot " + itemId);
//
//             networkInventory.Add(new InventoryItemData
//             {
//                 itemId = itemId,
//                 quantity = quantity
//             });
//             
//             NotifyPickupClientRpc(itemId);
//         }
//         else
//         {
//             // Inventory full - notify client
//             InventoryFullClientRpc();
//         }
//     }
//     
//     [ClientRpc]
//     private void NotifyPickupClientRpc(int itemId)
//     {
//         if (!IsOwner) return;
//         
//         // Show pickup notification
//         GameObject itemPrefab = GetItemPrefabById(itemId);
//         if (itemPrefab != null)
//         {
//             Item item = itemPrefab.GetComponent<Item>();
//             if (item != null)
//             {
//                 Sprite itemIcon = item.GetComponent<Sprite>();
//                 ItemPickUpUIController.Instance?.ShowItemPickUpUI(item.itemName, itemIcon);
//             }
//         }
//     }
//     
//     [ClientRpc]
//     private void InventoryFullClientRpc()
//     {
//         if (!IsOwner) return;
//         Debug.Log("Inventory is full!");
//         // Show UI message
//     }
//     
//     private GameObject GetItemPrefabById(int itemId)
//     {
//         // NOW THIS ACTUALLY RETURNS SOMETHING
//         if (itemPrefabs != null && itemId >= 0 && itemId < itemPrefabs.Length)
//         {
//             return itemPrefabs[itemId];
//         }
//         
//         Debug.LogError($"Item prefab not found for itemId: {itemId}");
//         return null;
//     }
//     
//     // Add these to your PlayerInventoryController class
//
//     [ServerRpc]
//     public void DropItemServerRpc(int slotIndex, int quantity)
//     {
//         if (slotIndex < 0 || slotIndex >= networkInventory.Count) return;
//         
//         InventoryItemData itemData = networkInventory[slotIndex];
//         if (itemData.itemId < 0) return;
//         
//         // Find player position
//         Transform playerTransform = transform;
//         CircleCollider2D detector = GetComponentInChildren<CircleCollider2D>();
//         float pickupRadius = detector != null ? detector.radius * transform.localScale.x : 0f;
//         
//         Vector2 direction = Random.insideUnitCircle.normalized;
//         float distance = Random.Range(pickupRadius + 2f, 3f);
//         Vector2 dropPosition = (Vector2)playerTransform.position + (direction * distance);
//         
//         // Spawn the item in world
//         GameObject itemPrefab = ItemDatabase.Instance.GetItemPrefab(itemData.itemId);
//         if (itemPrefab != null)
//         {
//             GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
//             NetworkObject networkObject = droppedItem.GetComponent<NetworkObject>();
//             
//             if (networkObject != null)
//             {
//                 networkObject.Spawn();
//                 
//                 Item item = droppedItem.GetComponent<Item>();
//                 if (item != null)
//                 {
//                     item.quantity = quantity;
//                 }
//             }
//         }
//         
//         // Remove from inventory
//         if (quantity >= itemData.quantity)
//         {
//             networkInventory.RemoveAt(slotIndex);
//         }
//         else
//         {
//             itemData.quantity -= quantity;
//             networkInventory[slotIndex] = itemData;
//         }
//     }
//
//     [ServerRpc]
//     public void StackItemsServerRpc(int targetSlotIndex, int quantityToAdd)
//     {
//         if (targetSlotIndex < 0 || targetSlotIndex >= networkInventory.Count) return;
//         
//         InventoryItemData itemData = networkInventory[targetSlotIndex];
//         itemData.quantity += quantityToAdd;
//         networkInventory[targetSlotIndex] = itemData;
//     }
//
//     [ServerRpc]
//     public void SplitStackServerRpc(int slotIndex)
//     {
//         if (slotIndex < 0 || slotIndex >= networkInventory.Count) return;
//         if (networkInventory.Count >= slotCount) return; // No space
//         
//         InventoryItemData itemData = networkInventory[slotIndex];
//         if (itemData.quantity <= 1) return;
//         
//         int splitAmount = itemData.quantity / 2;
//         
//         // Reduce original stack
//         itemData.quantity -= splitAmount;
//         networkInventory[slotIndex] = itemData;
//         
//         // Add new stack
//         networkInventory.Add(new InventoryItemData
//         {
//             itemId = itemData.itemId,
//             quantity = splitAmount
//         });
//     }
// }
//