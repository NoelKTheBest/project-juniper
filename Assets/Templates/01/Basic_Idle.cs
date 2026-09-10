using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Character))]
public class Basic_Idle : StateMachineBehaviour
{
    Transform target;
    Rigidbody2D rb;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

        target = animator.GetComponent<Character>().target;
        rb = animator.GetComponent<Rigidbody2D>();

        if (target != null)
        {
            Vector2 targetVector = new Vector2(target.position.x, target.position.y);
            Vector2 lookVector = targetVector - (Vector2)animator.transform.position;
            animator.SetBool("isMoving", true);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.updateMode = AnimatorUpdateMode.Normal;
    }
}
