using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Ranged Attack Parameters")]
    [SerializeField] private float rangedCooldown;
    [SerializeField] private float rangedRange;
    //[SerializeField] private int rangedDamage;
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private AudioClip projectileSound;

    [Header("Melee Attack Parameters")]
    [SerializeField] private float meleeCooldown;
    [SerializeField] private float meleeRange;
    [SerializeField] private int meleeDamage;
    [SerializeField] private AudioClip meleeSound;

    [Header("Collider Parameters")]
    [SerializeField] private float rangedColliderDistance;
    [SerializeField] private float meleeColliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;

    // TESTING ---------------------------------------------------------
    private float rangedTimer = Mathf.Infinity; //Mathf.Infinity //matches cooldown
    private float meleeTimer = Mathf.Infinity;

    private Animator anim;
    private Health playerHealth;
    //private RangedEnemyPatrol enemyPatrol;
    private BossChaseController chaseController;
    //public float RangedRange => rangedRange;
    //public float MeleeRange => meleeRange;
    //enum AttackType { None, Ranged, Melee }

    /*
    private void Awake()
    {
        anim = GetComponent<Animator>();
        //enemyPatrol = GetComponentInParent<RangedEnemyPatrol>();

        //added
        chaseController = GetComponent<BossChaseController>();
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            chaseController.SetPlayer(GameObject.FindGameObjectWithTag("Player").transform);
        }
    }
    */

    private void Awake()
    {
        anim = GetComponent<Animator>();
        chaseController = GetComponent<BossChaseController>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Transform playerTransform = playerObj.transform;
            chaseController.SetPlayer(playerTransform);
            playerHealth = playerTransform.GetComponent<Health>(); // <- Add this line
        }
    }

    /*
    //original
    private void Update()
    {
        rangedTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;

        //bool playerInRanged = PlayerInSight(rangedRange, rangedColliderDistance);
        //bool playerInMelee = PlayerInSight(meleeRange, meleeColliderDistance);

        //boss chase
        if (playerHealth != null && playerHealth.currentHealth > 0)
        {
            bool playerInRanged = PlayerInSight(rangedRange, rangedColliderDistance);
            bool playerInMelee = PlayerInSight(meleeRange, meleeColliderDistance);

            if (playerInRanged && rangedTimer >= rangedCooldown)
            {
                rangedTimer = 0;
                anim.SetTrigger("rangedAttack");
                chaseController.CanMove = false;
            }
            else if (playerInMelee && meleeTimer >= meleeCooldown)
            {
                meleeTimer = 0;
                anim.SetTrigger("meleeAttack");
                SoundManager.instance.PlaySound(meleeSound);
                chaseController.CanMove = false;
            }
            else
            {
                chaseController.CanMove = true; //true
            }
        }
    }
    */

    private void Update()
    {
        rangedTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;

        if (playerHealth != null && playerHealth.currentHealth > 0)
        {
            bool inRanged = IsPlayerInRanged();
            bool inMelee = IsPlayerInMelee();

            if (inRanged && rangedTimer >= rangedCooldown)
            {
                rangedTimer = 0;
                anim.SetTrigger("rangedAttack");
                chaseController.CanMove = false;
            }
            else if (inMelee && meleeTimer >= meleeCooldown && (!inRanged || rangedTimer < rangedCooldown))
            {
                meleeTimer = 0;
                anim.SetTrigger("meleeAttack");
                SoundManager.instance.PlaySound(meleeSound);
                chaseController.CanMove = false;
            }
            else
            {
                chaseController.CanMove = true;
            }
        }
    }

    /*
    private void Update()
    {
        rangedTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;

        if (playerHealth != null && playerHealth.currentHealth > 0)
        {
            bool playerInRanged = IsPlayerInRanged();
            bool playerInMelee = IsPlayerInMelee();

            if (playerInRanged && rangedTimer >= rangedCooldown)
            {
                rangedTimer = 0;
                anim.SetTrigger("rangedAttack");
                chaseController.CanMove = false;
            }
            else if (playerInMelee && meleeTimer >= meleeCooldown && rangedTimer < rangedCooldown)
            {
                // Only melee if:
                // - player is in melee range AND
                // - not eligible for a ranged attack (either out of range or on cooldown)
                meleeTimer = 0;
                anim.SetTrigger("meleeAttack");
                SoundManager.instance.PlaySound(meleeSound);
                chaseController.CanMove = false;
            }
            else
            {
                chaseController.CanMove = true;
            }
        }
    }
    */

    /*
    private void Update()
    {
        rangedTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;

        if (playerHealth != null && playerHealth.currentHealth > 0)
        {
            AttackType nextAttack = GetNextAttack();

            switch (nextAttack)
            {
                case AttackType.Ranged:
                    rangedTimer = 0;
                    anim.SetTrigger("rangedAttack");
                    chaseController.CanMove = false;
                    break;

                case AttackType.Melee:
                    meleeTimer = 0;
                    anim.SetTrigger("meleeAttack");
                    SoundManager.instance.PlaySound(meleeSound);
                    chaseController.CanMove = false;
                    break;

                case AttackType.None:
                    chaseController.CanMove = true;
                    break;
            }
        }
    }
    */

    // Ranged attack logic
    private void RangedAttack()
    {
        SoundManager.instance.PlaySound(projectileSound);
        GameObject projectile = projectiles[FindProjectile()];
        projectile.transform.position = firepoint.position;
        projectile.GetComponent<EnemyProjectile>().ActivateProjectile();
        //chaseController.CanMove = true; //added
    }

    /*
    // Melee attack logic
    private void MeleeDamage()
    {
        if (PlayerInSight(meleeRange, meleeColliderDistance))
        {
            playerHealth.TakeDamage(meleeDamage);
            //chaseController.CanMove = true; //added
        }
    }
    */

    private void MeleeDamage()
    {
        if (IsPlayerInMelee())
        {
            playerHealth.TakeDamage(meleeDamage);
        }
    }

    /*
    // Shared raycast logic
    private bool PlayerInSight(float range, float distance)
    {
        Vector2 castDirection = transform.right * Mathf.Sign(transform.localScale.x);

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + (Vector3)castDirection * range * distance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, castDirection, 0, playerLayer
        );

        if (hit.collider != null)
        {
            if (playerHealth == null)
                playerHealth = hit.transform.GetComponent<Health>();
            return true;
        }
        return false;
    }
    */

    private bool IsPlayerInRanged()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)boxCollider.bounds.center + direction * rangedRange * rangedColliderDistance;
        Vector2 size = new Vector2(boxCollider.bounds.size.x * rangedRange, boxCollider.bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.zero, 0, playerLayer);
        return hit.collider != null;
    }

    private bool IsPlayerInMelee()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)boxCollider.bounds.center + direction * meleeRange * meleeColliderDistance;
        Vector2 size = new Vector2(boxCollider.bounds.size.x * meleeRange, boxCollider.bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.zero, 0, playerLayer);
        return hit.collider != null;
    }

    /*
    private bool PlayerInSight(float range, float distance)
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)boxCollider.bounds.center + direction * range * distance;

        Vector2 size = new Vector2(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.zero, 0, playerLayer);

        if (hit.collider != null)
        {
            if (playerHealth == null)
                playerHealth = hit.transform.GetComponent<Health>();
            return true;
        }
        return false;
    }
    */

    private int FindProjectile()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (!projectiles[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 offset = transform.right * Mathf.Sign(transform.localScale.x);
        Gizmos.DrawWireCube(boxCollider.bounds.center + offset * rangedRange * rangedColliderDistance,
            new Vector3(boxCollider.bounds.size.x * rangedRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCollider.bounds.center + offset * meleeRange * meleeColliderDistance,
            new Vector3(boxCollider.bounds.size.x * meleeRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }

    public bool IsRangedAttackReady()
    {
        return rangedTimer >= rangedCooldown;
    }

    /*
    public bool IsPlayerInRangedSight()
    {
        return PlayerInSight(rangedRange, rangedColliderDistance);
    }

    public bool IsPlayerInMeleeSight()
    {
        return PlayerInSight(meleeRange, meleeColliderDistance);
    }
    *

    /*
    private AttackType GetNextAttack()
    {
        bool inRanged = PlayerInSight(rangedRange, rangedColliderDistance);
        bool inMelee = PlayerInSight(meleeRange, meleeColliderDistance);

        if (inRanged && rangedTimer >= rangedCooldown)
            return AttackType.Ranged;
        if (inMelee && meleeTimer >= meleeCooldown && !(inRanged && rangedTimer < rangedCooldown))
            return AttackType.Melee;
        return AttackType.None;
    }
    */
    
}