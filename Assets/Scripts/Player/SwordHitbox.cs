using System;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public Vector3 faceRight = new Vector3(0.8f, 0f, 0f);
    public Vector3 faceLeft = new Vector3(-0.8f, 0f, 0f);
    public float swordDamage = 1f;
    public float knockbackForce = 50f;
    public Collider2D swordCollider;
    void Start()
    {
        if (swordCollider == null) Debug.LogWarning("SwordHitbox script is missing swordCollider");
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only process enemies
        if (!other.CompareTag("Enemy"))
        {
            Debug.Log("Not an enemy.");
            return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();
    
        if (damageable != null && damageable.Targetable)
        {
            Vector2 parentPosition = transform.parent.position;
            Vector2 direction = ((Vector2)other.transform.position - parentPosition).normalized;
            Vector2 knockback = direction * knockbackForce;
            Debug.Log($"Force: {knockback}");
            damageable.OnHit(swordDamage, knockback);
        }
    }
    
    void IsFacingRight(bool isRight)
    {
        if (isRight)
            gameObject.transform.localPosition = faceRight;
        else
            gameObject.transform.localPosition = faceLeft;
    }
}