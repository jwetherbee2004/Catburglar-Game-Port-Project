using System.Collections;
using UnityEngine;

public class VentHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private PlayerMovement playerMovement;
    private GameObject player;

    [Header("Camera Path")]
    [SerializeField] private Transform[] cameraWaypoints;
    [SerializeField] private float cameraPanSpeed = 2.5f;
    [SerializeField] private float waypointHoldTime = 0.1f;

    [Header("Exit")]
    [SerializeField] private Transform exitSpawnPoint;
    [SerializeField] private Vector2 launchDirection = Vector2.right;
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float launchRollDuration = 1.0f;

    [Header("Trigger")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    void Start()
    {
        if (playerMovement == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerMovement = player.GetComponent<PlayerMovement>();

            if (playerMovement == null)
                playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (cameraController == null)
        {
            if (Camera.main != null)
                cameraController = Camera.main.GetComponent<CameraController>();

            if (cameraController == null)
                cameraController = FindFirstObjectByType<CameraController>();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (!collision.CompareTag("Player") || playerMovement == null || cameraController == null)
            return;

        hasTriggered = true;
        StartCoroutine(RunVentSequence());
    }

    private IEnumerator RunVentSequence()
    {
        playerMovement.LockMovement();
        player.GetComponent<SpriteRenderer>().enabled = false;
        playerMovement.isUnderPlatform = true;

        // Let the lock settle for one frame so the player doesn't drift into the vent.
        yield return null;

        Debug.Log("post-lock movement, pre-pan");

        if (cameraWaypoints != null && cameraWaypoints.Length > 0)
            yield return StartCoroutine(cameraController.PanBetweenWaypoints(cameraWaypoints, cameraPanSpeed, waypointHoldTime));

        Debug.Log("post-pan, pre-teleport");

        if (exitSpawnPoint != null)
            playerMovement.TeleportTo(exitSpawnPoint.position);

        player.GetComponent<SpriteRenderer>().enabled = true;
        cameraController.SnapToPlayer();

        // Release the player into the next room as a rolling launch.
        Vector2 launchVelocity = launchDirection.sqrMagnitude > 0.0001f
            ? launchDirection.normalized * launchSpeed
            : Vector2.right * launchSpeed;

        playerMovement.BeginVentLaunch(launchVelocity, launchRollDuration);
        playerMovement.isUnderPlatform = false;
    }
}

