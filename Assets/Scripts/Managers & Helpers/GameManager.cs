using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;
    private List<PlayerController> players = new List<PlayerController>();
    public int numberOfPlayers = 2;
    public HealthBarController healthBarController; // assign in inspector if needed

    public List<PlayerController> GetPlayers()
    {
        return players;
    }
    
    public void SpawnPlayers()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            // Ensure we have spawn points
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned!");
                return;
            }

            Transform spawnPoint = spawnPoints[players.Count % spawnPoints.Length];

            // Instantiate player prefab
            GameObject playerObj = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            PlayerStats stats = playerObj.GetComponent<PlayerStats>();

            if (playerController != null && stats != null)
            {
                // Create unique PlayerData for this player
                PlayerData data = ScriptableObject.CreateInstance<PlayerData>();
                data.characterName = "Player " + (i + 1);
                data.maxHealth = 100;
                data.speed = 5f;

                // Initialize PlayerStats
                stats.Initialize(data);

                // Assign to PlayerController
                playerController.playerStats = stats;

                // Assign health bar if you have one
                // if (healthBarController != null)
                //     playerController.healthBarController = healthBarController;

                players.Add(playerController);
            }
            else
            {
                Debug.LogError("Spawned player is missing PlayerController or PlayerStats!");
            }
        }
    }

}