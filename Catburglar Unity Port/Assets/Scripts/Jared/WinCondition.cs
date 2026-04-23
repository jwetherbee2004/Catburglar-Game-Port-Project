using UnityEngine;

public class WinCondition : MonoBehaviour
{
    private TimeTracker timeTracker;

    private void Start()
    {
        timeTracker = GameManager.Instance.timeTracker;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (timeTracker != null && timeTracker.IsTracking() && GameManager.Instance.GetLoot() >= 300)
            {
                timeTracker.TriggerWinCondition();
            }
        }    
    }
}
