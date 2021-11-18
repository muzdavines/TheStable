using UnityEngine;

namespace CoverShooter
{
    public enum ReloadType
    {
        Magazine,
        Bullet,
        Pump
    }

    public class ReloadAnimation : StateMachineBehaviour
    {
        /// <summary>
        /// Type of the reload process.
        /// </summary>
        [Tooltip("Type of the reload process.")]
        public ReloadType Type;

        /// <summary>
        /// Normalized time in the animation when to stop the process.
        /// </summary>
        [Tooltip("Normalized time in the animation when to stop the process.")]
        [Range(0, 1)]
        public float End = 0.9f;

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            var motor = CharacterMotor.animatorToMotorMap[animator];

            switch (Type)
            {
                case ReloadType.Bullet: motor.InputEndBulletLoad(); break;
                case ReloadType.Magazine: motor.InputEndMagazineLoad(); break;
                case ReloadType.Pump: motor.InputEndPump(); break;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.normalizedTime >= End)
            {
                var motor = CharacterMotor.animatorToMotorMap[animator];

                switch (Type)
                {
                    case ReloadType.Bullet: motor.InputEndBulletLoad(); break;
                    case ReloadType.Magazine: motor.InputEndMagazineLoad(); break;
                    case ReloadType.Pump: motor.InputEndPump(); break;
                }
            }
        }
    }
}
