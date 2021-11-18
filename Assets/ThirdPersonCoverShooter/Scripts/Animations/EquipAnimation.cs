using UnityEngine;

namespace CoverShooter
{
    public class EquipAnimation : StateMachineBehaviour
    {
        /// <summary>
        /// Normalized time in the animation when to grab the weapon.
        /// </summary>
        [Tooltip("Normalized time in the animation when to grab the weapon.")]
        [Range(0, 1)]
        public float Grab = 0.5f;

        /// <summary>
        /// Normalized time in the animation when to stop the process.
        /// </summary>
        [Tooltip("Normalized time in the animation when to stop the process.")]
        [Range(0, 1)]
        public float End = 0.9f;

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            var motor = CharacterMotor.animatorToMotorMap[animator];

            motor.InputGrabWeapon();
            motor.InputFinishEquip();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            var motor = CharacterMotor.animatorToMotorMap[animator];

            if (animatorStateInfo.normalizedTime >= Grab)
                motor.InputGrabWeapon();

            if (animatorStateInfo.normalizedTime >= End)
                motor.InputFinishEquip();
        }
    }
}
