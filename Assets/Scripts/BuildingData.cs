// using System;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.Tilemaps;
//
// public class BuildingData : NetworkBehaviour
// {
//     public string buildingID = "";
//     
//     private NetworkVariable<bool> isBuilt = new NetworkVariable<bool>(false);
//     
//     private TilemapRenderer tilemapRenderer;
//     
//     public bool hasBeenBuilt => isBuilt.Value;
//     
//     private void Awake()
//     {
//         if (string.IsNullOrEmpty(buildingID))
//         {
//             buildingID = System.Guid.NewGuid().ToString();
//             Debug.LogWarning($"[BuildingData] No ID set for {gameObject.name}, generated: {buildingID}");
//         }
//         else
//         {
//             Debug.Log($"[BuildingData] {gameObject.name} has ID: {buildingID}");
//         }
//         
//         // Get the tilemap renderer
//         tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
//         if (tilemapRenderer != null)
//         {
//             tilemapRenderer.enabled = false; // Start hidden
//             Debug.Log($"[BuildingData] {buildingID} tilemap renderer hidden");
//         }
//     }
//     
//     public override void OnNetworkSpawn()
//     {
//         base.OnNetworkSpawn();
//         
//         Debug.Log($"[BuildingData] OnNetworkSpawn for {buildingID}, isBuilt: {isBuilt.Value}");
//         
//         // Subscribe to value changes
//         isBuilt.OnValueChanged += OnBuiltStatusChanged;
//         
//         // Update visibility based on current state
//         UpdateVisibility();
//     }
//     
//     private void OnBuiltStatusChanged(bool oldValue, bool newValue)
//     {
//         Debug.Log($"[BuildingData] {buildingID} status changed from {oldValue} to {newValue}");
//         UpdateVisibility();
//     }
//     
//     private void UpdateVisibility()
//     {
//         if (tilemapRenderer != null)
//         {
//             tilemapRenderer.enabled = isBuilt.Value;
//             Debug.Log($"[BuildingData] {buildingID} tilemap visibility: {isBuilt.Value}");
//         }
//     }
//     
//     public void SetBuilt(bool built)
//     {
//         if (IsServer)
//         {
//             Debug.Log($"[BuildingData] SetBuilt called for {buildingID}: {built}");
//             isBuilt.Value = built;
//         }
//     }
// }


using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingData : MonoBehaviour // Changed from NetworkBehaviour
{
    public string buildingID = "";

    public bool _isBuilt = false; // Changed from NetworkVariable

    private TilemapRenderer tilemapRenderer;

    public bool hasBeenBuilt => _isBuilt;

    // Event to notify when building is built
    public event Action<bool> OnBuiltStatusChanged;

    private void Awake()
    {
        if (string.IsNullOrEmpty(buildingID))
        {
            buildingID = System.Guid.NewGuid().ToString();
            Debug.LogWarning($"[BuildingData] No ID set for {gameObject.name}, generated: {buildingID}");
        }
        else
        {
            Debug.Log($"[BuildingData] {gameObject.name} has ID: {buildingID}");
        }

        // Get the tilemap renderer
        tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
        if (tilemapRenderer != null)
        {
            tilemapRenderer.enabled = false; // Start hidden
            Debug.Log($"[BuildingData] {buildingID} tilemap renderer hidden");
        }
        else
        {
            Debug.LogError($"[BuildingData] {buildingID} - No TilemapRenderer found!");
        }
    }

    private void UpdateVisibility()
    {
        if (tilemapRenderer != null)
        {
            tilemapRenderer.enabled = _isBuilt;
            Debug.Log($"[BuildingData] {buildingID} tilemap visibility set to: {_isBuilt}");
        }
        else
        {
            Debug.LogError($"[BuildingData] {buildingID} - tilemapRenderer is null");
        }
    }
    public void SetBuilt(bool isBuilt)
    {
        if (_isBuilt == isBuilt) return; // Avoid unnecessary updates

        _isBuilt = isBuilt;

        // Update visual state
        UpdateVisibility();

        // Notify listeners
        OnBuiltStatusChanged?.Invoke(_isBuilt);

        Debug.Log($"[BuildingData] {buildingID} set built: {_isBuilt}");
    }
}