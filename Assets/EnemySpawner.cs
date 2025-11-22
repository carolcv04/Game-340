using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Scene Enemies to Spawn")]
    [SerializeField] private GameObject[] enemyPrefabs; // Assign your enemy PREFABS here
    [SerializeField] private Transform[] spawnPoints; // Create empty GameObjects as spawn points
    
    private void Start()
    {
        if (!IsServer) return;
        
        Debug.Log("[EnemySpawner] Server spawning enemies...");
        
        // Spawn enemies at designated points
        for (int i = 0; i < spawnPoints.Length && i < enemyPrefabs.Length; i++)
        {
            SpawnEnemy(enemyPrefabs[i % enemyPrefabs.Length], spawnPoints[i].position);
        }
    }
    
    private void SpawnEnemy(GameObject enemyPrefab, Vector3 position)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            Debug.Log($"[EnemySpawner] Spawned {enemy.name} at {position}");
        }
        else
        {
            Debug.LogError($"[EnemySpawner] {enemy.name} has no NetworkObject!");
        }
    }
}