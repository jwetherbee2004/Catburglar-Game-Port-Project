using UnityEngine;

public class Gate : MonoBehaviour, IInteractable
{
    public string InteractionText => "Enter";
    [SerializeField] Gate connectedGate;
    public GameObject Object => gameObject;

    public void Interact(PlayerInteraction player)
    {
        player.gameObject.transform.position = connectedGate.gameObject.transform.position;
    }
}
