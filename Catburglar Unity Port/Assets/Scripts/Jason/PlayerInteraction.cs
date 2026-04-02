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
        // This is a catch for if the player is moved but still in a trigger
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && !uiShown)
        {
            ShowHideUI();
        }
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
