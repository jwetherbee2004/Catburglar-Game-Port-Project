using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject uiObject;
    [SerializeField] private TextMeshProUGUI interactText;
    private IInteractable currentInteractable;
    private bool uiShown = false;

    // Update is called once per frame
    void Update()
    {
        if (uiShown && Input.GetKeyDown(KeyCode.X))
        {
            currentInteractable.Interact(this);
            // Forces a UI refresh
            ShowHideUI();
        }
    }

    // Shows/Hides the interaction UI prompt
    void ShowHideUI()
    {
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
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
        {
            return;
        }

        currentInteractable = interactable;

        // This is a catch for if the player is moved but still in a trigger
        if (!uiShown)
        {
            ShowHideUI();
        }
        // This stops the player from interacting with the same object repeatedly
        if (!CheckIfInteractable())
        {
            uiObject.SetActive(false);
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
}
