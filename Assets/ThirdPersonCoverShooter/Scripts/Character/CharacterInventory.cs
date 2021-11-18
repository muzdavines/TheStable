using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterInventory : MonoBehaviour
    {
        /// <summary>
        /// All the weapons belonging in the inventory.
        /// </summary>
        [Tooltip("All the weapons belonging in the inventory.")]
        public WeaponDescription[] Weapons;

        private void Awake()
        {
            var motor = GetComponent<CharacterMotor>();

            for (int i = 0; i < Weapons.Length; i++)
            {
                var weapon = Weapons[i];

                if (weapon.RightItem != null && (!motor.IsEquipped || motor.Weapon.RightItem != weapon.RightItem)) weapon.RightItem.SetActive(false);
                if (weapon.RightHolster != null && (!motor.IsEquipped || motor.Weapon.RightHolster != weapon.RightHolster)) weapon.RightHolster.SetActive(true);

                if (weapon.LeftItem != null && (!motor.IsEquipped || motor.Weapon.LeftItem != weapon.LeftItem)) weapon.LeftItem.SetActive(false);
                if (weapon.LeftHolster != null && (!motor.IsEquipped || motor.Weapon.LeftHolster != weapon.LeftHolster)) weapon.LeftHolster.SetActive(true);

                if (weapon.Shield != null && (!motor.IsEquipped || motor.Weapon.Shield != weapon.Shield)) weapon.Shield.SetActive(false);
            }
        }
    }
}
