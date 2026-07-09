using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    // I forgot that this class doesn't directly inherit from Monobehaviour
    // Considering that the class is somewhat generic, I will leave it alone

    //---Fields for movement
    [HideInInspector] public float speed;
    float moveSpeed;

    //---Fields for Attack---------------------------
    Vector2 orientationVector;
    Vector2 attackVector;
    [HideInInspector] public bool attackReady;
    bool attacked;
    [HideInInspector] public bool openWindow;
    [HideInInspector] public bool useFastAdvance;
    //public GameObject[] ammoCount;

    //---Fields for Attack Movement & Step-----------
    Vector2 stepVector;
    [HideInInspector] public float attackScalar;
    [HideInInspector] public bool isStepping;
    private bool canStep = true;
    bool stepped = false;
    [HideInInspector] public float stepTime;
    [HideInInspector] public float stepCooldown;
    [HideInInspector] public float stepSpeed;
    [HideInInspector] public float recoveryAmount;
    float recovery;
    private bool isfallingBack;

    //---Fields for dashing--------------------------
    [HideInInspector] public bool isDashing;
    private bool canDash = true;
    bool dashed = false;

    [HideInInspector] public float accelTime;
    [HideInInspector] public float accelAmount;
    private float dashAccel;

    [HideInInspector] public float totalDashTime;
    [HideInInspector] public float dashTime;
    [HideInInspector] public float dashCooldown;
    [HideInInspector] public float dashSpeed;

    [HideInInspector] public float decelTime;
    [HideInInspector] public float decelAmount;
    private float dashDecel;

    //---Fields for projectiles----------------------
    [HideInInspector] public float shootingWait;
    ObjectPooler objectPooler;
    private float timeToCount;
    //[HideInInspector] public bool projectileReady;

    //---Components----------------------------------
    [HideInInspector] public Transform firepath;
    private Rigidbody2D rb;
    private InputProcessor IP;
    private PlayerStatistics stats;
    private Hitbox hitbox;
    private PlayerHurtbox hurtbox;
    private Animator animator;
    private CircleCollider2D col;
    private Transform fxPosition;


    //---Other---------------------------------------
    [HideInInspector] public bool testingHurtbox;

    //---Direction Variables-------------------------
    //do something about all of these direction variables
    int dirState; // review this variable
    int moveDirection;
    string currentDirection;//-----------| All necessary
    string startDirection = "right";//---|
    string previousDirection;//----------V
    bool idle;
    bool moving;
    
    void Awake()
    {
        //later on i will incorporate scriptable objects so data can persist between scenes
        //sound = GetComponentInChildren<AudioSource>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStatistics>();
        rb = GetComponent<Rigidbody2D>();
        IP = GetComponent<InputProcessor>();
        hitbox = GetComponent<Hitbox>();
        col = GetComponent<CircleCollider2D>();
        // fxPosition = GameObject.FindGameObjectWithTag("FX Position").transform;
    }

    void Start()
    {
        recovery = recoveryAmount;
        moveSpeed = speed;
        currentDirection = startDirection;
        previousDirection = currentDirection;
        openWindow = false;
        objectPooler = ObjectPooler.Instance;
        attackReady = true;
        idle = true;
    }

    //edit rb.velocity in this function
    void FixedUpdate()
    {
        if (dashed || stepped || isfallingBack)
        {
            return;
        }

        if (!dashed && !attacked) rb.velocity = new Vector2(IP.movement.x, IP.movement.y) * moveSpeed;
        if (!dashed && attacked) rb.velocity = new Vector2(attackVector.x, attackVector.y) * attackScalar;

        //Debug.Log(rb.velocity);
    }

    // Everything else pertaining to controlling the character on screen will be handled in Update and LateUpdate
    void Update()
    {
        if (!stats.dead)
        {
            /*
            if (stats.currentHP <= 0)
            {
                if (!dead)
                {
                    Die();
                }

                dead = true;
                stats.dead = true;
            }
            */
            
            if (IP.pbutton && timeToCount == shootingWait)
            {
                StartCoroutine(ShootProjectile());
            }

            if (IP.dbutton && canDash)
            {
                if (IP.fxAnimationDirection == 4)
                    objectPooler.SpawnFromPool("Player Dash FX Right", fxPosition.position, 
                        fxPosition.right, fxPosition.rotation);
                if (IP.fxAnimationDirection == 3)
                    objectPooler.SpawnFromPool("Player Dash FX Left", transform.position,
                        fxPosition.right, fxPosition.rotation);

                StartCoroutine(Dash());
                //animator.SetTrigger("Dash");
                attackReady = false;
            }

            if (IP.abutton)
            {
                AnimateAttacks();
            }

            // for testing
            if (Input.GetKeyDown(KeyCode.F) && canStep)
            {
                StartCoroutine(Step());
            }
            
            if (!dashed && !attacked && !hitbox.wasHit)
            {
                AnimateMovement();
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    void AnimateAttacks()
    {
        //Debug.Log("ready: " + attackReady + ", testing: " + testingHurtbox + ", openWindow: " + openWindow);
        // The first attack will always execute and have attackScalar changed accordingly
        if (/*IP.abutton && */attackReady && !testingHurtbox && !openWindow)
        {
            attacked = true;
            attackReady = false;
            animator.SetInteger("Direction", IP.attackDirection);
            animator.SetTrigger("Attack A");
            StartCoroutine(Step());
        }
        else if (/*IP.abutton && */!attackReady && !testingHurtbox && openWindow)
        {
            animator.SetInteger("Direction", IP.attackDirection);
            //animator.SetBool("Fast Advance", useFastAdvance);
            animator.SetTrigger("Sub-Combo");
            animator.SetTrigger("Combo");
            StartCoroutine(Step());
        }
        
    }
    
    IEnumerator Step()
    {
        #region Note
        // When the player moves with the attack, i want it to feel as if they are 
        //  taking a step instead of gliding across the floor. This can be controlled
        //  in the animation, however:

        // There are couple of problem areas to think about when using animations to
        //  alter the magnitude of the movement vector.

        // Problem Area 1: Holes, or gaps in the animator that will allow code to still
        //  run without the animation altering key variables

        // Problem Area 2: The animator does not run in a fixed time loop, and as such
        //  the physics of movement in the animations will be off and not work properly
        //  when the framerate dips.

        // Problem Area 3: I have to go into each animation and manually change the
        //  value of the scalar and if I need to change that value of that scalar
        //  often, it soon becomes a huge pain.

        // I could go on, but I think it's clear that I need to switch to a more
        //  physics-based approach. So I guess I will be having a step mechanic.
        #endregion
        // Wait for FixedUpdate before updating values
        yield return new WaitForFixedUpdate();

        stepped = true;
        canStep = false;
        isStepping = true;

        //Vector2 initialMovement = Vector2.zero;
        if (IP.directionForAttack == "look")
        {
            rb.velocity = new Vector2(IP.lookX, IP.lookY) * stepSpeed;
        }
        else if (IP.directionForAttack == "move")
        {
            if (IP.movement != Vector2.zero) rb.velocity = IP.movement * stepSpeed;
            if (IP.movement == Vector2.zero) rb.velocity = orientationVector * stepSpeed;
        }

        for (float duration = stepTime; duration > 0; duration -= Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
        }

        isStepping = false;
        stepped = false;

        for (float duration = stepCooldown; duration > 0; duration -= Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector2.zero;

        canStep = true;
    }

    IEnumerator Dash()
    {
        // test the mechanics first, then add the animations
        
        // Wait for FixedUpdate before updating values
        yield return new WaitForFixedUpdate();

        // Setup
        dashed = true;
        canDash = false;
        isDashing = true; //not sure which one to use
        
        // set the initial acceleration and deceleration for the dash
        dashAccel = moveSpeed;
        dashDecel = dashSpeed;

        Vector2 initialMovement = Vector2.zero;
        if (IP.directionForDash == "move")
        {
            if (IP.movement != Vector2.zero) initialMovement = IP.movement;
            if (IP.movement == Vector2.zero) initialMovement = orientationVector;
        }
        else if (IP.directionForDash == "look")
        {
            initialMovement = IP.lookV;
        }

        //-------------------------------------------------------------------------------//

        // Wait for dashAccel to build up
        #region Note
        // WaitUntil and WaitWhile can't be used because it relies on a value outside of the coroutine
        //  Because the coroutine pauses its own execution until a certain condition is met,
        //  the coroutine will wait forever because it can't update it's own values when its execution
        //  is paused.

        // Saving this note for later, just in case I forget why I'm doing this.
        #endregion
        for (float duration = accelTime; duration > 0; duration -= Time.fixedDeltaTime)
        {
            dashAccel += accelAmount;

            // Dash will accelerate until hitting the dash speed
            dashAccel = Mathf.Clamp(dashAccel, moveSpeed, dashSpeed);
            rb.velocity = new Vector2(initialMovement.x, initialMovement.y) * dashAccel;
            yield return new WaitForFixedUpdate();
        }

        //-------------------------------------------------------------------------------//

        // Main dash starts
        rb.velocity = new Vector2(initialMovement.x, initialMovement.y) * dashSpeed;

        // Main dash stops
        for (float duration = dashTime; duration > 0; duration -= Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
        }

        //-------------------------------------------------------------------------------//

        // Dash starts decelerating
        for (float duration = decelTime; duration > 0; duration -= Time.fixedDeltaTime)
        {
            dashDecel -= decelAmount;

            // Dash will decelerate until hitting the base movement speed
            dashDecel = Mathf.Clamp(dashDecel, moveSpeed, dashSpeed);
            rb.velocity = new Vector2(initialMovement.x, initialMovement.y) * dashDecel;
            yield return new WaitForFixedUpdate();
        }

        //-------------------------------------------------------------------------------//

        // Player is no longer dashing however, they should still be able to move
        isDashing = false;
        dashed = false;

        // I force the player to wait for a cooldown period before they can dash again
        //  It won't be really long
        for (float duration = dashCooldown; duration > 0; duration -= Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
        }

        // The player can dash again
        canDash = true;
        attackReady = true;
    }
    
    IEnumerator Knockback(Vector2 knockbackDirection, float knockbackSpeed)
    {
        // Wait for FixedUpdate before updating values
        yield return new WaitForFixedUpdate();

        isfallingBack = true;
        float initialSpeed = knockbackSpeed;
        
        while (knockbackSpeed > 0)
        {
            //Debug.Log(knockbackDirection.normalized);
            rb.velocity = knockbackDirection.normalized * knockbackSpeed;
            knockbackSpeed -= recovery;
            //knockbackSpeed = Mathf.Clamp(knockbackSpeed, 0f, initialSpeed);
            yield return new WaitForFixedUpdate();
        }

        isfallingBack = false;
    }

    IEnumerator ShootProjectile()
    {
        animator.SetTrigger("Shoot");

        //firepath.right = (((Vector3)lookVector + firepath.position) - transform.position) + transform.position;
        firepath.right = ((Vector3)IP.lookV - transform.position) + transform.position;
        
        //sound.clip = clips[1];
        //sound.Play();

        GameObject ammoInstance;
        ammoInstance = objectPooler.SpawnFromPool("Tissy's Blade", firepath.position, firepath.right, firepath.rotation);
        //ammoInstance = objectPooler.GetPooledObject();

        //projectileReady = false;
        timeToCount = shootingWait;

        while (timeToCount > 0)
        {
            timeToCount -= Time.deltaTime;

            yield return null;
        }

        //projectileReady = true;
    }
    
    void AnimateMovement()
    {
        // review this method

        /*
         * one bug I've found is that when moving in two directions and you swtich one of the directions 
         *  you are moving and being animated in quickly enough that the animator either can't keep up or
         *  there isn't enough logic to help the cpu decide how to animate the character in those situations
         */
        
        if (IP.controller == 0)
        {
            int direction = 0;

            // The first 4 if statements determine the current direction

            // both of these if statements determine the current horizontal direction
            if (IP.moveH < 0)
            {
                // set previous direction is player was idle
                if (currentDirection == "none")
                {
                    previousDirection = "left";
                }

                currentDirection = "left";
                direction = 4;
                moving = true;
                idle = false;
            }
            else if (IP.moveH > 0)
            {
                // set previous direction is player was idle
                if (currentDirection == "none")
                {
                    previousDirection = "right";
                }

                currentDirection = "right";
                direction = 3;
                moving = true;
                idle = false;
            }

            // both of these if statements determine the current vertical direction
            if (IP.moveV < 0)
            {
                // set previous direction is player was idle
                if (currentDirection == "none")
                {
                    previousDirection = "down";
                }

                currentDirection = "down";
                direction = 2;
                moving = true;
                idle = false;
            }
            else if (IP.moveV > 0)
            {
                // set previous direction is player was idle
                if (currentDirection == "none")
                {
                    previousDirection = "up";
                }

                currentDirection = "up";
                direction = 1;
                moving = true;
                idle = false;
            }

            // These next 4 if statements determine the previous direction

            // Based on the logic, nothing changes when the player isn't moving
            //  in a direction that can change their orientation.
            // These statements allow for the previous direction to change when
            //  the player is no longer moving in a direction congruent with or
            //  opposite to the direction they started in.
            if (currentDirection == "up" && IP.moveH == 0)
            {
                previousDirection = "up";
                direction = 1;
                moveDirection = direction;
                moving = true;
                idle = false;
            }

            if (currentDirection == "down" && IP.moveH == 0)
            {
                previousDirection = "down";
                direction = 2;
                moveDirection = direction;
                moving = true;
                idle = false;
            }

            if (currentDirection == "right" && IP.moveV == 0)
            {
                previousDirection = "right";
                direction = 3;
                moveDirection = direction;
                moving = true;
                idle = false;
            }

            if (currentDirection == "left" && IP.moveV == 0)
            {
                previousDirection = "left";
                direction = 4;
                moveDirection = direction;
                moving = true;
                idle = false;
            }

            //if (currentDirection == "up" && previousDirection == "left")
            // Reset variables when player stops moving
            if (IP.moveH == 0 && IP.moveV == 0)
            {
                currentDirection = "none";
                idle = true;
                moving = false;
            }

            switch (direction)
            {
                // up
                case 1:
                    orientationVector = Vector2.up;
                    break;
                // down
                case 2:
                    orientationVector = Vector2.down;
                    break;
                // right
                case 3:
                    orientationVector = Vector2.right;
                    break;
                // left
                case 4:
                    orientationVector = Vector2.left;
                    break;
            }
        }
        else if (IP.controller == 1)
        {
            if (IP.movement != Vector2.zero)
            {
                moveDirection = IP.moveDirection;
                moving = true;
                idle = false;
            }
            else if (IP.movement == Vector2.zero)
            {
                idle = true;
                moving = false;
            }

            switch (moveDirection)
            {
                // up
                case 1:
                    orientationVector = Vector2.up;
                    break;
                // down
                case 2:
                    orientationVector = Vector2.down;
                    break;
                // right
                case 3:
                    orientationVector = Vector2.right;
                    break;
                // left
                case 4:
                    orientationVector = Vector2.left;
                    break;
            }
        }

        //Debug.Log("facing direction: " + orientationVector);
        //Debug.Log(moveDirection + " : " + previousDirection);
        //Debug.Log(currentDirection + " : " + previousDirection);

        animator.SetInteger("Direction", moveDirection);
        animator.SetBool("Idle", idle);
        animator.SetBool("Moving", moving);
    }
    
    //maybe reset direction var here using dirstate?
    public void ResetVars()
    {
        //make sure that resetvars is only called at the end of animations or at the beginning
        if (attacked)
        {
            attacked = false;
            attackReady = true;
        }
    }

    public void ReceiveKnockback(Vector2 knockbackVector, float velocity)
    {
        AnimateDamage(knockbackVector);
        if (!isfallingBack) StartCoroutine(Knockback(knockbackVector, velocity));
        if (isfallingBack)
        {
            StopCoroutine(Knockback(knockbackVector, velocity));
            StartCoroutine(Knockback(knockbackVector, velocity));
        }
    }

    void AnimateDamage(Vector2 animationDirection)
    {
        if (!stats.dead)
        {
            //for later:
            //setup dash to not be available when getting hit
            int direction = 0;
            if (animationDirection.x < 0)
            {
                direction = 3;
            }
            else if (animationDirection.x > 0)
            {
                direction = 4;
            }
            moveDirection = direction;
            animator.SetInteger("Direction", direction);
            animator.SetTrigger("Hit");
            //sound.clip = clips[2];
            //sound.Play();
        }
    }

    void Die()
    {
        animator.SetInteger("Direction", dirState);//might change dirState
        animator.SetTrigger("Dead");
        animator.SetBool("Stay Dead", true);
        //sound.clip = clips[3];
        //sound.Play();
    }
}
