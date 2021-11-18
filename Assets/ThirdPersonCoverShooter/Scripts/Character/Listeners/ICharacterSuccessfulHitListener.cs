using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterSuccessfulHitListener
    {
        /// <summary>
        /// Character succeeds in hitting something (melee or bullet hit).
        /// </summary>
        void OnSuccessfulHit(Hit hit);
    }
}