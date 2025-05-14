using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header ("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;
    //[SerializeField] private int damage;

    [Header ("Ranged Attack")]
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject[] fireballs;

    [Header ("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header ("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;
    
    [Header ("Fireball Sound")]
    [SerializeField] private AudioClip fireballSound;

    //references
    private Animator anim;
    private Health playerHealth;
    //private EnemyPatrol enemyPatrol;

    // New:
    private RangedEnemyPatrol enemyPatrol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        //enemyPatrol = GetComponentInParent<EnemyPatrol>();

        // New in Awake():
        enemyPatrol = GetComponentInParent<RangedEnemyPatrol>();
    }

    
    //original
    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        //attack only when player in sight
        if (PlayerInSight())
        {
            if (cooldownTimer >= attackCooldown && playerHealth != null && playerHealth.currentHealth > 0)
            {
                cooldownTimer = 0;
                anim.SetTrigger("rangedAttack");
            }
        }
        
        if (enemyPatrol != null)
            enemyPatrol.enabled = !PlayerInSight();
    }
    

    /*
    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        bool inSight = PlayerInSight();

        if (enemyPatrol != null)
        {
            if (inSight)
                enemyPatrol.Pause(); // Stop movement but allow control
            else
                enemyPatrol.enabled = true; // Re-enable patrol if not in sight
        }

        if (inSight && cooldownTimer >= attackCooldown && playerHealth != null && playerHealth.currentHealth > 0)
        {
            cooldownTimer = 0;
            anim.SetTrigger("rangedAttack");
        }
    }
    */

    private void RangedAttack()
    {
        SoundManager.instance.PlaySound(fireballSound);
        cooldownTimer = 0;
        fireballs[FindFireball()].transform.position = firepoint.position;
        fireballs[FindFireball()].GetComponent<EnemyProjectile>().ActivateProjectile();
    }

    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if(!fireballs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    /*
    //original
    private bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
        new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
        0, Vector2.left, 0, playerLayer);

        if (hit.collider != null)
        {
        playerHealth = hit.transform.GetComponent<Health>();
        }

        return hit.collider != null;
    }
    */

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
        new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }
    
    private bool PlayerInSight()
    {
        Vector2 castDirection = transform.right * Mathf.Sign(transform.localScale.x);

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + (Vector3)castDirection * range * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, castDirection, 0, playerLayer
        );

        if (hit.collider != null)
            playerHealth = hit.transform.GetComponent<Health>();

        return hit.collider != null;
    }

    
}
