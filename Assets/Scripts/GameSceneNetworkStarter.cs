using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class GameSceneNetworkStarter : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(InitializeNetwork());
    }
    
    private IEnumerator InitializeNetwork()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[GameSceneNetworkStarter] NetworkManager not found in scene!");
            yield break;
        }
        
        if (!NetworkManager.Singleton.IsListening)
        {
            Debug.Log("[GameSceneNetworkStarter] Starting NetworkManager as host");
            NetworkManager.Singleton.StartHost();
            
            // Wait for network to fully initialize
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.Log("[GameSceneNetworkStarter] NetworkManager already running");
        }
        
        // Wait for FountainGameManager to spawn
        float timeout = 5f;
        float elapsed = 0f;
        
        while (FountainGameManager.Instance != null && !FountainGameManager.Instance.IsSpawned && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (FountainGameManager.Instance != null && FountainGameManager.Instance.IsSpawned)
        {
            Debug.Log("[GameSceneNetworkStarter] ✅ FountainGameManager spawned and ready!");
        }
        else
        {
            Debug.LogError("[GameSceneNetworkStarter] ❌ FountainGameManager failed to spawn!");
            
            // Try to manually spawn if it exists but isn't spawned
            if (FountainGameManager.Instance != null)
            {
                var networkObject = FountainGameManager.Instance.GetComponent<NetworkObject>();
                if (networkObject != null && !networkObject.IsSpawned)
                {
                    Debug.LogWarning("[GameSceneNetworkStarter] Attempting manual spawn...");
                    networkObject.Spawn();
                }
            }
        }
    }
}