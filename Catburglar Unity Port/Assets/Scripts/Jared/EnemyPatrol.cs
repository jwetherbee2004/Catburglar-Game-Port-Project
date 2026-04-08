using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Enemy Settings")]
    public Transform enemy;
    public Transform bubbleSprite;
    public Sprite[] bubbleSprites;
    public float speed;
    
    [Header("Detection Settings")]
    public Transform player;
    public float raycastDistance = 10f;
    public LayerMask lightLayer;
    public LayerMask playerLayer;
    public float questionMarkDuration = 1.5f;
    
    private Vector3 initScale;
    private Vector3 bubbleInitLocalPos;
    private bool movingLeft;
    private bool isWaiting;
    private bool isSpotting;
    private Animator animator;
    private int currentDirection;
    private SpriteRenderer bubbleSpriteRenderer;

    private void Awake()
    {
        initScale = enemy.localScale;
        animator = enemy.GetComponent<Animator>();
        if (bubbleSprite != null)
        {
            bubbleInitLocalPos = bubbleSprite.localPosition;
            bubbleSpriteRenderer = bubbleSprite.GetComponent<SpriteRenderer>();
        }
        
        if (bubbleSpriteRenderer != null)
            bubbleSpriteRenderer.enabled = false;
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        if (!isSpotting && !isWaiting && DetectPlayer())
        {
            StartCoroutine(SpotPlayer());
            return;
        }

        if (isWaiting || isSpotting)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        animator.SetBool("isWalking", true);

        if (movingLeft)
        {
            if (enemy.position.x >= pointA.position.x)
                MoveInDirection(-1);
            else
                DirectionChange();
        }
        else
        {
            if (enemy.position.x <= pointB.position.x)
                MoveInDirection(1);
            else
                DirectionChange();
        }
    }

    private bool DetectPlayer()
    {
        if (player == null)
            return false;

        Vector3 directionToPlayer = (player.position - enemy.position).normalized;
        float distanceToPlayer = Vector3.Distance(enemy.position, player.position);

        if (distanceToPlayer > raycastDistance)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(enemy.position, directionToPlayer, distanceToPlayer, playerLayer);
        if (!hit.collider)
            return false;

        RaycastHit2D lightHit = Physics2D.Raycast(enemy.position, directionToPlayer, distanceToPlayer, lightLayer);
        
        return lightHit.collider != null;
    }

    private IEnumerator SpotPlayer()
    {
        isSpotting = true;
        animator.SetTrigger("Still");
    
        yield return new WaitForSeconds(0.2f);
        
        ShowBubble(0);

        yield return new WaitForSeconds(questionMarkDuration);
        
        if (DetectPlayer())
        {
            ShowBubble(1);
            LockPlayerMovement();
            yield return new WaitForSeconds(2f);
        }
        
        HideBubble();
        isSpotting = false;
    }

    private void ShowBubble(int spriteIndex)
    {
        if (bubbleSpriteRenderer != null && bubbleSprites.Length > spriteIndex)
        {
            bubbleSpriteRenderer.sprite = bubbleSprites[spriteIndex];
            bubbleSpriteRenderer.enabled = true;
        }
    }

    private void HideBubble()
    {
        if (bubbleSpriteRenderer != null)
            bubbleSpriteRenderer.enabled = false;
    }

    private void LockPlayerMovement()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private void DirectionChange()
    {
        StartCoroutine(WaitForPauseAnimation());
    }

    private IEnumerator WaitForPauseAnimation()
    {
        isWaiting = true;
        animator.SetTrigger("Pause");
        
        yield return null;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        
        movingLeft = !movingLeft;
        animator.SetBool("isWalking", true);
        isWaiting = false;
    }

    private void MoveInDirection(int _direction)
    {
        currentDirection = _direction;
        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * _direction, initScale.y, initScale.z);
        
        if (bubbleSprite != null)
        {
            bubbleSprite.localPosition = new Vector3(Mathf.Abs(bubbleInitLocalPos.x) * _direction, bubbleInitLocalPos.y, bubbleInitLocalPos.z);
            bubbleSprite.localScale = new Vector3(Mathf.Abs(bubbleSprite.localScale.x) * _direction, bubbleSprite.localScale.y, bubbleSprite.localScale.z);
        }
        
        enemy.position = new Vector3(enemy.position.x + Time.deltaTime * _direction * speed, enemy.position.y, enemy.position.z);
    }
}