using System;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damage = 0.25f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
            Debug.Log("Enemy collided with player!");
        }
    }
}
