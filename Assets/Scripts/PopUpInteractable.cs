// using System;
// using TMPro;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.UI;
//
// [System.Serializable]
// public struct BuildingRequirement
// {
//     public ItemPreset itemPrefab;
//     public int amount;
// }
//
// public class PopUpInteractable : Interactable
// {
//     [Header("UI")]
//     public GameObject popupUI;
//     public TextMeshProUGUI popupUIText;
//     public Button confirmButton;
//
//     [Header("Building")]
//     public GameObject building;                 // Building object in scene
//     private BuildingData buildingData;          // NEW: reference to data script
//     public BuildingRequirement[] requirements;
//     private bool hasClickedConfirm = false;
//
//     private PopUpInteractable activeInteractable;
//
//
//     private void Awake()
//     {
//         // Cache the BuildingData script
//         if (building != null)
//             buildingData = building.GetComponent<BuildingData>();
//
//         if (buildingData == null)
//             Debug.LogError($"[PopUpInteractable] No BuildingData found on {building.name}!");
//         
//         if (buildingData != null)
//             Debug.Log($"[PopUpInteractable] {gameObject.name} has buildingID {buildingData.buildingID}");
//     }
//
//
//     // -----------------------------
//     // When player opens interactable
//     // -----------------------------
//     protected override void OpenInteractable()
//     {
//         hasClickedConfirm = false; // Reset for this popup
//
//         if (popupUI != null)
//             popupUI.SetActive(true);
//
//         // Build requirement text
//         string requiredItemsText = "Required items:\n";
//         foreach (var req in requirements)
//             requiredItemsText += $"- ({req.amount}) {req.itemPrefab.name}\n";
//
//         popupUIText.text = requiredItemsText;
//
//         confirmButton.onClick.RemoveAllListeners();
//         confirmButton.onClick.AddListener(() => OnConfirmButton(this));
//     }
//
//
//     // -----------------------------
//     // Confirm button = request build
//     // -----------------------------
//     public void OnConfirmButton(PopUpInteractable popup)
//     {
//         if (popup.hasClickedConfirm)
//             return;
//
//         popup.hasClickedConfirm = true;
//
//         if (popup.buildingData == null)
//         {
//             Debug.LogError("[PopUp] Cannot build: BuildingData missing!");
//             return;
//         }
//
//         if (popup.buildingData.hasBeenBuilt)
//         {
//             Debug.Log("[PopUp] This building is already built.");
//             return;
//         }
//
//         // ✅ Pass the buildingID directly (it's already a string)
//         popup.RequestBuildServerRpc(popup.buildingData.buildingID);
//
//         if (popup.popupUI != null)
//             popup.popupUI.SetActive(false);
//     }
//     // -----------------------------
//     // SERVER: Validate & build
//     // -----------------------------
//     [ServerRpc(RequireOwnership = false)]
//     private void RequestBuildServerRpc(string requestedBuildingID, ServerRpcParams rpcParams = default)
//     {
//         Debug.Log($"[Server] Build request for '{requestedBuildingID}'");
//
//         // Find the correct interactable by ID
//         PopUpInteractable target = null;
//         foreach (var pop in FindObjectsOfType<PopUpInteractable>())
//         {
//             if (pop.buildingData != null && pop.buildingData.buildingID.ToString() == requestedBuildingID)
//             {
//                 target = pop;
//                 break;
//             }
//         }
//
//         if (target == null)
//         {
//             Debug.LogError($"[Server] No PopUpInteractable found for ID '{requestedBuildingID}'");
//             return;
//         }
//
//         if (target.buildingData.hasBeenBuilt)
//         {
//             Debug.Log("[Server] Already built — ignoring.");
//             return;
//         }
//
//         // Get player inventory
//         PlayerInventory inv = GetPlayerInventory(rpcParams.Receive.SenderClientId);
//         if (inv == null)
//         {
//             Debug.LogError("[Server] Could not find player's inventory!");
//             return;
//         }
//
//         // Check requirements
//         if (!target.CanBuildOnServer(inv))
//         {
//             Debug.Log("[Server] Build failed: missing materials.");
//             return;
//         }
//
//         // Remove materials
//         foreach (var req in target.requirements)
//         {
//             inv.RemoveItemServerRpc(req.itemPrefab.itemID, req.amount);
//         }
//
//         // Mark building as built
//         target.buildingData.SetBuilt(true);
//
//         // Activate object on all clients
//         target.ActivateBuildingClientRpc();
//
//         target.SetInteracted(true);
//
//         Debug.Log($"[Server] Building '{requestedBuildingID}' successfully built!");
//         Debug.Log($"[Server] Target building = {target.buildingData.buildingID}");
//     }
//
//
//     // -----------------------------
//     // Get player's inventory
//     // -----------------------------
//     private PlayerInventory GetPlayerInventory(ulong clientId)
//     {
//         foreach (var inv in FindObjectsOfType<PlayerInventory>())
//         {
//             if (inv.OwnerClientId == clientId)
//                 return inv;
//         }
//         return null;
//     }
//
//
//     // -----------------------------
//     // Requirement checking
//     // -----------------------------
//     private bool CanBuildOnServer(PlayerInventory inventory)
//     {
//         if (inventory == null)
//         {
//             Debug.LogError("[Server] PlayerInventory is null!");
//             return false;
//         }
//
//         foreach (var req in requirements)
//         {
//             if (!inventory.HasItem(req.itemPrefab.itemID, req.amount))
//             {
//                 Debug.Log($"[Server] Missing {req.amount}x {req.itemPrefab.name}");
//                 return false;
//             }
//         }
//
//         return true; // All requirements are met
//     }
//
//
//     // -----------------------------
//     // CLIENT: Activate building
//     // -----------------------------
//     [ClientRpc]
//     private void ActivateBuildingClientRpc()
//     {
//         // Don't SetActive - just let BuildingData show itself
//         Debug.Log($"[Client] Building '{buildingData.buildingID}' activated (visibility handled by BuildingData)");
//     }
// }

