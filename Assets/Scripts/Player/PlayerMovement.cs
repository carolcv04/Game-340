using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public PlayerSpawnPoint spawnPoint;
    private Animator animator;
    SpriteRenderer spriteRenderer;
    public bool canMove = true;
    private bool playingFootsteps = false;
    public float footstepSpeed = 0.5f;
    private float lastMoveX = 1; // 1 = right, -1 = left
    public GameObject swordHitbox;
    Collider2D swordCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; //prevents the character from sliding down
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        swordCollider = swordHitbox.GetComponent<Collider2D>();
        SetSpawnPoint(spawnPoint);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        
        if (PauseMenu.IsGamePaused())
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            StopFootsteps();
        }
        rb.linearVelocity = moveInput * moveSpeed;
        animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0);

        if (rb.linearVelocity.magnitude > 0 && !playingFootsteps)
        {
            StartFootsteps();
        }
        else if(rb.linearVelocity.magnitude == 0)
        {
            StopFootsteps();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        
        // //TODO ADD THIS LOGIC TO FLIP SPRITES! REMOVE TREE
        // gameObject.BroadcastMessage("IsFacingRight", true);
        // gameObject.BroadcastMessage("IsFacingRight", false);

    }
    public void OnFire(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            Debug.Log("OnFire triggered!");

            // Flip sprite based on the last facing direction
            // if (lastMoveX < 0)
            //     spriteRenderer.flipX = false;
            // else
            //     spriteRenderer.flipX = true;
            
            animator.SetTrigger("isAttacking");
            PlayAttackAnimationServerRpc();
            
            // spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    [ServerRpc]
    private void PlayAttackAnimationServerRpc()
    {
        // Tell all OTHER clients (not the one who called it)
        PlayAttackAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        if (IsOwner) return; // Skip owner - they already played it locally
    
        animator.SetTrigger("isAttacking");
    }
    void StartFootsteps()
    {
        playingFootsteps = true;
        InvokeRepeating(nameof(PlayFootstep), 0f, footstepSpeed);
    }

    void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootstep));
    }

    void PlayFootstep()
    {
        SoundEffectManager.Play("Footstep", true);
    }

    void SetSpawnPoint(PlayerSpawnPoint spawnPoint)
    {
        transform.position = spawnPoint.position;
    }
}
