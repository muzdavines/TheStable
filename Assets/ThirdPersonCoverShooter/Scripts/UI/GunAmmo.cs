using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Maintains information about the gun and it's ammo.
    /// </summary>
    [RequireComponent(typeof(Text))]
    [ExecuteInEditMode]
    public class GunAmmo : MonoBehaviour
    {
        /// <summary>
        /// Character whose gun information is displayed.
        /// </summary>
        [Tooltip("")]
        public CharacterMotor Motor;

        /// <summary>
        /// Determines if the display is hidden when the motor is dead.
        /// </summary>
        [Tooltip("Determines if the display is hidden when the motor is dead.")]
        public bool HideWhenDead = true;

        /// <summary>
        /// Determines if the display is hidden when there is no gun.
        /// </summary>
        [Tooltip("Determines if the display is hidden when there is no gun.")]
        public bool HideWhenNone = false;

        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void LateUpdate()
        {
            if (Motor == null)
                return;

            var gun = Motor.Weapon.Gun;
            if (!Motor.IsEquipped) gun = null;

            if (gun != null)
                _text.text = gun.Name + " " + gun.LoadedBulletsLeft.ToString();

            if (Application.isPlaying)
            {
                var isVisible = true;

                if (Motor == null)
                    isVisible = !HideWhenNone;
                else
                    isVisible = (Motor.IsAlive || !HideWhenDead) && (gun != null || !HideWhenNone);

                _text.enabled = isVisible;
            }
        }
    }
}