using System;
using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PlayerStats : NetworkBehaviour, IDamageable
{
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    public PlayerData playerData;

    [SerializeField] private float _currentHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxTotalHealth;

    public bool isDead;
    public bool inCombat;
    private Coroutine regenerateCoroutine;
    public bool _targetable = true;
    public event Action<PlayerStats> OnPlayerDied;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D physicsCollider;

    // --- IDamageable Properties ---
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
                _currentHealth = 0;
                OnPlayerDied?.Invoke(this); // fire event
                
                // if (IsServer && TurnManager.Instance != null)
                // {
                //     TurnManager.Instance.EndTurn();
                // }
            }
        }
        get { return _currentHealth; }
    }

    public float Health { get; set; }
    public bool Targetable
    {
        get
        {
            return _targetable;
        }
        set
        {
            _targetable = value;
            rb.simulated = false;
            physicsCollider.enabled = value;
        } }

    public bool Invincible { get; set; }

    // -------------------------------
    public float MaxHealth => maxHealth;
    public float MaxTotalHealth => maxTotalHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        physicsCollider = GetComponent<Collider2D>();
        animator.SetBool("isAlive", true);
        CurrentHealth = playerData.maxHealth;
    }

    // private void OnCombatStateChanged(bool combat)
    // {
    //     inCombat = combat;
    //
    //     if (!inCombat && regenerateCoroutine == null)
    //     {
    //         regenerateCoroutine = StartCoroutine(RegenerateHealth());
    //     }
    //     else if (regenerateCoroutine != null)
    //     {
    //         StopCoroutine(regenerateCoroutine);
    //         regenerateCoroutine = null;
    //     }
    // }

    private IEnumerator RegenerateHealth()
    {
        while (!inCombat && CurrentHealth < maxHealth)
        {
            Heal(1f);
            yield return new WaitForSeconds(1f);
        }
    }

    public void Initialize(PlayerData data)
    {
        playerData = data;
        _currentHealth = playerData.maxHealth;
        maxHealth = playerData.maxHealth;
        maxTotalHealth = playerData.maxTotalHealth;
    }
    public bool IsDead() => _currentHealth <= 0;
    public void Heal(float amount)
    {
        CurrentHealth += amount;
        ClampHealth();
    }

    public void Revive()
    {
        CurrentHealth = maxHealth;
    }

    private void ClampHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
        onHealthChangedCallback?.Invoke();
    }

    // ------------------- IDamageable Methods -------------------
    public void OnHit(float damage, Vector2 knockback)
    {
        Debug.Log($"Player OnHit called! Damage: {damage}, Knockback: {knockback}, RB Type: {rb.bodyType}");
        CurrentHealth -= damage;
        rb.AddForce(knockback, ForceMode2D.Impulse);
        ClampHealth();
    }

    public void OnHit(float damage)
    {
        Debug.Log($"Player OnHit called! Damage: {damage}");
        CurrentHealth -= damage;
        ClampHealth();
    }
    public void OnObjectDestroyed()
    {
        gameObject.SetActive(false);
    }
}
