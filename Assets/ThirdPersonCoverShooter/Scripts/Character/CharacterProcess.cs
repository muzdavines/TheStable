using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Custom motor process description.
    /// </summary>
    public struct CharacterProcess
    {
        /// <summary>
        /// Animator trigger to set on the start of the process.
        /// </summary>
        [Tooltip("Animator trigger to set on the start of the process.")]
        public string AnimationTrigger;

        /// <summary>
        /// Can the motor aim and fire during the process.
        /// </summary>
        [Tooltip("Can the motor aim and fire during the process.")]
        public bool CanAim;

        /// <summary>
        /// Can the motor move during the process.
        /// </summary>
        [Tooltip("Can the motor move during the process.")]
        public bool CanMove;

        /// <summary>
        /// Should the motor leave cover during the process.
        /// </summary>
        [Tooltip("Should the motor leave cover during the process.")]
        public bool LeaveCover;

        public CharacterProcess(string animationTrigger, bool canAim, bool canMove, bool leaveCover)
        {
            AnimationTrigger = animationTrigger;
            CanAim = canAim;
            CanMove = canMove;
            LeaveCover = leaveCover;
        }
    }
}
