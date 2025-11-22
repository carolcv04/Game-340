using UnityEngine;

public class DamageableCharacter : MonoBehaviour, IDamageable
{
    private float _currentHealth;
    public bool _targetable = true;
    
    Animator animator;
    Rigidbody2D rb;
    Collider2D physicsCollider;
    public CharacterData characterData;
    public float invincibilityTime = 0.25f;
    private bool _invincible;
    public bool invincibleEnabled = true;
    private float _invincibilityTimeElapsed = 0f;

    public float CurrentHealth
    {
        set
        {
            if (value < _currentHealth)
                animator.SetTrigger("hitTrigger");

            _currentHealth = value;

            if (_currentHealth <= 0)
            {
                animator.SetBool("isAlive", false);
                Targetable = false;
            }
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

    public bool Targetable
    {
        get { return _targetable; }
        set { _targetable = value; }
    }

    public bool Invincible
    {
        get => _invincible;
        set => _invincible = value;
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        // if (_invincible)
        // {
        //     Debug.Log("Hit blocked - invincible!");
        //     return;
        // }

        Debug.Log($"OnHit called! Damage: {damage}, Knockback: {knockback}");
        CurrentHealth -= damage;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        // Start invincibility timer
        // if (invincibleEnabled)
        // {
        //     _invincible = true;
        //     _invincibilityTimeElapsed = 0f; // ✅ Reset timer
        // }
    }

    public void OnHit(float damage)
    {
        // if (_invincible)
        // {
        //     Debug.Log("Hit blocked - invincible!");
        //     return;
        // }
        
        CurrentHealth -= damage;
        
        // // Start invincibility timer
        // if (invincibleEnabled)
        // {
        //     _invincible = true;
        //     _invincibilityTimeElapsed = 0f; // ✅ Reset timer
        // }
    }

    // void FixedUpdate()
    // {
    //     if (_invincible)
    //     {
    //         _invincibilityTimeElapsed += Time.fixedDeltaTime;
    //         if (_invincibilityTimeElapsed >= invincibilityTime)
    //         {
    //             _invincible = false;
    //             _invincibilityTimeElapsed = 0f; // ✅ Reset for next time
    //         }
    //     }
    // }
}