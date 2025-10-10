using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Item> itemPrefabs;
    private Dictionary<int, GameObject> itemDictionary;

    public void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs != null)
            {
                itemPrefabs[i].itemID = i++;
            }

            foreach (Item item in itemPrefabs)
            {
                itemDictionary[item.itemID] = item.gameObject;
            }
        }
    }

    public GameObject GetItemPrefab(int itemID)
    {
        itemDictionary.TryGetValue(itemID, out GameObject prefab);

        if (prefab == null)
        {
            Debug.LogWarning($"Found prefab: {itemID}");
        }
        return prefab;

    }
}
