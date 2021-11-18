using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes the type of the tool, used by the CharacterMotor and AI.
    /// </summary>
    public enum ToolType
    {
        none,
        radio,
        phone,
        flashlight
    }

    /// <summary>
    /// Tools are like Gun or Melee attached to a weapon. Their existance tells CharacterMotor that the weapon can be used as a tool.
    /// </summary>
    public abstract class Tool : MonoBehaviour
    {
        /// <summary>
        /// Signifies if a character needs to aim while using the tool.
        /// </summary>
        [HideInInspector]
        public bool HasAiming;

        /// <summary>
        /// Is the use animation continuous instead of a single action.
        /// </summary>
        [HideInInspector]
        public bool IsContinuous;

        /// <summary>
        /// Signifies if a character needs to aim while using the tool in alternate mode.
        /// </summary>
        [HideInInspector]
        public bool HasAlternateAiming;

        /// <summary>
        /// Is the alternate use animation continuous instead of a single action.
        /// </summary>
        [HideInInspector]
        public bool IsAlternateContinuous;

        /// <summary>
        /// Method used every frame on the tool during the use animation.
        /// </summary>
        public virtual void ContinuousUse(CharacterMotor character, bool isAlternate)
        {
        }

        /// <summary>
        /// Method called during a specific moment in the use animation.
        /// </summary>
        public virtual void Use(CharacterMotor character, bool isAlternate)
        {
        }
    }
}
