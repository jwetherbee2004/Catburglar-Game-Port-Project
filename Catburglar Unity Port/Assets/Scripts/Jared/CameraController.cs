using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity = Vector3.zero;
    private float smoothTime = 0.3f;

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
