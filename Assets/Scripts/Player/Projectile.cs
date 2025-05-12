using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private float damageSkill;
    private float direction;
    private bool hit;
    private BoxCollider2D boxCollider;
    private Animator anim;
    private float lifetime; //so that fireball doesn't exist infinitely if doesn't collide with anything
    private bool isSkillProjectile;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > 2.5) gameObject.SetActive(false); //set how long lifetime is
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        boxCollider.enabled = false;
        anim.SetTrigger("explode");

        /*
        if (collision.tag == "Enemy")
            collision.GetComponent<Health>().TakeDamage(damage);
        */

        if (collision.tag == "Enemy")
        {
            float appliedDamage = isSkillProjectile ? damageSkill : damage;
            collision.GetComponent<Health>().TakeDamage(appliedDamage);
        }
    }

    public void SetDirection(float _direction, bool isSkill = false) //added
    {
        lifetime = 0;
        direction = _direction;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;
        isSkillProjectile = isSkill; //added

        //check localScale.x and if it doesn't match direction, reverses it
        float localScaleX = transform.localScale.x;
        if (Mathf.Sign(localScaleX) != _direction)
            localScaleX = -localScaleX;

        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
