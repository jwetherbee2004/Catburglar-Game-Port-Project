using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
[RequireComponent(typeof(PolygonCollider2D))]
public class FlickeringLight : MonoBehaviour
{
    private Light2D lightComponent;
    private PolygonCollider2D collider;
    [SerializeField] private float holdTimer = 5f;
    [SerializeField] private float flickerHoldTimer = 1.6f;
    [SerializeField] private float flickerInterval = .5f;
    [SerializeField] private float flicker = .1f;

    private bool isLightOn = true;
    private bool isFlickering = false;

    void Awake()
    {
        lightComponent = GetComponent<Light2D>();
        collider = GetComponent<PolygonCollider2D>();
    }

    void Update()
    {
        if (holdTimer > 0f)
        {
            holdTimer -= Time.deltaTime;
            return;
        }

        if (flickerHoldTimer > 0f)
        {
            flickerHoldTimer -= Time.deltaTime;

            if (flicker > 0f && isFlickering)
            {
                flicker -= Time.deltaTime;
                return;
            }
            else if (flicker <= 0f && isFlickering)
            {
                lightComponent.enabled = !lightComponent.enabled;
                isFlickering = false;
            }

            if (flickerInterval > 0f)
            {
                flickerInterval -= Time.deltaTime;
                return;
            }

            lightComponent.enabled = !lightComponent.enabled;
            flicker = .1f;
            flickerInterval = .5f;
            isFlickering = true;
            return;
        }

        lightComponent.enabled = !isLightOn;
        collider.enabled = !isLightOn;
        isLightOn = !isLightOn;
        holdTimer = 5f;
        flickerHoldTimer = 1.6f;
        flickerInterval = .5f;
        flicker = .1f;
        isFlickering = true;
    }
}
