using UnityEngine;

public class HackObject : MonoBehaviour, IInteractable
{
    public string InteractionText => "Hack";
    public bool doneInteracting = false;
    public GameObject Object => gameObject;

    public void Interact(PlayerInteraction player)
    {
        HackingManager.instance.StartHacking();
    }
}
