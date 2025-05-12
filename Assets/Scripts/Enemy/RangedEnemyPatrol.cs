using UnityEngine;

public class RangedEnemyPatrol : MonoBehaviour
{
    [Header("Patrol Parameters")]
    //[SerializeField] private float moveDistance;
    [SerializeField] private float speed;
    [SerializeField] private float idleTime;
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [Header("Enemy")]
    [SerializeField] private Transform rangedEnemy; // Child object with animation, firepoint, etc.

    private Vector3 initScale;
    private bool movingLeft = true;
    private float idleTimer;
    private Animator anim;

    private void Awake()
    {
        initScale = rangedEnemy.localScale;
        anim = rangedEnemy.GetComponent<Animator>();
    }

    private void Update()
    {
        if (movingLeft)
        {
            if (rangedEnemy.position.x > leftEdge.position.x)
                MoveInDirection(-1);
            else
                Idle();
        }
        else
        {
            if (rangedEnemy.position.x < rightEdge.position.x)
                MoveInDirection(1);
            else
                Idle();
        }
    }

    private void Idle()
    {
        idleTimer += Time.deltaTime;
        SetMoving(false);

        if (idleTimer >= idleTime)
        {
            movingLeft = !movingLeft;
            idleTimer = 0f;
        }
    }

    private void MoveInDirection(int direction)
    {
        idleTimer = 0f;
        SetMoving(true);

        // Flip enemy to face movement direction
        //rangedEnemy.localScale = new Vector3(Mathf.Abs(rangedEnemy.localScale.x) * direction, rangedEnemy.localScale.y, rangedEnemy.localScale.z);
        rangedEnemy.localScale = new Vector3(Mathf.Abs(initScale.x) * -direction, initScale.y, initScale.z);

        // Move enemy
        rangedEnemy.position = new Vector3(rangedEnemy.position.x + Time.deltaTime * direction * speed, rangedEnemy.position.y, rangedEnemy.position.z);
    }

    private void SetMoving(bool state)
    {
        if (anim != null)
            anim.SetBool("moving", state);
    }

    /*
    public void Pause()
    {
        SetMoving(false); // Stop animation
    }
    */
    
    private void OnDisable()
    {
        SetMoving(false);
    }
    

    public float GetDirection()
    {
        return movingLeft ? -1f : 1f;
    }
}