using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Enemy Settings")]
    public Transform enemy;
    public float speed;
    public float waitTime = 2f;
    
    private Vector3 initScale;
    private bool movingLeft;
    private float waitTimer;
    private bool isWaiting;
    private Animator animator;

    private void Awake()
    {
        initScale = enemy.localScale;
        animator = enemy.GetComponent<Animator>();
    }
    
    private void Update()
    {
        if (isWaiting)
        {
            animator.SetBool("isWalking", false);
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                movingLeft = !movingLeft;
            }
            return;
        }

        animator.SetBool("isWalking", true);

        if (movingLeft)
        {
            if (enemy.position.x >= pointA.position.x)
                MoveInDirection(-1);
            else
                DirectionChange();
        }
        else
        {
            if (enemy.position.x <= pointB.position.x)
                MoveInDirection(1);
            else
                DirectionChange();
        }
    }

    private void DirectionChange()
    {
        isWaiting = true;
        waitTimer = waitTime;
    }

    private void MoveInDirection(int _direction)
    {
        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * _direction, initScale.y, initScale.z);
        enemy.position = new Vector3(enemy.position.x + Time.deltaTime * _direction * speed, enemy.position.y, enemy.position.z);
    }
}