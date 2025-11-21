using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private string viewName = "PlayerInventory";
    [SerializeField] private Slot[] slots;

    private InventoryController _inventoryController;

    public static Dictionary<string, InventoryView> instances = new Dictionary<string, InventoryView>();

    private void Awake()
    {
        Debug.Log($"[InventoryView] Awake called for viewName: {viewName}");

        if (instances.ContainsKey(viewName))
        {
            Debug.LogWarning($"[InventoryView] Duplicate InventoryView for viewName: {viewName}. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        instances[viewName] = this;
        Debug.Log($"[InventoryView] Registered instance for viewName: {viewName}");

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("[InventoryView] No slots assigned!");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                Debug.LogError($"[InventoryView] Slot {i} is null!");
                continue;
            }

            slots[i].Init(this, i);
            Debug.Log($"[InventoryView] Initialized slot {i}");
        }

        Debug.Log($"[InventoryView] All slots initialized for viewName: {viewName}");
    }

    private void OnDestroy()
    {
        if (instances != null && instances.ContainsKey(viewName))
        {
            instances.Remove(viewName);
            Debug.Log($"[InventoryView] Unregistered instance for viewName: {viewName}");
        }
    }

    public void RedrawEverything(InventoryItemData[] inventoryItems)
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("[InventoryView] Cannot redraw - no slots available!");
            return;
        }

        int itemCount = inventoryItems.Length;
        int slotCount = slots.Length;

        for (int i = 0; i < slotCount; i++)
        {
            if (i < itemCount)
                slots[i].SetItem(inventoryItems[i]);
            else
                slots[i].ResetTile();
        }

        if (itemCount > slotCount)
            Debug.LogWarning($"[InventoryView] Inventory has more items ({itemCount}) than slots ({slotCount})!");
        
        Debug.Log($"[InventoryView] Redrawn {itemCount} items across {slotCount} slots");
    }
}