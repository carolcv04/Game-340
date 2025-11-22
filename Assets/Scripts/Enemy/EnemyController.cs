using UnityEngine;

public class EnemyController : MonoBehaviour // Changed from NetworkBehaviour
{
    [Header("Stats")]
    public EnemyData enemyData;
    private float enemyDamage;
    public float knockbackForce = 10f;
    public float moveSpeed = 2f;
    
    [Header("Detection")]
    public DetectionZone detectionZone;
    
    private Rigidbody2D rb;
    private Transform targetPlayer;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[Enemy {gameObject.name}] Awake");
    }
    
    private void Start()
    {
        if (enemyData != null)
        {
            enemyDamage = enemyData.damage;
        }
    }
    
    private void FixedUpdate()
    {
        // Just move - no IsServer check needed
        if (detectionZone != null && detectionZone.detectedObject.Count > 0)
        {
            targetPlayer = detectionZone.detectedObject[0].transform;
            
            if (targetPlayer != null)
            {
                Vector2 direction = (targetPlayer.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamageable damageable = collision.collider.GetComponent<IDamageable>();
        
        if (damageable != null && damageable.Targetable)
        {
            Vector2 direction = ((Vector2)collision.collider.transform.position - (Vector2)transform.position).normalized;
            Vector2 knockback = direction * knockbackForce;
            damageable.OnHit(enemyDamage, knockback);
        }
    }
    
    public void OnDeath()
    {
        Destroy(gameObject);
    }
}