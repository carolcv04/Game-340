using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class Item : NetworkBehaviour,  INetworkSerializable, System.IEquatable<Item>
{
    [SerializeField] private ItemPreset preset;
    public ItemPreset Preset => preset;

    public int quantity = 1;
    private TMP_Text quantityText;
    public string itemName;
    private string itemID;
    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
        Debug.Log($"Item {preset.itemName} has ID {preset.itemID}");
        itemName = preset.itemName;
        itemID = preset.itemID;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemID);
        serializer.SerializeValue(ref quantity);
    }
    
    public bool Equals(Item other)
    {
        if (other == null) return false;
        return preset != null 
               && other.preset != null 
               && preset.itemID == other.preset.itemID 
               && quantity == other.quantity;
    }
    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }
    public void RequestDestroy()
    {
        if (IsServer)
        {
            // If we're the server, destroy directly
            DestroyItem();
        }
        else
        {
            // If we're a client, ask the server to destroy
            RequestDestroyServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestDestroyServerRpc()
    {
        DestroyItem();
    }
    
    private void DestroyItem()
    {
        // Despawn from network first
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
        
        // Then destroy the GameObject
        Destroy(gameObject);
    }

    public void AddToSTack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromSTack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        Item cloneItem = clone.GetComponent<Item>();
        cloneItem.quantity = newQuantity;
        cloneItem.UpdateQuantityDisplay();
        return clone;
    }
    public virtual void PickUp()
    {
        Sprite itemIcon = GetComponent<Image>().sprite;
        if (ItemPickUpUIController.Instance != null)
        {
            ItemPickUpUIController.Instance.ShowItemPickUpUI(itemName, itemIcon);
        }
    }
    public virtual void UseItem()
    {
        Debug.Log($"Using item {itemName}");
    }
    
    public string GetItemId()
    {
        return itemID;
    }


}

