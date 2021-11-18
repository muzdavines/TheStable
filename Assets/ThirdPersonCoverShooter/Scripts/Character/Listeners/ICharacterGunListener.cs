using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterGunListener
    {
        /// <summary>
        /// Character starts firing.
        /// </summary>
        void OnStartGunFire();

        /// <summary>
        /// Character stops firing.
        /// </summary>
        void OnStopGunFire();
    }
}