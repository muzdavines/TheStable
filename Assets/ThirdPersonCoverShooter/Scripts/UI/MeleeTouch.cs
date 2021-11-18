using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// When pressed makes the target motor attack with melee.
    /// </summary>
    public class MeleeTouch : PressButton, IPointerDownHandler
    {
        /// <summary>
        /// Character motor that is instructed to attack.
        /// </summary>
        [Tooltip("Character motor that is instructed to attack.")]
        public CharacterMotor Motor;

        protected override void OnPress()
        {
            if (Motor != null)
                Motor.InputMelee();
        }
    }
}