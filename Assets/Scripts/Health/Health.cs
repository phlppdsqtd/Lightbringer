using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header ("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set;} //added this since set to public
    private Animator anim;
    private bool dead;

    [Header ("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;
    
    [Header ("Components")]
    [SerializeField] private Behaviour[] components;
    private bool invulnerable;

    [Header ("Sound")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;

    [Header("Mana Drop (Enemy Only)")]
    [SerializeField] private GameObject manaDropPrefab; //mana collectible prefab
    [SerializeField] private bool isEnemy = false; //flag to indicate if this is an enemy
    [SerializeField] private Transform dropPoint; //point where the mana drop should spawn

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();

        if (isEnemy && UIManager.Instance != null)
        {
            UIManager.Instance.RegisterEnemy(gameObject);
        }
    }

    public void TakeDamage(float _damage)
    {
        if (invulnerable) return;
        
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0) {
            //player hurt
            anim.SetTrigger("hurt");
            //iframes
            StartCoroutine(Invulnerability());
            SoundManager.instance.PlaySound(hurtSound);
        }
        else {
            if (!dead)
            {
                //deactive all attached component classes
                foreach (Behaviour component in components)
                {
                    component.enabled = false;
                }

                //check if enemy and drop mana
                if (isEnemy && manaDropPrefab != null)
                {
                    //always drop mana
                    Instantiate(manaDropPrefab, dropPoint != null ? dropPoint.position : transform.position, Quaternion.identity);
                }
                
                if (gameObject.CompareTag("Player"))
                {
                    anim.SetBool("grounded", true);
                }

                //update UI counter for enemy
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UnregisterEnemy(gameObject);
                }

                anim.SetTrigger("die");
                dead = true;
                SoundManager.instance.PlaySound(deathSound);
            }

        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }
    
    public void Respawn()
    {
        dead = false;
        AddHealth(startingHealth);
        anim.ResetTrigger("die");
        anim.Play("Idle");
        StartCoroutine(Invulnerability());

        //activate all attached component classes
        foreach (Behaviour component in components)
        {
            component.enabled = true;
        }
    }

    private IEnumerator Invulnerability()
    {
        invulnerable = true;
        Physics2D.IgnoreLayerCollision(10,11,true); //player at layer 10, enemy at layer 11
        for (int i=0; i < numberOfFlashes; i++) {
            spriteRend.color = new Color (1,0,0,0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        Physics2D.IgnoreLayerCollision(10,11,false);
        invulnerable = false;
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
