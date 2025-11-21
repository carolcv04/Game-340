using System;
using System.Collections;
using Unity.VisualScripting;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    public EnemyData enemyData;
    private float enemyDamage;
    public float knockbackForce = 100f;
    void Start()
    {
        enemyDamage = enemyData.damage;
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        IDamageable damageable = collider.GetComponent<IDamageable>();
    
        if (damageable != null && damageable.Targetable)
        {
            Debug.Log("damageable");
            Vector2 direction = ((Vector2) collider.transform.position - (Vector2) transform.position).normalized;
            Vector2 knockback = direction * knockbackForce;
            Debug.Log($"Force: {knockback}");
            damageable.OnHit(enemyDamage, knockback);
        }
        else
        {
            Debug.Log("no damageable");
        }
    }
    
    public void OnDeath()
    {
        // if (enemyData.itemDropped != null)
        // {
        //     DropItem();
        // }
        
        // Destroy enemy
        if (IsServer)
        {
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
            Destroy(gameObject);
        }
    }

    // private void DropItem()
    // {
    //     if (!IsServer) return; // Only server spawns items
    //     
    //     // Get item prefab from database
    //     if (!ItemDatabase.itemDatabaseInstance.TryGetItem(enemyData.itemDropped.itemID, out ItemPreset preset))
    //     {
    //         Debug.LogError($"Failed to find item {enemyData.itemDropped.itemID} in database!");
    //         return;
    //     }
    //     
    //     // Find the world item prefab (you'll need to store this somewhere)
    //     Item itemPrefab = enemyData.modelPrefab.prefab;
    //     if (itemPrefab == null)
    //     {
    //         Debug.LogError("World item prefab not found!");
    //         return;
    //     }
    //     
    //     // Spawn at enemy position with slight offset
    //     Vector2 dropPosition = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
    //     GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
    //     
    //     // Set up the item
    //     Item worldItem = droppedItem.GetComponent<Item>();
    //     if (worldItem != null)
    //     {
    //         worldItem.Initialize(preset, 1);
    //     }
    //     
    //     // Spawn on network
    //     NetworkObject networkObject = droppedItem.GetComponent<NetworkObject>();
    //     if (networkObject != null)
    //     {
    //         networkObject.Spawn();
    //     }
    // }

    private void Update()
    {
        transform.Translate(Vector3.left * enemyData.speed * Time.deltaTime);
    }
}