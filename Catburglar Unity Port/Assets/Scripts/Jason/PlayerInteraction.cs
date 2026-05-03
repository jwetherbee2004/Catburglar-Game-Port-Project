using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject uiObject;
    [SerializeField] private TextMeshProUGUI interactText;
    public Transform player;
    private IInteractable currentInteractable;
    private bool uiShown = false;
    public bool inDialogue = false;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (uiShown && Input.GetKeyDown(KeyCode.X) && player.GetComponent<PlayerMovement>().canInteract)
        {
            var isDialogue = currentInteractable.Object.GetComponent<DialoguePoint>();
            if (isDialogue != null)
            {
                currentInteractable.Interact(this);
            }
            else if (!inDialogue)
            {
                currentInteractable.Interact(this);
            }
            // Forces a UI refresh
            ShowHideUI();
        }
    }

    // Shows/Hides the interaction UI prompt
    void ShowHideUI()
    {
        // Don't show UI if the player cannot interact
        if (!player.GetComponent<PlayerMovement>().canInteract)
        {
            uiShown = false;
            uiObject.SetActive(false);
            return;
        }
        
        uiShown = !uiShown;
        uiObject.SetActive(uiShown);
        if (currentInteractable == null)
        {
            return;
        }
        if (currentInteractable.Object == null)
        {
            return;
        }
        interactText.text = currentInteractable.InteractionText;
        if (!CheckIfInteractable())
        {
            uiObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var otherinteractable = other.GetComponent<DialoguePoint>();
        if (otherinteractable != null)
        {
            if (!otherinteractable.used)
            {
                if (currentInteractable != null)
                {
                    if (otherinteractable.Object != currentInteractable.Object)
                    {
                        currentInteractable = otherinteractable;
                        GameManager.Instance.DisplayDialogue(otherinteractable.dialogueText);
                        inDialogue = true;
                        return;
                    }
                } else
                {
                    currentInteractable = otherinteractable;
                    GameManager.Instance.DisplayDialogue(otherinteractable.dialogueText);
                    inDialogue = true;
                    return;
                }
            }
        }
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            ShowHideUI();
            var gate = other.GetComponentInChildren<Gate>();
            if (gate != null)
            {
                gate.ShowHideBorder();
            }
        }
        var yetanotherinteractable = other.GetComponent<TipPoint>();
        if (yetanotherinteractable != null)
        {
            if (!yetanotherinteractable.used)
            {
                yetanotherinteractable.SetTip();
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var otherinteractable = other.GetComponent<DialoguePoint>();
        if (otherinteractable != null)
        {
            if (!otherinteractable.used)
            {
                currentInteractable = otherinteractable;
                interactText.text = currentInteractable.InteractionText;
                return;
            }
        }

        if (inDialogue)
        {
            return;
        }

        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
        {
            return;
        }

        currentInteractable = interactable;
        
        bool canInteract = player.GetComponent<PlayerMovement>().canInteract;
        
        // Hide UI if player can't interact
        if (!canInteract && uiShown)
        {
            uiShown = false;
            uiObject.SetActive(false);
        }
        // Show UI if player can interact and UI is hidden
        else if (canInteract && !uiShown && CheckIfInteractable())
        {
            uiShown = true;
            uiObject.SetActive(true);
            if (currentInteractable != null && currentInteractable.Object != null)
            {
                interactText.text = currentInteractable.InteractionText;
            }
        }
    }

    bool CheckIfInteractable()
    {
        if (currentInteractable == null)
        {
            return false;
        }
        if (currentInteractable.Object == null)
        {
            return false;
        }

        var searchObj = currentInteractable.Object.GetComponent<SearchObject>();
        if (searchObj != null)
        {
            return !searchObj.doneInteracting;
        }

        var hackObj = currentInteractable.Object.GetComponent<HackObject>();
        if (hackObj != null)
        {
            return !hackObj.doneInteracting;
        }

        return true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == currentInteractable && interactable != null)
        {
            ShowHideUI();
            currentInteractable = null;
        }
        var gate = other.GetComponentInChildren<Gate>();
        if (gate != null)
        {
            gate.ShowHideBorder();
        }
    }

    public void ForceUIChange()
    {
        ShowHideUI();
    }

    public void DestroyStolenObject()
    {
        if (currentInteractable != null)
        {
            Destroy(currentInteractable.Object);
        }
    }
}
