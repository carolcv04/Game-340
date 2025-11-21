using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    private Dictionary<string, ItemPreset> _itemPreset = new();
    public static ItemDatabase itemDatabaseInstance { get; private set; }
    private void Awake()
    {
        if (itemDatabaseInstance != null && itemDatabaseInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        itemDatabaseInstance = this;
        
        var presets = Resources.LoadAll<ItemPreset>("");
        foreach (var preset in presets)
        {
            if(!_itemPreset.TryAdd(preset.itemID, preset))
                Debug.LogError($"Failed to add item preset {preset.itemID}. Duplicate item preset ID found.");
        }
    }
    
    private void OnDestroy()
    {
        if (itemDatabaseInstance == this)
        {
            itemDatabaseInstance = null;
        }
    }

    public bool TryGetItem(string itemID, out ItemPreset preset)
    {
        if (string.IsNullOrEmpty(itemID))
        {
            preset = null;
            return false;
        }
        return _itemPreset.TryGetValue(itemID, out preset);
    }
}