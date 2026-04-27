using System.Collections;
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
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        pm.PlayStealAndUnlock();
        player.ForceUIChange();
    }

}
