using UnityEngine;

namespace CoverShooter
{
    public class CoverOffsetAnimation : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputMidCoverOffset(animatorStateInfo.normalizedTime);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputMidCoverOffset(1);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputMidCoverOffset(animatorStateInfo.normalizedTime);
        }
    }
}
