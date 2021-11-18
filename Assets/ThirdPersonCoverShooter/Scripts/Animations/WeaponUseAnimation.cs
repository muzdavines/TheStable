using UnityEngine;

namespace CoverShooter
{
    public class WeaponUseAnimation : StateMachineBehaviour
    {
        /// <summary>
        /// Moment inside the animation when the weapon is used.
        /// </summary>
        [Tooltip("Moment inside the animation when the weapon is used.")]
        [Range(0, 1)]
        public float Moment = 0.5f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].internalIsToolAnimation = true;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].internalIsToolAnimation = false;
            CharacterMotor.animatorToMotorMap[animator].InputUseWeapon();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.normalizedTime >= Moment)
                CharacterMotor.animatorToMotorMap[animator].InputUseWeapon();
        }
    }
}
