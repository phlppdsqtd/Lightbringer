using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header ("Patrol Points")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [Header ("Enemy")]
    [SerializeField] private Transform enemy;

    [Header ("Movement parameters")]
    [SerializeField] private float speed;
    private Vector3 initScale;
    private bool movingLeft;

    [Header ("Idle Behavior")]
    [SerializeField] private float idleDuration;
    private float idleTimer;

    [Header ("Enemy Animator")]
    [SerializeField] private Animator anim;

    private void Awake()
    {
        initScale = enemy.localScale;
    }

    private void Update()
    {
        if (movingLeft)
        {
            if (enemy.position.x >= leftEdge.position.x)
                MoveInDirection(-1);
            else
            {
                //change direction
                DirectionChange();
            }
        }
        else
        {
            if (enemy.position.x <= rightEdge.position.x)
                MoveInDirection(1);
            else
            {
                //change direction
                DirectionChange();
            }
        }

    }

    private void OnDisable()
    {
        anim.SetBool("moving", false);
    }

    private void DirectionChange()
    {
        anim.SetBool("moving", false);
        idleTimer += Time.deltaTime;

        if (idleTimer > idleDuration)
            movingLeft = !movingLeft;
    }

    private void MoveInDirection(int _direction)
    {
        idleTimer = 0;
        anim.SetBool("moving", true);

        //make enemy face direction
        //enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * _direction, initScale.y, initScale.z);
        //enemy.localScale = new Vector3(Mathf.Sign(initScale.x) * _direction * Mathf.Abs(initScale.x), initScale.y, initScale.z);
        enemy.localScale = new Vector3(-_direction * Mathf.Abs(initScale.x), initScale.y, initScale.z);

        //move in that direction
        enemy.position = new Vector3(enemy.position.x + Time.deltaTime * _direction * speed,
            enemy.position.y, enemy.position.z);
    }

        public float GetDirection()
    {
        return Mathf.Sign(enemy.localScale.x); // or however you store direction
    }
}
