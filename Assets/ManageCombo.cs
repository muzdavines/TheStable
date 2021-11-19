using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageCombo : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        int comboNum = animator.GetInteger("ComboNum");
        comboNum++;
        animator.SetInteger("ComboNum", comboNum);
        Move nextMove = null;
        try { nextMove = animator.GetComponent<MissionCharacter>().character.activeMoves[comboNum]; } catch { Debug.Log(animator.transform.name + " ERROR: Combo Num " + comboNum); }


        if (nextMove !=null && comboNum < 3 && nextMove.moveType > 0) {
            Debug.Log(animator.transform.name + " Setting Trigger");
            animator.SetTrigger("ComboHit");
        }
        else { animator.SetTrigger("EndMelee"); Debug.Log(animator.transform.name + " ComboNum Triggering Exit: " + comboNum);}
    }
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        int comboNum = animator.GetInteger("ComboNum");
        Move nextMove = animator.GetComponent<MissionCharacter>().character.activeMoves[comboNum];

        if (nextMove != null && comboNum < 3 && nextMove.moveType > 0) {
            Debug.Log(animator.transform.name + " Setting Trigger");
            animator.SetTrigger("ComboHit");
        }
        else { Debug.Log("No Trigger " + comboNum + "  "+animator.transform.name); }
    }
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
