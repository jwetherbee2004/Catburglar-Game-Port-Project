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
        }
    }

    // Shows/Hides the interaction UI prompt
    void ShowHideUI()
    {
        uiShown = !uiShown;
        uiObject.SetActive(uiShown);
        if (currentInteractable != null)
        {
            interactText.text = currentInteractable.InteractionText;
            if (!CheckIfInteractable())
            {
                uiObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            ShowHideUI();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
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
    }

    bool CheckIfInteractable()
    {
        var searchObj = currentInteractable.Object.GetComponent<SearchObject>();
        if (searchObj != null)
        {
            if (searchObj.doneInteracting == true)
            {
                return false;
            }
        }
        else
        {
            var hackObj = currentInteractable.Object.GetComponent<HackObject>();
            if (hackObj != null)
            {
                if (hackObj.doneInteracting == true)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == currentInteractable)
        {
            currentInteractable = null;
            ShowHideUI();
        }
    }
}
