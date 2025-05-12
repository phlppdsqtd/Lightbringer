using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;

    [Header ("Basic Attack")]
    [SerializeField] private GameObject[] lightballs;
    [SerializeField] private AudioClip lightballsSound;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float manaCost;

    [Header ("Skill")]
    [SerializeField] private GameObject[] fireballSkill;
    [SerializeField] private AudioClip fireballSkillSound;
    [SerializeField] private float fireballSkillCooldown;
    [SerializeField] private float fireballSkillManaCost;

    private PlayerSkillManager playerSkillManager;
    private Mana mana;
    private UIManager uiManager;
    private Animator anim;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity; //so that cooldown is not 0 at start of game; player can attack immediately

    private void Awake()
    {
        mana = GetComponent<Mana>();
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        uiManager = FindFirstObjectByType<UIManager>();
        
        // Delay this until the instance is available
        //playerSkillManager = FindFirstObjectByType<PlayerSkillManager>();

        /*
        if (playerSkillManager == null)
        {
            Debug.LogError("PlayerSkillManager instance not found! Make sure it exists in the scene.");
        }
        */
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayerSkillManager());
    }


    private IEnumerator WaitForPlayerSkillManager()
    {
        while (PlayerSkillManager.instance == null)
            yield return null;

        playerSkillManager = PlayerSkillManager.instance;

        // (Optional) If you want to check whether Fireball is unlocked at start:
        if (playerSkillManager.HasSkill("Fireball"))
            Debug.Log("Fireball skill unlocked!");
    }

    private void Update()
    {
        if (uiManager != null && (uiManager.IsTalking() || uiManager.IsControlsUIActive()))
            return;

        if(Input.GetMouseButton(0) && cooldownTimer > attackCooldown && playerMovement.canAttack())
        {
            if(Time.timeScale != 0 && mana.UseMana(manaCost)) //added since attack still functions if paused
            {    
                Attack();
            }
            else
            {
                uiManager.FlashManaBar(); //this line to trigger UI feedback
            }
        }

        if (Input.GetMouseButtonDown(1) && playerSkillManager != null && cooldownTimer > fireballSkillCooldown && playerMovement.canAttack())
        {
            if (Time.timeScale != 0 && playerSkillManager.HasSkill("Fireball"))
            {
                if (mana.GetCurrentMana() >= fireballSkillManaCost)
                {
                    mana.UseMana(fireballSkillManaCost);
                    FireballSkill();
                }
                else
                {
                    uiManager.FlashManaBar();
                }
            }
        }
        cooldownTimer += Time.deltaTime;
    }

    private void Attack() {
        SoundManager.instance.PlaySound(lightballsSound);
        
        anim.SetTrigger("attack");
        cooldownTimer = 0;

        //orig code
        lightballs[FindLightball()].transform.position = firePoint.position;
        lightballs[FindLightball()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));

        /*
        //revised >> error was in sorting layer
        int fireballIndex = FindFireball();
        lightballs[fireballIndex].transform.position = firePoint.position;
        lightballs[fireballIndex].SetActive(true); // Important: make sure it gets activated
        lightballs[fireballIndex].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
        */
    }

    private void FireballSkill()
    {
        SoundManager.instance.PlaySound(fireballSkillSound);

        anim.SetTrigger("fireball");
        cooldownTimer = 0;

        int index = FindFireball();
        fireballSkill[index].transform.position = firePoint.position;
        //fireballSkill[index].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
        fireballSkill[FindFireball()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x), true);
    }

    private int FindLightball() {
        for (int i = 0; i < lightballs.Length; i++) {
            if(!lightballs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    private int FindFireball()
    {
        for (int i = 0; i < fireballSkill.Length; i++)
        {
            if (!fireballSkill[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
