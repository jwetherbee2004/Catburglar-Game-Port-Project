using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 5f;

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

    void HandleMovement()
    {
        bool isCrouching = Input.GetKey(KeyCode.DownArrow);
        float moveInput = 0f;

        if (!isCrouching)
        {
            if (Input.GetKey(KeyCode.RightArrow))
                moveInput = 1f;
            if (Input.GetKey(KeyCode.LeftArrow))
                moveInput = -1f;
        }

        if (spriteRenderer != null)
        {
            if (moveInput > 0f)
                spriteRenderer.flipX = false;
            else if (moveInput < 0f)
                spriteRenderer.flipX = true;
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput != 0f);
            animator.SetBool("isCrouching", isCrouching);
        }
    }
}
