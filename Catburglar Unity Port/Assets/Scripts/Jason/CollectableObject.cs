using UnityEngine;

public class CollectableObject : MonoBehaviour, IInteractable
{
    public string InteractionText => "Steal";
    [SerializeField] int value = 0;
    public GameObject Object => gameObject;
    public Transform player;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    public void Interact(PlayerInteraction player)
    {
        GameManager.Instance.AddMoney(value);
        player.GetComponent<PlayerMovement>().GetAnimator().SetTrigger("Steal");
        // In the future this method should be called through the stealing animation
        player.ForceUIChange();
        // In the future this should be destroyed after the stealing animation
        // Destroy(gameObject);
    }
}
