using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecurityCamera : MonoBehaviour
{
    public Animator animator;
    public Collider2D cameraColliderLeft;
    public Collider2D cameraColliderRight;
    public bool startFacingLeft = true;
    public float switchInterval = 4f;
    public Transform player;

    private bool playerInCamera;
    private bool isSwitching;
    private bool facingLeft;
    private float blinkingDuration = 0f;
    private bool isCatching = false;

    void Awake()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        if (cameraColliderLeft != null)
            cameraColliderLeft.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        if (cameraColliderRight != null)
            cameraColliderRight.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // if (cameraCollider == null)
        //     cameraCollider = GetComponent<Collider2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        facingLeft = startFacingLeft;

        animator.SetBool("FacingLeft", facingLeft);
        animator.SetBool("Blinking", false);

        StartCoroutine(CameraSwitchCycle());
    }

    void Update()
    {
        if (isSwitching || isCatching)
        {
            animator.SetBool("Blinking", false);
            cameraColliderLeft.enabled = false;
            cameraColliderRight.enabled = false;
            return;
        }

        Collider2D activeCollider;
        Collider2D inactiveCollider;

        if (facingLeft)
        {
            activeCollider = cameraColliderLeft;
            inactiveCollider = cameraColliderRight;
        }
        else
        {
            activeCollider = cameraColliderRight;
            inactiveCollider = cameraColliderLeft;
        }

        if (!activeCollider.enabled)
            activeCollider.enabled = true;

        if (inactiveCollider.enabled)
            inactiveCollider.enabled = false;

        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapCollider(activeCollider, new ContactFilter2D(), results);

        bool playerDetected = false;
        foreach (Collider2D col in results)
        {
            if (col.CompareTag("Player"))
            {
                playerDetected = true;
                break;
            }
        }

        playerInCamera = playerDetected;
        bool shouldBlink = playerInCamera && GameManager.Instance.inLight && !GameManager.Instance.isHidden;
        animator.SetBool("Blinking", shouldBlink);

        if (shouldBlink)
        {
            blinkingDuration += Time.deltaTime;
            if (blinkingDuration >= 1f && !isCatching) // Change the number to adjust how long the player needs to be in the camera before being caught
            {
                StartCoroutine(CatchPlayer());
            }
        }
        else
        {
            blinkingDuration = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInCamera = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInCamera = false;
    }

    private IEnumerator CatchPlayer()
    {
        if(player.GetComponent<PlayerMovement>().playerIsCaught)
            yield break;
        
        isCatching = true;
        blinkingDuration = 0f;
        
        LockPlayerMovement();
        player.GetComponent<PlayerMovement>().GetAnimator().SetTrigger("Caught");
        GameManager.Instance.ResetLevel();
        
        yield return new WaitForSeconds(2f);
        
        isCatching = false;
    }

    private IEnumerator CameraSwitchCycle()
    {
        while (true)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < switchInterval)
            {
                bool shouldBlink = playerInCamera && GameManager.Instance.inLight && !GameManager.Instance.isHidden;
                
                if (shouldBlink)
                {
                    yield return null;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            
            yield return StartCoroutine(SwitchCamera());
        }
    }

    private IEnumerator SwitchCamera()
    {
        isSwitching = true;
        playerInCamera = false;

        // if (cameraCollider != null)
        //     cameraCollider.enabled = false;

        string triggerName = facingLeft ? "SwitchToRight" : "SwitchToLeft";
        animator.SetTrigger(triggerName);

        yield return null;
        yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f
            || animator.IsInTransition(0))
        {
            yield return null;
        }

        facingLeft = !facingLeft;
        animator.SetBool("FacingLeft", facingLeft);

        // if (cameraCollider != null)
        //     cameraCollider.enabled = true;

        isSwitching = false;
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
}
