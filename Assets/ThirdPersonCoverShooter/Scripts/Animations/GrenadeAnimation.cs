using UnityEngine;

namespace CoverShooter
{
    public class GrenadeAnimation : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].internalIsGrenadeAnimation = true;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].internalIsGrenadeAnimation = false;
        }
    }
}
