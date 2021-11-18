using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes the object as a phone.
    /// </summary>
    public class Phone : Tool
    {
        public Phone()
        {
            HasAiming = false;
            IsContinuous = false;
            HasAlternateAiming = true;
            IsAlternateContinuous = true;
        }

        /// <summary>
        /// The call has been made.
        /// </summary>
        public override void Use(CharacterMotor character, bool isAlternate)
        {
            if (isAlternate)
                character.SendMessage("OnCallMade", SendMessageOptions.DontRequireReceiver);
        }
    }
}
