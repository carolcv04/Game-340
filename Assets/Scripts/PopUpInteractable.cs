using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public struct BuildingRequirement
{
    public ItemPreset itemPrefab;
    public int amount;
}

public class PopUpInteractable : Interactable
{
    public GameObject popupUI;
    public GameObject building;
    public TextMeshProUGUI popupUIText;
    public BuildingRequirement[] requirements;
    private static PopUpInteractable activeInteractable;
    public Button confirmButton;

    protected override void OpenInteractable()
    {
        string requiredItemsText = "Required items: ";
        activeInteractable = this;

        if (popupUI != null)
            popupUI.SetActive(true);

        if (popupUIText != null)
        {
            foreach (BuildingRequirement item in requirements)
            {
                requiredItemsText += "(" + item.amount.ToString() + ") " + item.itemPrefab.name.ToString() + " ";
            }

            popupUIText.text = requiredItemsText;
        }

        foreach (var r in requirements)
        {
            Debug.Log($"Req: {r.itemPrefab.name} (ID: {r.itemPrefab.itemID}), amount: {r.amount}");
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButton);
        }
        else
        {
            Debug.LogError("[PopUpInteractable] confirmButton is NULL! Assign it in Inspector!");
        }
    }

    public void OnConfirmButton()
    {
        Debug.Log("=== CONFIRM BUTTON CLICKED ===");
        
        if (!CanBuild())
        {
            Debug.LogWarning("❌ Missing required materials - build cancelled.");
            return;
        }
        
        Debug.Log("✅ Can build! Requesting server to build...");
        
        if (activeInteractable != null)
        {
            // Request the server to build instead of building locally
            RequestBuildServerRpc();
            
            // Close popup immediately for responsiveness
            popupUI.SetActive(false);
            activeInteractable = null;
        }
    }

    private bool CanBuild()
    {
        Debug.Log("=== CanBuild() CHECK START ===");
        
        // Try to get the inventory controller
        InventoryController inventory = InventoryController.Instance;
        
        // If InventoryController.Instance is null, try PlayerInventory (which inherits from InventoryController)
        if (inventory == null)
        {
            inventory = FindObjectOfType<PlayerInventory>();
            if (inventory != null)
            {
                Debug.Log("[PopUpInteractable] Using PlayerInventory as InventoryController");
            }
        }
        
        if (inventory == null)
        {
            Debug.LogError("No InventoryController found! Make sure InventoryController.Instance is set.");
            return false;
        }

        foreach (var req in requirements)
        {
            if (req.itemPrefab == null)
            {
                Debug.LogError("Requirement has null itemPrefab!");
                continue;
            }
            
            string requiredID = req.itemPrefab.itemID;
            int requiredAmount = req.amount;
            
            int currentAmount = inventory.GetItemCount(requiredID);
            bool hasEnough = inventory.HasItem(requiredID, requiredAmount);
            
            Debug.Log($"Checking {req.itemPrefab.name}:");
            Debug.Log($"  Required ID: '{requiredID}' (length: {requiredID.Length})");
            Debug.Log($"  Need: {requiredAmount}, Have: {currentAmount}");
            Debug.Log($"  HasItem result: {hasEnough}");
            
            if (!hasEnough)
            {
                Debug.LogError($"❌ FAILED: {req.itemPrefab.name} - need {requiredAmount}, have {currentAmount}");
                return false;
            }
            else
            {
                Debug.Log($"✅ PASSED: {req.itemPrefab.name}");
            }
        }

        Debug.Log("=== ✅ ALL REQUIREMENTS MET! ===");
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBuildServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("[Server] Build request received");
        
        // Double-check requirements on server (important for security)
        if (!CanBuildOnServer(rpcParams.Receive.SenderClientId))
        {
            Debug.Log("[Server] Build failed - missing materials");
            return;
        }

        // Remove items on server
        foreach (var req in requirements)
        {
            InventoryController.Instance.RemoveItemServerRpc(req.itemPrefab.itemID, req.amount);
        }

        // Activate building for all clients
        ActivateBuildingClientRpc();
        
        SetInteracted(true);
        
        Debug.Log("[Server] Building placed successfully");
    }

    private bool CanBuildOnServer(ulong clientId)
    {
        if (InventoryController.Instance == null) return false;

        foreach (var req in requirements)
        {
            if (!InventoryController.Instance.HasItem(req.itemPrefab.itemID, req.amount))
                return false;
        }

        return true;
    }

    [ClientRpc]
    private void ActivateBuildingClientRpc()
    {
        if (building != null)
        {
            building.SetActive(true);
            Debug.Log("[Client] Building activated");
        }
        else
        {
            Debug.LogError("[Client] Building GameObject is null!");
        }
    }

    // Alternative: If you want to keep the old Build() method for testing
    public void Build()
    {
        if (InventoryController.Instance == null)
        {
            Debug.LogError("InventoryController.Instance is null!");
            return;
        }
        
        foreach (var req in requirements)
        {
            InventoryController.Instance.RemoveItemServerRpc(req.itemPrefab.itemID, req.amount);
        }

        if (building != null)
        {
            building.SetActive(true);
            Debug.Log("Building activated locally");
        }
        else
        {
            Debug.LogError("Building GameObject is null!");
        }

        if (popupUI != null)
            popupUI.SetActive(false);

        SetInteracted(true);
    }
}