using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 5f;
    public float rollDistance = 8f;
    public float rollDuration = 1.5f;
    public bool playerIsCaught = false;
    public bool isCurrentlyRolling = false;
    public bool canInteract = true;
    
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.5f;
    
    private float rollTimer = 0f;
    private float rollDirection = 0f;
    private bool isGrounded = false;
    public bool isCrouching { get; private set; } = false;
    
    private bool rollCanceled = false;
    private float rollRecoveryDuration = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    bool CheckGrounded()
    {
        Vector2 rayOrigin = new Vector2(rb.position.x, rb.position.y - rb.GetComponent<Collider2D>().bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    void HandleMovement()
    {
        isGrounded = CheckGrounded();
        
        isCrouching = Input.GetKey(KeyCode.DownArrow);
        float moveInput = 0f;
        
        if (rollTimer > 0f)
            rollTimer -= Time.deltaTime;

        isCurrentlyRolling = rollTimer > 0f;
        
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

        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput != 0f && !isCurrentlyRolling);
            animator.SetBool("isCrouching", isCrouching && !isCurrentlyRolling);
            animator.SetBool("isRolling", isCurrentlyRolling);
            animator.SetBool("isFalling", !isGrounded);
            
            // Seek animation to frame 6 recovery frames when roll is canceled
            if (rollCanceled)
            {
                // Player_Roll has 13 frames, frame 6 = 6/13 ≈ 0.4615 normalized time
                animator.Play("Player_Roll", 0, 6f / 13f);
            }
        }
    }
}
