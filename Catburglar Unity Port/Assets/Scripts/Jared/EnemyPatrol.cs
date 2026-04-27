using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

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

    [Header("Sleeper Settings")]
    public bool isSleeper;
    public int positionsUntilSleep = 5;
    private int positionAmt;
    public float sleepTime;
    
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
    private bool isSleeping;
    private Animator animator;
    private int currentDirection;
    private SpriteRenderer bubbleSpriteRenderer;
    
    // Gizmo debug info
    private Vector3 lastRaycastOrigin;
    private Vector3 lastRaycastDirection;
    private float lastRaycastDistance;
    private bool lastDetectionResult;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private AudioClip spotted;
    [SerializeField] private AudioClip caught;
    [SerializeField] private AudioClip nevermind;
    [SerializeField] private AudioClip sleep;

    private AudioSource audioSource;

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
        audioSource = GetComponentInChildren<AudioSource>();

        if (isSleeper && !isSleeping)
        {
            if (positionAmt >= positionsUntilSleep)
            {
                StartCoroutine(Sleep());
                positionAmt = 0;
            }
        }

        if (!isSpotting && !isWaiting && DetectPlayer())
        {
            StartCoroutine(SpotPlayer());
            return;
        }

        if (isWaiting || isSpotting || isSleeping)
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
        if (player == null || isSleeping)
            return false;

        Vector3 directionToPlayer = (player.position - enemy.position).normalized;
        float distanceToPlayer = Vector3.Distance(enemy.position, player.position);

        if (distanceToPlayer > raycastDistance)
        {
            lastDetectionResult = false;
            return false;
        }

        bool playerInFront = (currentDirection > 0 && player.position.x > enemy.position.x) ||
                            (currentDirection < 0 && player.position.x < enemy.position.x);
        
        if (!playerInFront)
        {
            lastDetectionResult = false;
            return false;
        }

        lastRaycastOrigin = enemy.position;
        lastRaycastDirection = directionToPlayer;
        lastRaycastDistance = distanceToPlayer;

        RaycastHit2D hit = Physics2D.Raycast(enemy.position, directionToPlayer, distanceToPlayer, playerLayer);
        if (!hit.collider)
        {
            lastDetectionResult = false;
            //Debug.Log(hit.collider);
            return false;
        }

        if (GameManager.Instance.isHidden || hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Ground"))
        {
            lastDetectionResult = false;
            //Debug.Log(hit.collider.IsTouchingLayers(LayerMask.NameToLayer("Ground")));
            return false;
        }

        Collider2D playerInLight = Physics2D.OverlapPoint(player.position, lightLayer);
        
        lastDetectionResult = playerInLight != null;
        return lastDetectionResult;
    }

    private IEnumerator SpotPlayer()
    {
        isSpotting = true;
        animator.SetTrigger("Still");
        PlaySpotted();
    
        yield return new WaitForSeconds(0.2f);
        
        ShowBubble(0);

        yield return new WaitForSeconds(questionMarkDuration);
        
        if (DetectPlayer())
        {
            if(!player.GetComponent<PlayerMovement>().playerIsCaught)
            {
                player.GetComponent<PlayerMovement>().playerIsCaught = true;
                ShowBubble(1);
                LockPlayerMovement();
                player.GetComponent<PlayerMovement>().GetAnimator().SetTrigger("Caught");
                PlayCaught();
                GameManager.Instance.ResetLevel();
                yield return new WaitForSeconds(2f);
            }
        }
        
        HideBubble();
        isSpotting = false;
        PlayNevermind();
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

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;
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
        positionAmt++;
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

    private IEnumerator Sleep()
    {
        isSleeping = true;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("sleep_start");
        
        yield return null;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        
        animator.SetBool("isSleeping", true);
        
        yield return new WaitForSeconds(sleepTime);
        
        animator.SetBool("isSleeping", false);
        animator.SetTrigger("wake_up");
        
        yield return null;
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        
        isSleeping = false;
        animator.SetBool("isWalking", true);
    }

    private void OnDrawGizmos()
    {
        if (lastRaycastDistance > 0 && enemy != null)
        {
            Vector3 raycastEnd = lastRaycastOrigin + lastRaycastDirection * lastRaycastDistance;
            Gizmos.color = lastDetectionResult ? Color.red : Color.green;
            Gizmos.DrawLine(lastRaycastOrigin, raycastEnd);
            Gizmos.DrawWireSphere(raycastEnd, 0.1f);
        }
    }

    // Audio methods
    public void PlayFootstep()
    {
        if (footsteps.Length > 0 && audioSource != null)
        {
            AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlaySpotted()
    {
        if (spotted != null && audioSource != null)
        {
            audioSource.PlayOneShot(spotted);
        }
    }

    public void PlayCaught()
    {
        if (caught != null && audioSource != null)
        {
            audioSource.PlayOneShot(caught);
        }
    }

    public void PlayNevermind()
    {
        if (nevermind != null && audioSource != null)
        {
            audioSource.PlayOneShot(nevermind);
        }
    }

    public void PlaySleep()
    {
        if (sleep != null && audioSource != null)
        {
            audioSource.PlayOneShot(sleep);
        }
    }
}