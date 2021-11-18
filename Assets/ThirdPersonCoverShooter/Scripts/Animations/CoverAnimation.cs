using UnityEngine;

namespace CoverShooter
{
    public class CoverAnimation : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].internalIsCoverAnimation = true;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].internalIsCoverAnimation = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            var motor = CharacterMotor.animatorToMotorMap[animator];

            if (motor.internalIsCoverAnimation || motor.GroundTimer < 0.4f)
                return;

            var cycle = Mathf.Repeat(animatorStateInfo.normalizedTime, 1);

            var x = animator.GetFloat("MovementX");
            var z = animator.GetFloat("MovementZ");

            const float e = 0.1f;

            bool isRightFirst;

            if (z > e)
                isRightFirst = true;
            else if (z > -e)
            {
                if (x > 0)
                    isRightFirst = true;
                else
                    isRightFirst = false;
            }
            else
                isRightFirst = false;

            if (animator.GetFloat("MovementSpeed") > 0.4f)
            {
                int location;

                if (cycle < 0.1f)
                    location = 0;
                else if (cycle < 0.6f)
                    location = 1;
                else
                    location = 2;

                if ((location == 1 && isRightFirst) || (location != 1 && !isRightFirst))
                    animator.SetFloat("JumpLeg", 1);
                else
                    animator.SetFloat("JumpLeg", -1);
            }
        }
    }
}
