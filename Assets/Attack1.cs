using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack1 : StateMachineBehaviour
{

    PlayerController player;


    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    //    animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
    //    Debug.Log(Time.time);
       Debug.Log("OnStateEnter: " + animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
       Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    //    Debug.Log(Time.time);
    //    animator.updateMode = AnimatorUpdateMode.Normal;
       Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
       if (player == null) player = animator.GetComponent<PlayerController>();
    //    Debug.Log(player.openWindow);
    //    Debug.Log("OnStateMachineEnter: " + animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
       Debug.Log(animator.GetNextAnimatorStateInfo(0).normalizedTime);
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
       Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }
}
