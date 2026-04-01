using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 5f;
    public float rollDistance = 8f;
    public float rollDuration = 1.5f;
    
    private float rollTimer = 0f;
    private float rollDirection = 0f;

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

    void HandleMovement()
    {
        bool isCrouching = Input.GetKey(KeyCode.DownArrow);
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

        rb.velocity = new Vector2(velocityX, rb.velocity.y);

        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput != 0f && !isCurrentlyRolling);
            animator.SetBool("isCrouching", isCrouching && !isCurrentlyRolling);
            animator.SetBool("isRolling", isCurrentlyRolling);
        }
    }
}
