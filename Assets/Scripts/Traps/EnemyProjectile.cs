using UnityEngine;

public class EnemyProjectile : EnemyDamage //will damage the player every time they touch
{
    [SerializeField] private float speed;
    [SerializeField] private float resetTime;
    private float lifetime;
    private Animator anim;
    private BoxCollider2D coll;

    private bool hit;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
    }

    public void ActivateProjectile()
    {
        hit = false;
        lifetime = 0;
        gameObject.SetActive(true);
        coll.enabled = true;
    }

    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime;
        //transform.Translate(movementSpeed, 0, 0);
        transform.Translate(-Vector3.right * movementSpeed, Space.Self);
        //transform.Translate(-transform.right * speed * Time.deltaTime, Space.World);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
            gameObject.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        base.OnTriggerEnter2D(collision); //execute logic from parent script first
        coll.enabled = false;

        if(anim != null)
            anim.SetTrigger("enemy_explode"); //when the object is a ENEMY fireball explode it
        else
            gameObject.SetActive(false); //when this hits any object deactivate arrow
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
