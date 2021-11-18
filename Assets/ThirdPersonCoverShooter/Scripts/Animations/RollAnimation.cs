using UnityEngine;

namespace CoverShooter
{
    public class RollAnimation : StateMachineBehaviour
    {
        /// <summary>
        /// Normalized time in the animation when to stop the process.
        /// </summary>
        [Tooltip("Normalized time in the animation when to stop the process.")]
        [Range(0, 1)]
        public float End = 0.8f;

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputEndRoll();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.normalizedTime >= End)
                CharacterMotor.animatorToMotorMap[animator].InputEndRoll();
        }
    }
}
