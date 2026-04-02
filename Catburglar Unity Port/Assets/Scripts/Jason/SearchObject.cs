using UnityEngine;

public class SearchObject : MonoBehaviour, IInteractable
{
    public string InteractionText => "Search";
    [SerializeField] int value = 0;
    public bool doneInteracting = false;
    public GameObject Object => gameObject;

    public void Interact(PlayerInteraction player)
    {
        GameManager.Instance.AddMoney(value);
        doneInteracting = true;
    }
}
