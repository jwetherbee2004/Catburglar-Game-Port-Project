using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DialoguePoint : MonoBehaviour, IInteractable
{
    public string InteractionText => "Skip";
    public bool used = false;
    [SerializeField] public string dialogueText;
    [SerializeField] public GameObject dialogueBox;
    public GameObject Object => gameObject;

    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
    public void Interact(PlayerInteraction player)
    {
        player.ForceUIChange();
        player.inDialogue = false;
        player.gameObject.GetComponent<PlayerMovement>().UnlockMovement();
        used = true;
        dialogueBox.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
        player.gameObject.GetComponent<PlayerMovement>().ToggleIsInDialoguePoint(false);
    }

    private void OnDestroy()
    {
        Debug.Log("DialoguePoint destroyed: " + gameObject.name);
    }
}
