using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightToggle : MonoBehaviour, IInteractable
{
    public string InteractionText => "Toggle Light";
    public bool doneInteracting = false;
    public GameObject Object => gameObject;
    private bool isOn = true;

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip lightSwitch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact(PlayerInteraction player)
    {
        isOn = !isOn;
        GetComponentInChildren<Light2D>().enabled = isOn;
        GetComponentInChildren<PolygonCollider2D>().enabled = isOn;
        audioSource.PlayOneShot(lightSwitch);
    }
}
