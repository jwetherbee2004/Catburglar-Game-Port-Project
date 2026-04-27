using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    public void PlayFootstep()
    {
        EnemyPatrol enemyPatrol = GetComponentInParent<EnemyPatrol>();
        if (enemyPatrol != null)
        {
            enemyPatrol.PlayFootstep();
        }
    }

    public void PlayEnemySleep()
    {
        EnemyPatrol enemyPatrol = GetComponentInParent<EnemyPatrol>();
        if (enemyPatrol != null)
        {
            enemyPatrol.PlaySleep();
        }
    }
    
    public void PlayCameraMove()
    {
        SecurityCamera securityCamera = GetComponentInParent<SecurityCamera>();
        if (securityCamera != null)
        {
            securityCamera.PlayCameraMove();
        }
    }
}