using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct BuildingRequirement
{
    public ItemPreset itemPrefab;
    public int amount;
}

public class PopUpInteractable : Interactable
{
    [Header("UI")]
    public GameObject popupUI;
    public TextMeshProUGUI popupUIText;
    public Button confirmButton;

    [Header("Building")]
    public GameObject building;
    private BuildingData buildingData;
    public BuildingRequirement[] requirements;
    private bool hasClickedConfirm = false;

    private void Awake()
    {
        if (building != null)
            buildingData = building.GetComponent<BuildingData>();

        if (buildingData == null)
            Debug.LogError($"[PopUpInteractable] No BuildingData found on {building?.name}!");
        else
            Debug.Log($"[PopUpInteractable] {gameObject.name} has buildingID {buildingData.buildingID}");
    }

    protected override void OpenInteractable()
    {
        hasClickedConfirm = false;

        if (popupUI != null)
            popupUI.SetActive(true);

        string requiredItemsText = "Required items:\n";
        foreach (var req in requirements)
            requiredItemsText += $"- ({req.amount}) {req.itemPrefab.name}\n";

        popupUIText.text = requiredItemsText;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => OnConfirmButton(this));
    }

    private void OnConfirmButton(PopUpInteractable interactable)
    {
        if (interactable.hasClickedConfirm) return;
        interactable.hasClickedConfirm = true;

        if (interactable.buildingData == null)
        {
            Debug.LogError("[PopUp] Cannot build: BuildingData missing!");
            return;
        }

        if (interactable.buildingData.hasBeenBuilt)
        {
            Debug.Log("[PopUp] This building is already built.");
            return;
        }

        Debug.Log($"[PopUp] Attempting to build '{buildingData.buildingID}'");

        // Get player inventory (in single-player, just find the local one)
        PlayerInventory inv = PlayerInventory.LocalInstance;
        if (inv == null)
        {
            Debug.LogError("[PopUp] Could not find player's inventory!");
            if (interactable.popupUI != null) interactable.popupUI.SetActive(false);
            return;
        }

        // Check if player has all required items
        if (!CanBuild(inv))
        {
            Debug.Log("[PopUp] Build failed: missing materials.");
            if (popupUI != null) interactable.popupUI.SetActive(false);
            return;
        }

        // Remove materials from inventory
        foreach (var req in requirements)
        {
            inv.RemoveItem(req.itemPrefab.itemID, req.amount);
            Debug.Log($"[PopUp] Removed {req.amount}x {req.itemPrefab.name}");
        }

        Debug.Log($"[PopUp] About to call SetBuilt on buildingData: {buildingData != null}, buildingData is: {buildingData?.gameObject.name}");
        buildingData.SetBuilt(true);
        Debug.Log($"[PopUp] ✅ Building '{buildingData.buildingID}' successfully built!");;

        SetInteracted(true);

        if (interactable.popupUI != null)
            interactable.popupUI.SetActive(false);
        
    }

    private bool CanBuild(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError("[PopUp] PlayerInventory is null!");
            return false;
        }

        foreach (var req in requirements)
        {
            if (!inventory.HasItem(req.itemPrefab.itemID, req.amount))
            {
                Debug.Log($"[PopUp] Missing {req.amount}x {req.itemPrefab.name}");
                return false;
            }
        }

        return true;
    }
}