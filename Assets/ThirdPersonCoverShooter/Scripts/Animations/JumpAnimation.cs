using UnityEngine;

namespace CoverShooter
{
    public class JumpAnimation : StateMachineBehaviour
    {
        /// <summary>
        /// Moment during the animation when to end the process.
        /// </summary>
        [Tooltip("Moment during the animation when to end the process.")]
        [Range(0, 1)]
        public float End = 0.75f;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.normalizedTime >= End)
                CharacterMotor.animatorToMotorMap[animator].InputEndJump();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputEndJump();
        }
    }
}
