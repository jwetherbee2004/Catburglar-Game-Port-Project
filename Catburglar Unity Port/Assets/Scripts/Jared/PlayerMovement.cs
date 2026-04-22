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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleMovement();
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
        if (hit.collider != null)
            return true;
        RaycastHit2D hit2 = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, platformLayer);
        if (hit2.collider != null)
            return true;
        return false;
    }

    void HandleMovement()
    {
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

        bool isCurrentlyRolling = rollTimer > 0f;

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
        if (isCurrentlyRolling)
        {
            velocityX = rollDirection * (rollDistance / Mathf.Max(rollDuration, 0.01f));
        }
        else
        {
            velocityX = moveInput * moveSpeed;
        }

        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
        //rb.Slide(new Vector2(velocityX, rb.linearVelocity.y), Time.deltaTime, m_slideMovement);

        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput != 0f && !isCurrentlyRolling);
            animator.SetBool("isCrouching", isCrouching && !isCurrentlyRolling);
            animator.SetBool("isRolling", isCurrentlyRolling);
            animator.SetBool("isFalling", !isGrounded);
        }
    }
}
