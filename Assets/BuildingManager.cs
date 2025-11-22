using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    
    [Header("Buildings to Track")]
    [SerializeField] private List<BuildingData> requiredBuildings = new List<BuildingData>();
    
    private bool allBuildingsCompleted = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        Debug.Log($"[BuildingManager] ✅ Instance created. Required buildings in list: {requiredBuildings.Count}");
        
        // List all buildings
        for (int i = 0; i < requiredBuildings.Count; i++)
        {
            if (requiredBuildings[i] != null)
            {
                Debug.Log($"[BuildingManager] Building {i}: {requiredBuildings[i].gameObject.name} (ID: {requiredBuildings[i].buildingID})");
            }
            else
            {
                Debug.LogError($"[BuildingManager] Building {i} is NULL!");
            }
        }
    }
    
    // public override void OnNetworkSpawn()
    // {
    //     base.OnNetworkSpawn();
    //     Debug.Log($"[BuildingManager] OnNetworkSpawn called. IsServer: {IsServer}");
    // }
    
    private void Update()
    {
        // if (!IsServer) return;
        if (allBuildingsCompleted) return;
        
        // Check if all buildings are built
        bool allBuilt = true;
        int builtCount = 0;
        
        foreach (var building in requiredBuildings)
        {
            if (building != null && building.hasBeenBuilt)
            {
                builtCount++;
            }
            else if (building != null)
            {
                allBuilt = false;
            }
        }
        
        // Log status every few seconds
        if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
        {
            Debug.Log($"[BuildingManager] Status check: {builtCount}/{requiredBuildings.Count} buildings built");
        }
        
        // All buildings complete!
        if (allBuilt && requiredBuildings.Count > 0 && builtCount == requiredBuildings.Count)
        {
            allBuildingsCompleted = true;
            Debug.Log("[BuildingManager] ✅✅✅ ALL BUILDINGS COMPLETE! Triggering game end...");
            
            // Notify FountainGameManager
            if (FountainGameManager.Instance != null)
            {
                Debug.Log("[BuildingManager] Calling SetStreetCompleted");
                FountainGameManager.Instance.SetStreetCompleted(true);
            }
            else
            {
                Debug.LogError("[BuildingManager] FountainGameManager.Instance is null!");
            }
        }
    }
    
    public int GetBuiltBuildingsCount()
    {
        int count = 0;
        foreach (var building in requiredBuildings)
        {
            if (building != null && building.hasBeenBuilt)
                count++;
        }
        return count;
    }
    
    public void ResetAllBuildings()
    {
        // if (!IsServer) return;
    
        Debug.Log("[BuildingManager] Resetting all buildings...");
    
        allBuildingsCompleted = false;
    
        foreach (var building in requiredBuildings)
        {
            if (building != null)
            {
                building.SetBuilt(false); // Hide the building again
            }
        }
    }
    public int GetTotalBuildingsCount() => requiredBuildings.Count;
}