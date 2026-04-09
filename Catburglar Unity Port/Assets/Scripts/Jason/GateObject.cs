using UnityEngine;

public class Gate : MonoBehaviour, IInteractable
{
    public string InteractionText => "Enter";
    [SerializeField] Gate connectedGate;
    [SerializeField] GameObject border;
    public GameObject Object => gameObject;
    public Animator animator;
    private GameObject playerObject;
    private bool borderActive = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Interact(PlayerInteraction player)
    {
        playerObject = player.gameObject;
        animator.SetTrigger("Open");
    }

    public void TeleportPlayer()
    {
        Vector3 endPos = connectedGate.gameObject.transform.position;
        endPos.Set(endPos.x-0.5f, endPos.y, endPos.z);
        playerObject.transform.position = endPos;
        connectedGate.animator.SetTrigger("OpenNoTP");
    }

    public void ShowHideBorder()
    {
        connectedGate.ShowHideBorderNoCall();
    }
    public void ShowHideBorderNoCall()
    {
        borderActive = !borderActive;
        border.SetActive(borderActive);
    }
}
