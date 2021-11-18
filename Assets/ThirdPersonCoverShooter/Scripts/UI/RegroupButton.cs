using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Stops all actions and tells the characters to move back to default positions.
    /// </summary>
    public class RegroupButton : PressButton
    {
        /// <summary>
        /// Character switcher whose active character is managed.
        /// </summary>
        [Tooltip("Character switcher whose active character is managed.")]
        public CharacterSwitcher Switcher;

        protected override void OnPress()
        {
            if (Switcher == null)
                return;

            for (int i = 0; i < Switcher.Characters.Length; i++)
            {
                var motor = Switcher.Characters[i];

                if (motor == null)
                    continue;

                if (i != Switcher.Active)
                {
                    motor.SendMessage("ToStopActions", SendMessageOptions.DontRequireReceiver);
                    motor.SendMessage("ToRegroupFormation");
                }
            }
        }
    }
}
