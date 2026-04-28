using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Rigidbody2D.SlideMovement m_slideMovement = new();

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 5f;
    public float rollDistance = 8f;
    public float rollDuration = 1.5f;
    public bool playerIsCaught = false;
    public bool isCurrentlyRolling = false;
    public bool canInteract = true;
    public bool isUnderPlatform = false;
    
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public string platformText = "Platform";
    public string playerText = "Player";
    public float groundCheckDistance = 0.5f;
    
    private float rollTimer = 0f;
    private float platformTimer = 0f;
    private float rollDirection = 0f;
    private bool isGrounded = false;
    public bool isCrouching { get; private set; } = false;
    
    private bool rollCanceled = false;
    private float rollRecoveryDuration = 0.1f;
    private bool isMovementLocked = false;
    private bool wasUnderPlatform = false;
    private float platformExitDelay = 0.3f;
    private float platformExitTimer = 0f;
    
    [Header("Roll Hitbox")]
    [SerializeField] private Vector2 rolledHitboxSize = new Vector2(0.8f, 0.8f);
    private Collider2D playerCollider;
    private Vector2 originalHitboxSize;
    private Vector2 originalHitboxOffset;
    private bool isHitboxShrunk = false;

    [Header("Audio")]
    public AudioSource audioSource;
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private AudioClip roll;
    [SerializeField] private AudioClip land;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider is BoxCollider2D boxCollider)
        {
            originalHitboxSize = boxCollider.size;
            originalHitboxOffset = boxCollider.offset;
        }
        else if (playerCollider is CapsuleCollider2D capsuleCollider)
        {
            originalHitboxSize = capsuleCollider.size;
            originalHitboxOffset = capsuleCollider.offset;
        }
    }

    void Update()
    {
        HandleMovement();
        canInteract = !isCurrentlyRolling && !playerIsCaught && !isCrouching;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (rollTimer > 0f && collision.CompareTag("Wall"))
        {
            rollDirection *= -1f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (rollTimer > 0f && collision.gameObject.CompareTag("Wall"))
        {
            rollDirection *= -1f;
        }
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    public void LockMovement()
    {
        isMovementLocked = true;
        animator.SetBool("isMoving", false);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void UnlockMovement()
    {
        isMovementLocked = false;
    }

    public void PlayStealAndUnlock()
    {
        LockMovement();
        StartCoroutine(StealCoroutine());
    }

    private IEnumerator StealCoroutine()
    {
        animator.SetTrigger("Steal");

        yield return null;
        yield return null;
        while (animator.IsInTransition(0))
            yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        UnlockMovement();
    }

    bool CheckGrounded()
    {
        Vector2 colliderCenter = rb.position;
        Collider2D collider = rb.GetComponent<Collider2D>();
        float colliderWidth = collider.bounds.extents.x;
        float colliderHeight = collider.bounds.extents.y;
        
        // Raycast straight down from center
        Vector2 rayOrigin = new Vector2(colliderCenter.x, colliderCenter.y - colliderHeight);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        if (hit.collider != null)
            return true;
        RaycastHit2D hit2 = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, platformLayer);
        if (hit2.collider != null && hit2.collider.CompareTag(platformText))
            return true;
        
        // Raycasts from the sides (for slanted platforms)
        Vector2 leftOrigin = new Vector2(colliderCenter.x - colliderWidth, colliderCenter.y);
        Vector2 rightOrigin = new Vector2(colliderCenter.x + colliderWidth, colliderCenter.y);
        
        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDistance + colliderHeight, platformLayer);
        if (hitLeft.collider != null && hitLeft.collider.CompareTag(platformText))
            return true;
        
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDistance + colliderHeight, platformLayer);
        if (hitRight.collider != null && hitRight.collider.CompareTag(platformText))
            return true;
        
        return false;
    }

    void HandleMovement()
    {
        if (isMovementLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        isGrounded = CheckGrounded();
        
        isCrouching = Input.GetKey(KeyCode.DownArrow);

        if (Input.GetKeyDown(KeyCode.X) && isCrouching)
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerText), LayerMask.NameToLayer(platformText), true);
            platformTimer = 1;
        } 
        else if (platformTimer >= 0f)
        {
            platformTimer -= Time.deltaTime;
        }
        else
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerText), LayerMask.NameToLayer(platformText), false);
        }

            float moveInput = 0f;
        
        if (rollTimer > 0f)
            rollTimer -= Time.deltaTime;

        isCurrentlyRolling = rollTimer > 0f;
        
        // Shrink hitbox while rolling, restore when done
        if (isCurrentlyRolling && !isHitboxShrunk)
        {
            // Adjust offset to keep collider bottom anchored at the same ground position
            Vector2 rolledOffset = new Vector2(
                originalHitboxOffset.x,
                originalHitboxOffset.y - (originalHitboxSize.y - rolledHitboxSize.y) / 2f
            );
            if (playerCollider is BoxCollider2D boxCol)
            {
                boxCol.size = rolledHitboxSize;
                boxCol.offset = rolledOffset;
                isHitboxShrunk = true;
            }
            else if (playerCollider is CapsuleCollider2D capsuleCol)
            {
                capsuleCol.size = rolledHitboxSize;
                capsuleCol.offset = rolledOffset;
                isHitboxShrunk = true;
            }
        }
        else if (!isCurrentlyRolling && isHitboxShrunk)
        {
            if (playerCollider is BoxCollider2D boxCol2)
            {
                boxCol2.size = originalHitboxSize;
                boxCol2.offset = originalHitboxOffset;
                isHitboxShrunk = false;
            }
            else if (playerCollider is CapsuleCollider2D capsuleCol2)
            {
                capsuleCol2.size = originalHitboxSize;
                capsuleCol2.offset = originalHitboxOffset;
                isHitboxShrunk = false;
            }
        }
        
        // Check for roll cancellation (only during frames 2-5)
        if (isCurrentlyRolling && Input.GetKeyDown(KeyCode.X) && !rollCanceled && animator != null)
        {
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
            // Allow cancel only between frame 2 and frame 5 (inclusive)
            // Frame 2 = 2/13, Frame 6 = 6/13 (can cancel until frame 6 starts)
            if (normalizedTime >= 2f / 13f && normalizedTime < 6f / 13f)
            {
                rollCanceled = true;
                rollTimer = rollRecoveryDuration;
            }
        }
        
        // Reset rollCanceled when roll ends
        if (!isCurrentlyRolling && rollCanceled)
        {
            rollCanceled = false;
        }

        if (!isCurrentlyRolling)
        {
            if (!isCrouching)
            {
                if (Input.GetKey(KeyCode.RightArrow))
                    moveInput = 1f;
                if (Input.GetKey(KeyCode.LeftArrow))
                    moveInput = -1f;
            }
            else
            {
                if (Input.GetKey(KeyCode.RightArrow))
                    moveInput = 1f;
                if (Input.GetKey(KeyCode.LeftArrow))
                    moveInput = -1f;
            }

            if (isCrouching && moveInput != 0f)
            {
                rollTimer = Mathf.Max(rollDuration, 0.01f);
                rollDirection = moveInput;
                isCurrentlyRolling = true;
            }
        }

        if (spriteRenderer != null)
        {
            if (isCurrentlyRolling)
            {
                if (rollDirection > 0f)
                    spriteRenderer.flipX = false;
                else if (rollDirection < 0f)
                    spriteRenderer.flipX = true;
            }
            else
            {
                if (moveInput > 0f)
                    spriteRenderer.flipX = false;
                else if (moveInput < 0f)
                    spriteRenderer.flipX = true;
            }
        }

        float velocityX = 0f;
        if (isCurrentlyRolling && !rollCanceled)
        {
            velocityX = rollDirection * (rollDistance / Mathf.Max(rollDuration, 0.01f));
        }
        else
        {
            velocityX = moveInput * moveSpeed;
        }

        // Default to Slide for slope climbing
        if (isUnderPlatform)
        {
            // Use linearVelocity when under platform to avoid collisions
            rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
            wasUnderPlatform = true;
            platformExitTimer = 0f;
            Debug.Log("Using normal velocity under platform");
        }
        else
        {
            // Use Slide by default, but give a grace period after exiting platform
            if (wasUnderPlatform && platformExitTimer < platformExitDelay)
            {
                platformExitTimer += Time.deltaTime;
                rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
                Debug.Log("Platform exit grace period: " + platformExitTimer);
            }
            else
            {
                // Reset horizontal velocity before switching to Slide to prevent launch
                if (wasUnderPlatform && platformExitTimer >= platformExitDelay)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    wasUnderPlatform = false;
                    Debug.Log("Switched to SlideMovement velocity");
                }
                rb.Slide(new Vector2(velocityX, rb.linearVelocity.y), Time.deltaTime, m_slideMovement);
            }
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput != 0f && !isCurrentlyRolling);
            animator.SetBool("isCrouching", isCrouching && !isCurrentlyRolling);
            animator.SetBool("isRolling", isCurrentlyRolling);
            animator.SetBool("isFalling", !isGrounded && !isCurrentlyRolling);
            
            // Seek animation to frame 6 recovery frames when roll is canceled
            if (rollCanceled)
            {
                // Player_Roll has 13 frames, frame 6 = 6/13 ≈ 0.4615 normalized time
                animator.Play("Player_Roll", 0, 6f / 13f);
            }
        }
    }

    // Audio methods called from animation events
    public void PlayFootstep()
    {
        if (footsteps.Length > 0 && audioSource != null)
        {
            AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayRoll()
    {
        if (roll != null && audioSource != null)
        {
            audioSource.PlayOneShot(roll);
        }
    }

    public void PlayLand()
    {
        if (land != null && audioSource != null)
        {
            audioSource.PlayOneShot(land);
        }
    }
}
