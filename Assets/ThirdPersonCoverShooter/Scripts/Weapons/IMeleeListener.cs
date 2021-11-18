using UnityEngine;

namespace CoverShooter
{
    public interface IMeleeListener
    {
        /// <summary>
        /// Melee attack event.
        /// </summary>
        void OnMeleeAttack();

        /// <summary>
        /// Successful melee hit event.
        /// </summary>
        void OnMeleeHit();

        /// <summary>
        /// Animation-defined melee event.
        /// </summary>
        void OnMeleeMoment();
    }
}