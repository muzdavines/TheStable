using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterPhysicsListener
    {
        /// <summary>
        /// Character lands on the ground.
        /// </summary>
        void OnLand();

        /// <summary>
        /// Character makes a footstep.
        /// </summary>
        void OnFootstep(Vector3 position);

        /// <summary>
        /// Character jumps off the ground.
        /// </summary>
        void OnJump();
    }
}