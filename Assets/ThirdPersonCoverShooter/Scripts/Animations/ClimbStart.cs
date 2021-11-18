using UnityEngine;

namespace CoverShooter
{
    public class ClimbStart : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputClimbStart();
        }
    }
}
