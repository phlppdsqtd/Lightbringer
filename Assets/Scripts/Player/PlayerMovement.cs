using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement Parameters")]
    [SerializeField] private float speed; //same as public float speed;
    [SerializeField] private float jumpPower;

    /*
    [Header ("Coyote Time")]
    [SerializeField] private float coyoteTime; //how much time the player can hang in the air before jumping
    private float coyoteCounter; //how much time has passed since the player ran off edge of object
    */

    [Header ("Multiple Jumps")]
    [SerializeField] private int extraJumps;
    private int jumpCounter;

    [Header("Jump Cooldown")]
    [SerializeField] private float jumpCooldown; // Time between jumps
    private float lastJumpTime; // When last jump occurred

    [Header ("Wall Jumping")]
    [SerializeField] private float wallJumpX; //horizontal wall jump force
    [SerializeField] private float wallJumpY; //vertical wall jump force

    [Header ("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header ("SFX")]
    [SerializeField] private AudioClip jumpSound;

    /*
    [Header("Jump States")]
    private bool hasUsedCoyoteJump = false; // Track if coyote jump was used
    private bool hasLeftGround = false; // Track if player left ground
    */

    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float wallJumpCooldown;
    private float horizontalInput;
    private UIManager uiManager;
    private bool doubleJumpEnabled = false;

    private void Awake()
    {
        //grab references for rigidbody and animator from object
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        uiManager = FindFirstObjectByType<UIManager>();
        extraJumps = 0;
    }

    private void Start()
    {
        if (PlayerSkillManager.instance != null && PlayerSkillManager.instance.HasSkill("Double Jump"))
        {
            EnableDoubleJump();
            doubleJumpEnabled = true;
        }
    }
    
    /*
    private void Start()
    {
        Debug.Log("Starting DelayedSkillApply Coroutine");
        StartCoroutine(DelayedSkillApply());
    }

    private IEnumerator DelayedSkillApply()
    {
        while (PlayerSkillManager.instance == null || !PlayerSkillManager.instance.HasSkill("DoubleJump"))
        {
            yield return null;
        }

        if (!doubleJumpEnabled)
        {
            EnableDoubleJump();
            doubleJumpEnabled = true;
            Debug.Log("DOUBLE JUMP ENABLED");
        }
    }
    */

    //for debug
    //private float logTimer = 0f;

    private void Update()
    {
        /*
        //added debug--------------------------------------------
        logTimer += Time.deltaTime;
        if (logTimer >= 0.25f)
        {
            Debug.Log("Extra jumps: " + extraJumps);
            Debug.Log($"Grounded: {isGrounded()}, Jumps: {jumpCounter}/{extraJumps}, Enabled: {doubleJumpEnabled}");
            logTimer = 0f;
        }
        */

        horizontalInput = Input.GetAxis("Horizontal");

        // Block all input if dialogue or controls UI is active
        if (uiManager != null && (uiManager.IsTalking() || uiManager.IsControlsUIActive()))
        {
            body.linearVelocity = new Vector2(0, body.linearVelocity.y); // Freeze horizontal movement
            anim.SetBool("run", false);
            anim.ResetTrigger("jump");
            return; 
        }

        //added this portion since wallJumpCooldown removed from ep12 tutorial
        wallJumpCooldown += Time.deltaTime;

        //changes direction of sprite if left/right
        if (horizontalInput > 0.01f) {
            //transform.localScale = Vector3.one;
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z); //change 1 to eg 0.3f if scaled to 0.3
        }
        else if (horizontalInput < -0.01f) {
            //transform.localScale = new Vector3(-1, 1, 1);
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z); //change 1 to eg 0.3f if scaled to 0.3
        }

        //set animator parameters
        anim.SetBool("run", horizontalInput !=0);
        anim.SetBool("grounded", isGrounded());

        //jump
        if (Input.GetKeyDown(KeyCode.Space))
            if(Time.timeScale != 0) //added since jump still functions if paused
                Jump();
        
        //adjustable jump height
        /*If the player taps Space, they jump lower. If they hold Space, they get the full jump height. This allows for variable-height jumps.*/
        if (Input.GetKeyUp(KeyCode.Space) && body.linearVelocity.y > 0)
            body.linearVelocity = new Vector2 (body.linearVelocity.x, body.linearVelocity.y / 2);

        if (onWall())
        {
            body.gravityScale = 0;
            body.linearVelocity = Vector2.zero;
        }
        else
        {
            body.gravityScale = 7;
            body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);

            /*
            //original
            if (isGrounded())
            {
                coyoteCounter = coyoteTime; //reset coyote counter when on ground

                //original
                jumpCounter = extraJumps; //reset jump counter to extra jump value
            }
            else
                coyoteCounter -= Time.deltaTime; //start decreasing coyote counter when not on the ground
            */

            if (isGrounded())
            {
                jumpCounter = extraJumps; // Reset jumps when grounded
            }
        }
    }

    /*
    //original
    private void Jump()
    {
        //added here since jump animation not playing if following tutorial
        //in animator, changed condition back to "jump" instead of "grounded"=false
        anim.SetTrigger("jump");

        if (coyoteCounter < 0 && !onWall() && jumpCounter <= 0) return;
        //if coyote counter is 0 or less and not on the wall and don't have any extra jumps don't do anything
        
        SoundManager.instance.PlaySound(jumpSound);

        if (onWall())
        {
            //WallJump();
            
            //changed to this since wallJumpCooldown was edited in ep12
            if (wallJumpCooldown > 0.2f)
                WallJump();
            return;
        }
        else
        {
            if (isGrounded())
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
            else
            {
                //if not on the ground and coyote counter > 0, do a normal jump
                if (coyoteCounter > 0) //if we have extra jumps then jump and decrease the jump counter
                    body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
                else
                {
                    if (jumpCounter > 0)
                    {
                        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
                        jumpCounter--;
                    }
                }
            }
            //reset coyote counter to 0 to avoid double jump
            coyoteCounter = 0;
        }
    }
    */

    private void Jump()
    {
        // Check jump cooldown first
        if (Time.time - lastJumpTime < jumpCooldown)
            return;

        /*
        //removed since not gonna implement wall jump
        // Wall jump has priority
        if (onWall())
        {
            if (wallJumpCooldown > 0.2f)
            {
                PerformJump();
                wallJumpCooldown = 0;
                lastJumpTime = Time.time;
            }
            return;
        }
        */

        // Ground jump
        if (isGrounded())
        {
            PerformJump();
            jumpCounter = extraJumps; // Reset jumps when grounded
            lastJumpTime = Time.time;
            return;
        }

        // Double jump (only when skill is enabled)
        if (doubleJumpEnabled && jumpCounter > 0)
        {
            PerformJump();
            jumpCounter--;
            lastJumpTime = Time.time;
            return;
        }
    }

    private void PerformJump()
    {
        anim.SetTrigger("jump");
        SoundManager.instance.PlaySound(jumpSound);
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
    }

    /*
    //removed since not used
    private void WallJump()
    {
        body.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX, wallJumpY));
        wallJumpCooldown = 0;
    }
    */

    //ORIGINAL  
    private bool isGrounded() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    
    /*
    //V2
    private bool isGrounded()
    {
        // Increase raycast distance slightly
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, 
                                            boxCollider.bounds.size * 0.9f, 
                                            0, 
                                            Vector2.down, 
                                            0.15f,  // Increased from 0.1f
                                            groundLayer);
        return hit.collider != null;
    }
    */

    private bool onWall() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool canAttack() {
        return horizontalInput == 0 && isGrounded() && !onWall();
    }

    #region SKILL
    public void EnableDoubleJump()
    {
        extraJumps = 1;
    }

    private void OnEnable()
    {
        PlayerSkillManager.OnSkillUnlocked += HandleSkillUnlocked;
    }

    private void OnDisable()
    {
        PlayerSkillManager.OnSkillUnlocked -= HandleSkillUnlocked;
    }

    private void HandleSkillUnlocked(string skillName)
    {
        if (skillName == "Double Jump" && !doubleJumpEnabled)
        {
            EnableDoubleJump();
            doubleJumpEnabled = true;
        }
    }
    #endregion
}