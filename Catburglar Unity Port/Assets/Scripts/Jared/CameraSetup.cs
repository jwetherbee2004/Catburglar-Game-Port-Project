using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraSetup : MonoBehaviour
{
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = false;
            cameraData.antialiasing = AntialiasingMode.None;
        }
    }
}
