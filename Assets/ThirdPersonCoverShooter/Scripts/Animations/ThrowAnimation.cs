using UnityEngine;

namespace CoverShooter
{
    public class ThrowAnimation : StateMachineBehaviour
    {
        /// <summary>
        /// Moment inside the animation when the weapon is used.
        /// </summary>
        [Tooltip("Moment inside the animation when the weapon is used.")]
        [Range(0, 1)]
        public float Moment = 0.5f;

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputThrow();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.normalizedTime >= Moment)
                CharacterMotor.animatorToMotorMap[animator].InputThrow();
        }
    }
}
