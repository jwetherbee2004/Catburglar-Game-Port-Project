using UnityEngine;

public interface IInteractable
{
    public string InteractionText { get; }
    void Interact(PlayerInteraction player);
}
