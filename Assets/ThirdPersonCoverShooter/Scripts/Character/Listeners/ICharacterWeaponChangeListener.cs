using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterWeaponChangeListener
    {
        /// <summary>
        /// Character begins switching weapons.
        /// </summary>
        void OnWeaponChangeStart();

        /// <summary>
        /// Character finishes equipping a weapon.
        /// </summary>
        void OnWeaponChange();
    }
}