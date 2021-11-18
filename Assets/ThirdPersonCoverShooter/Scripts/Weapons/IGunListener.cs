using UnityEngine;

namespace CoverShooter
{
    public interface IGunListener
    {
        /// <summary>
        /// Magazine eject event.
        /// </summary>
        void OnEject();

        /// <summary>
        /// Magazine rechamber event.
        /// </summary>
        void OnRechamber();

        /// <summary>
        /// Shotgun pump event.
        /// </summary>
        void OnPump();

        /// <summary>
        /// Bullet fire event.
        /// </summary>
        /// <param name="delay">Time to delay the creation of effects.</param>
        void OnFire(float delay);

        /// <summary>
        /// Empty fire event.
        /// </summary>
        void OnEmptyFire();

        /// <summary>
        /// Bullet load event.
        /// </summary>
        void OnBulletLoad();

        /// <summary>
        /// Event spawned all the bullets are loaded.
        /// </summary>
        void OnFullyLoaded();

        /// <summary>
        /// Event spawned when bullet loading starts.
        /// </summary>
        void OnBulletLoadStart();

        /// <summary>
        /// Event spawned before pumping.
        /// </summary>
        void OnPumpStart();

        /// <summary>
        /// Event spawned when magazine loading starts.
        /// </summary>
        void OnMagazineLoadStart();
    }
}