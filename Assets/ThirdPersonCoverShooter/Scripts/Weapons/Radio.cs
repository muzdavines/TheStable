using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes the object as a radio.
    /// </summary>
    public class Radio : Tool
    {
        public Radio()
        {
            HasAiming = false;
            IsContinuous = false;
            HasAlternateAiming = false;
            IsAlternateContinuous = false;
        }

        /// <summary>
        /// The call has been made.
        /// </summary>
        public override void Use(CharacterMotor character, bool isAlternate)
        {
            if (!isAlternate)
                character.SendMessage("OnCallMade", SendMessageOptions.DontRequireReceiver);
        }
    }
}
