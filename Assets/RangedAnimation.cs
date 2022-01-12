using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoverShooter;
public class RangedAnimation : StateMachineBehaviour {
    public Move thisMove;
    [Tooltip("Normalized time in the animation when to stop the attack chain.")]
    [Range(0, 1)]
    float End = 0.9f;
    public int thisComboNum;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        Debug.Log("Entering Ranged Anim");
        var motor = CharacterMotor.animatorToMotorMap[animator];
        motor._isInRangedAnim = true;
        thisMove = motor.GetComponent<MissionCharacter>().activeMoves[thisComboNum];
        animator.SetFloat("Combo", (float)motor.GetComponent<MissionCharacter>().character.activeMeleeMoves[thisComboNum].moveType);
        End = .9f;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        var motor = CharacterMotor.animatorToMotorMap[animator];
        motor._isInRangedAnim = false;
        motor.RangedAnimationFinished();
    }
}
