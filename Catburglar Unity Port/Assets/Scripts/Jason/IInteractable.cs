using UnityEngine;

public interface IInteractable
{
    public string InteractionText { get; }
    GameObject Object { get; }
    void Interact(PlayerInteraction player);
}
