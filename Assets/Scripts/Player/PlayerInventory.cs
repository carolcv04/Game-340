using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : InventoryController
{
    public static PlayerInventory LocalInstance { get; private set; }

    [SerializeField] private string viewName = "PlayerInventory";
    private InventoryView _inventoryView;

    protected override void Awake()
    {
        base.Awake(); // Call parent Awake to initialize NetworkList
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[PlayerInventory] OnNetworkSpawn - IsOwner: {IsOwner}");

        if (IsOwner)
        {
            LocalInstance = this;
            StartCoroutine(WaitForInventoryView());
        }

        networkInventory.OnListChanged += OnInventoryChanged;
    }

    private IEnumerator WaitForInventoryView()
    {
        Debug.Log("[PlayerInventory] Waiting for InventoryView...");
        
        while (InventoryView.instances == null || !InventoryView.instances.ContainsKey(viewName))
        {
            yield return null;
        }

        _inventoryView = InventoryView.instances[viewName];
        Debug.Log("[PlayerInventory] InventoryView found!");
        RefreshInventoryView();
    }

    private void OnInventoryChanged(NetworkListEvent<InventoryItemData> changeEvent)
    {
        Debug.Log($"[PlayerInventory] Inventory changed, IsOwner: {IsOwner}");
        if (IsOwner)
        {
            RefreshInventoryView();
        }
    }

    protected override void OnItemAdded(string itemID, int quantity)
    {
        base.OnItemAdded(itemID, quantity);
        
        if (IsOwner)
        {
            RefreshInventoryView();
        }
    }

    protected override void OnItemRemoved(string itemID, int quantity)
    {
        base.OnItemRemoved(itemID, quantity);
        
        if (IsOwner)
        {
            RefreshInventoryView();
        }
    }

    public void RefreshInventoryView()
    {
        if (_inventoryView == null)
        {
            Debug.LogWarning("[PlayerInventory] InventoryView is null, cannot refresh");
            return;
        }

        InventoryItemData[] items = new InventoryItemData[networkInventory.Count];
        for (int i = 0; i < networkInventory.Count; i++)
        {
            items[i] = networkInventory[i];
        }

        _inventoryView.RedrawEverything(items);
        Debug.Log($"[PlayerInventory] Refreshed view with {items.Length} items");
    }

    public override void OnNetworkDespawn()
    {
        networkInventory.OnListChanged -= OnInventoryChanged;
        
        if (LocalInstance == this)
        {
            LocalInstance = null;
        }

        base.OnNetworkDespawn();
    }
}