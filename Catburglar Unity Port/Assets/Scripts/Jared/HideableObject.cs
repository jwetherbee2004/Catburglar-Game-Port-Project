using UnityEngine;

public class HideableObject : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private bool playerPresent = false;

    void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (playerPresent && playerMovement != null)
        {
            if (playerMovement.isCrouching)
            {
                GameManager.Instance.SetHidden(true);
            }
            else
            {
                GameManager.Instance.SetHidden(false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerPresent = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerPresent = false;
            GameManager.Instance.SetHidden(false);
        }
    }
}
