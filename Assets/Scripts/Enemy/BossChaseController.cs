using UnityEngine;

public class BossChaseController : MonoBehaviour
{
    [SerializeField] private Transform bossBody; // reference to the visual (animated) part
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask playerLayer;

    [Header("Combat References")]
    [SerializeField] private BossEnemy bossEnemy; // Reference to BossEnemy script
    [SerializeField] private float stopDistance; // minimum distance to stop near the player
    [SerializeField] private float meleeRange; // Distance to stop for melee

    private Transform player;
    private Animator anim;
    private float scaleX;

    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        anim = bossBody.GetComponent<Animator>();
        scaleX = Mathf.Abs(bossBody.localScale.x);
    }

    
    //original
    private void Update()
    {
        if (!CanMove || player == null || bossEnemy == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool isRangedReady = bossEnemy.IsRangedAttackReady();

        float targetStopDistance = isRangedReady ? stopDistance : meleeRange;

        if (distance > targetStopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = new Vector2(transform.position.x + direction.x * moveSpeed * Time.deltaTime, transform.position.y);

            // Flip boss sprite to face player
            if (direction.x != 0)
                bossBody.localScale = new Vector3(-scaleX * Mathf.Sign(direction.x), bossBody.localScale.y, bossBody.localScale.z);
            anim.SetBool("moving", true);
        }
        else
        {
            anim.SetBool("moving", false);
        }
    }
    

    /*
    private void Update()
    {
        if (!CanMove || player == null || bossEnemy == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool isRangedReady = bossEnemy.IsRangedAttackReady();
        bool isPlayerInRanged = bossEnemy.IsPlayerInRangedSight();

        float targetStopDistance;

        // If player is in ranged sight AND attack is ready, stop earlier
        if (isPlayerInRanged && isRangedReady)
            targetStopDistance = stopDistance;
        else
            targetStopDistance = meleeRange;

        if (distance > targetStopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = new Vector2(transform.position.x + direction.x * moveSpeed * Time.deltaTime, transform.position.y);

            // Flip boss sprite to face player
            if (direction.x != 0)
                bossBody.localScale = new Vector3(-scaleX * Mathf.Sign(direction.x), bossBody.localScale.y, bossBody.localScale.z);

            anim.SetBool("moving", true);
        }
        else
        {
            anim.SetBool("moving", false);
        }
    }
    */

    public void SetPlayer(Transform target)
    {
        player = target;
    }
}
