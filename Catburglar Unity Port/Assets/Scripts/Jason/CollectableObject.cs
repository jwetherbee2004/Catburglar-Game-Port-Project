using UnityEngine;

public class CollectableObject : MonoBehaviour, IInteractable
{
    public string InteractionText => "Steal";
    [SerializeField] int value = 0;
    public GameObject Object => gameObject;

    public void Interact(PlayerInteraction player)
    {
        GameManager.Instance.AddMoney(value);
        // In the future this method should be called through the stealing animation
        player.ForceUIChange();
        // In the future this should be destroyed after the stealing animation
        Destroy(gameObject);
    }
}
