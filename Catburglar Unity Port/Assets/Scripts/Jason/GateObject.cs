using System.Collections;
using UnityEngine;

public class Gate : MonoBehaviour, IInteractable
{
    public string InteractionText => "Enter";
    [SerializeField] Gate connectedGate;
    [SerializeField] GameObject border;
    public GameObject Object => gameObject;
    public Animator animator;
    private GameObject playerObject;
    private bool borderActive = false;
    [SerializeField] private float fadeDuration = 1f;
    private Coroutine activeFade;
    
    private bool playerInDoor = false;
    private bool isTransitioning = false;
    private Collider2D doorTrigger;

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip doorOpen;
    [SerializeField] private AudioClip elevatorOpen;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Ensure there's a trigger collider for door detection
        doorTrigger = GetComponent<Collider2D>();
        if (doorTrigger != null)
        {
            doorTrigger.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInDoor = true;
            playerObject = collision.gameObject;
            
            // Notify camera to pan towards connected gate if it's far enough away
            if (connectedGate != null && playerObject != null)
            {
                float distanceToConnectedGate = Vector3.Distance(playerObject.transform.position, connectedGate.transform.position);
                
                CameraController camera = FindObjectOfType<CameraController>();
                if (camera != null)
                {
                    camera.PanTowardGate(connectedGate.transform.position, distanceToConnectedGate);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInDoor = false;
            
            // Notify camera to return to normal
            CameraController camera = FindObjectOfType<CameraController>();
            if (camera != null)
            {
                camera.ReturnToCenterCamera();
            }
        }
    }

    public void Interact(PlayerInteraction player)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        playerObject = player.gameObject;
        animator.SetTrigger("Open");
        playerObject.GetComponent<PlayerMovement>().LockMovement();

        // Center the player at the gate entrance
        Vector3 startPos = playerObject.transform.position;
        startPos.Set(transform.position.x-0.5f, startPos.y, startPos.z);
        playerObject.transform.position = startPos;
        playerObject.GetComponent<PlayerMovement>().GetAnimator().SetTrigger("EnterGate");

        // Start fade-out concurrent with the EnterGate animation
        if (activeFade != null) StopCoroutine(activeFade);
        activeFade = StartCoroutine(FadeOut());
    }

    public void TeleportPlayer()
    {
        // Stop any running fade before starting the fade-in
        if (activeFade != null) StopCoroutine(activeFade);

        Vector3 endPos = connectedGate.gameObject.transform.position;
        endPos.Set(endPos.x - 0.5f, endPos.y, endPos.z);
        playerObject.transform.position = endPos;
        playerObject.GetComponent<PlayerMovement>().GetAnimator().SetTrigger("Teleported");
        connectedGate.animator.SetTrigger("OpenNoTP");

        activeFade = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        SpriteRenderer sr = playerObject.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = c;
            yield return null;
        }
        c.a = 0f;
        sr.color = c;
        activeFade = null;
    }

    private IEnumerator FadeIn()
    {
        SpriteRenderer sr = playerObject.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = c;
            yield return null;
        }
        c.a = 1f;
        sr.color = c;
        activeFade = null;
        isTransitioning = false;
        playerObject.GetComponent<PlayerMovement>().UnlockMovement();
    }

    public void ShowHideBorder()
    {
        connectedGate.ShowHideBorderNoCall();
    }
    public void ShowHideBorderNoCall()
    {
        borderActive = !borderActive;
        border.SetActive(borderActive);
    }

    private void PlayOpen()
    {
        // check the tag if it's an elevator or regular door to play the correct sound
        if (audioSource != null)
        {
            if (gameObject.CompareTag("Elevator") && elevatorOpen != null)
            {
                audioSource.PlayOneShot(elevatorOpen);
            }
            else if (doorOpen != null)
            {
                audioSource.PlayOneShot(doorOpen);
            }
        }
    }
}
