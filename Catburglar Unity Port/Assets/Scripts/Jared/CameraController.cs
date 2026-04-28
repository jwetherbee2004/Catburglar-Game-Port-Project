using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 8f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Header("Camera Zoom")]
    public float orthographicSize = 5f;

    [Header("Camera Boundaries")]
    public bool useBoundaries = true;
    public float minX = -10f;
    public float maxX = 35f;
    public float minY = -5f;
    public float maxY = 22f;
    
    [Header("Door Pan")]
    [SerializeField] private float doorPanSpeed = 1.5f;
    [SerializeField] private float doorPanMinDistance = 12f;

    private Camera mainCamera;
    private Vector3 normalOffset;
    private Vector3 targetOffset;
    private bool isPanning = false;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = orthographicSize;
        }
        normalOffset = offset;
        targetOffset = offset;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        // Smoothly interpolate offset for door panning
        offset = Vector3.Lerp(offset, targetOffset, doorPanSpeed * Time.deltaTime);

        Vector3 targetPosition = player.position + offset;

        if (useBoundaries && mainCamera != null)
        {
            float orthoSize = mainCamera.orthographicSize;
            float aspectRatio = mainCamera.aspect;

            float viewportWidth = orthoSize * aspectRatio;
            float viewportHeight = orthoSize;

            float clampMinX = minX + viewportWidth;
            float clampMaxX = maxX - viewportWidth;
            float clampMinY = minY + viewportHeight;
            float clampMaxY = maxY - viewportHeight;

            targetPosition.x = Mathf.Clamp(targetPosition.x, clampMinX, clampMaxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, clampMinY, clampMaxY);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public void PanTowardGate(Vector3 connectedGatePosition, float distanceToGate)
    {
        if (player == null)
            return;

        // Only pan if the door is far enough away
        if (distanceToGate < doorPanMinDistance)
            return;

        Vector3 directionToGate = (connectedGatePosition - player.position).normalized;
        targetOffset = normalOffset + directionToGate * 5f;
        isPanning = true;
    }

    public void ReturnToCenterCamera()
    {
        targetOffset = normalOffset;
        isPanning = true;
    }

    private void OnDrawGizmos()
    {
        if (!useBoundaries)
            return;

        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        if (mainCamera != null)
        {
            float orthoSize = mainCamera.orthographicSize;
            float aspectRatio = mainCamera.aspect;
            float viewportWidth = orthoSize * aspectRatio;
            float viewportHeight = orthoSize;

            Vector3 clampTopLeft = new Vector3(minX + viewportWidth, maxY - viewportHeight, 0);
            Vector3 clampTopRight = new Vector3(maxX - viewportWidth, maxY - viewportHeight, 0);
            Vector3 clampBottomLeft = new Vector3(minX + viewportWidth, minY + viewportHeight, 0);
            Vector3 clampBottomRight = new Vector3(maxX - viewportWidth, minY + viewportHeight, 0);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(clampTopLeft, clampTopRight);
            Gizmos.DrawLine(clampTopRight, clampBottomRight);
            Gizmos.DrawLine(clampBottomRight, clampBottomLeft);
            Gizmos.DrawLine(clampBottomLeft, clampTopLeft);
        }
    }
}
