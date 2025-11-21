using UnityEngine;

public class DamageableCharacter : MonoBehaviour, IDamageable
{
    private float _currentHealth;
    public bool _targetable = true;
    
    Animator animator;
    Rigidbody2D rb;
    Collider2D physicsCollider;
    public CharacterData characterData;
    public float CurrentHealth
    {
        set
        {
            if (value < _currentHealth)
                animator.SetTrigger("hitTrigger");

            _currentHealth = value;

            if (_currentHealth <= 0)
                animator.SetBool("isAlive", false);
        }
        get { return _currentHealth; }
    }
    private void Awake()
    {
        if (characterData != null)
        {
            _currentHealth = characterData.maxHealth;
        }
    }
    public void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isAlive", true);
        physicsCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }
    public void OnObjectDestroyed()
    {
        Destroy(gameObject);
    }
    
    public float Health { get; set; }
    public bool Targetable {
        get { return _targetable; }
        set
        {
            _targetable = value;
            physicsCollider.enabled = value;
        }
    }
    public void OnHit(float damage, Vector2 knockback)
    {
        Debug.Log($"OnHit called! Damage: {damage}, Knockback: {knockback}, RB Type: {rb.bodyType}");
        CurrentHealth -= damage;
        rb.AddForce(knockback, ForceMode2D.Impulse);
    }

    public void OnHit(float damage)
    {
        CurrentHealth -= damage;
    }
}
