using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Retreat : StateMachineBehaviour
{
    public float retreatSpeed = 8f;
    public float retreatTime = 0.25f;
    public float cooldownTime = 0.1f;
    public float approachRange = 6;

    Rigidbody2D rb;
    Transform player;
    float timeTillRetreatEnds;
    //float cooldown;
    int animationDirection;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

        timeTillRetreatEnds = Time.fixedTime + retreatTime;
        //cooldown = -1f;
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb.velocity = Vector2.zero;

        Vector2 target = new Vector2(player.position.x, player.position.y);
        Vector2 lookVector = target - (Vector2)animator.transform.position;
        Debug.DrawLine((Vector2)animator.transform.position, 
            (Vector2)animator.transform.position + lookVector, Color.green);

        //DetermineAnimationDirection(lookVector);
        //animator.SetInteger("Direction", animationDirection);
        
        if (Vector2.Distance(player.position, rb.position) <= approachRange)
        {
            // This defines the movement during a specific period of time
            if (Time.fixedTime < timeTillRetreatEnds)
            {
                //Vector2 newPos = Vector2.MoveTowards(rb.position, target, stepSpeed * Time.fixedDeltaTime);
                rb.velocity = (lookVector.normalized * retreatSpeed) * -1;
            }
            // This should stop the boss from constantly moving back
            else if (Time.fixedTime > timeTillRetreatEnds)
            {
                rb.velocity = Vector2.zero;
                //cooldown = Time.fixedTime + cooldownTime;

                // Once the retreat has been completed, we wait for a cooldown period
                // After the character comes to a stop, I'll let the animation that will be played define the amount
                //  the boss waits before continuing its assault
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.updateMode = AnimatorUpdateMode.Normal;
    }

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (rb == null) rb = animator.GetComponent<Rigidbody2D>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        // Debug.Log(rb);
        // Debug.Log(player);

        // rb.velocity = Vector2.zero;
        if (Vector2.Distance(player.position, rb.position) <= approachRange)
        {
            Vector2 target = new Vector2(player.position.x, player.position.y);
            Vector2 lookVector = target - (Vector2)animator.transform.position;
            DetermineAnimationDirection(lookVector);

            // temporary fix to make sure the character is running away from the player
            //  and not running forwards, but moving backwards
            switch (animationDirection)
            {
                case 1:
                    animationDirection = 2;
                    break;
                case 2:
                    animationDirection = 1;
                    break;
                case 3:
                    animationDirection = 4;
                    break;
                case 4:
                    animationDirection = 3;
                    break;
            }

            animator.SetInteger("Direction", animationDirection);
            animator.SetBool("Neutral", false);
        }
        else
        {
            animator.SetBool("Neutral", true);
        }
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        //animator.ResetTrigger("Neutral");

        Vector2 target = new Vector2(player.position.x, player.position.y);
        Vector2 lookVector = target - (Vector2)animator.transform.position;
        DetermineAnimationDirection(lookVector);
        animator.SetInteger("Direction", animationDirection);

        rb.velocity = Vector2.zero;
    }

    // Could have made a class deriving from StateMachineBehaviour with this method 
    //  and made sure that the all the SMB scripts implement this method.
    void DetermineAnimationDirection(Vector2 look)
    {
        bool rangeX = false;
        bool rangeY = false;

        float xlook = look.normalized.x;
        float ylook = look.normalized.y;

        //Debug.Log(new Vector2(xlook, ylook));

        rangeX = (xlook > -0.7f) && (xlook < 0.7f); //can do 0.71 or 0.707 for more precision
        rangeY = (ylook > -0.7f) && (ylook < 0.7f);

        if (ylook > 0 && rangeX)
        {
            animationDirection = 1;
        }

        if (ylook < 0 && rangeX)
        {
            animationDirection = 2;
        }

        if (xlook > 0 && rangeY)
        {
            animationDirection = 3;
        }

        if (xlook < 0 && rangeY)
        {
            animationDirection = 4;
        }
    }
}
