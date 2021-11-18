using UnityEngine;

namespace CoverShooter
{
    public class UnequipAnimation : StateMachineBehaviour
    {
        /// <summary>
        /// Normalized time in the animation when to put the weapon away.
        /// </summary>
        [Tooltip("Normalized time in the animation when to put the weapon away.")]
        [Range(0, 1)]
        public float Put = 0.6f;

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].InputUnequip();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.normalizedTime >= Put)
                CharacterMotor.animatorToMotorMap[animator].InputUnequip();
        }
    }
}
