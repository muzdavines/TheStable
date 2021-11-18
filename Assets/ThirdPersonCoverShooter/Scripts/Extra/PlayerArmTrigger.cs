using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Causes player characters to pick a weapon when they enter the trigger area.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerArmTrigger : MonoBehaviour
    {
        /// <summary>
        /// Weapon to arm. Index starts at one. Value of 0 means none. 
        /// </summary>
        [Tooltip("Weapon to arm. Index starts at one. Value of 0 means none. ")]
        public int WeaponToUse = 1;

        /// <summary>
        /// Trigger is ignored if the player already has a weapon. When UseForce is enabled the player always switches to the new weapon.
        /// </summary>
        [Tooltip("Trigger is ignored if the player already has a weapon. When UseForce is enabled the player always switches to the new weapon.")]
        public bool UseForce = false;

        private void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null) return;

            var inventory = other.GetComponent<CharacterInventory>();
            if (inventory == null) return;

            if (WeaponToUse > 0 && WeaponToUse <= inventory.Weapons.Length)
            {
                motor.Weapon = inventory.Weapons[WeaponToUse - 1];
                motor.IsEquipped = true;
            }
            else
                motor.IsEquipped = false;
        }
    }
}
