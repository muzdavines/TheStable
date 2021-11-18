using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes a zone where objects have less visibility (decreases view distance for the AI).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DarkZone : Zone<DarkZone>
    {
        /// <summary>
        /// Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.")]
        public VisibilityType Type = VisibilityType.constant;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used when the AI is not alerted. Can be either a distance or a multiplier for the AI view distance depending on the Type.")]
        public float DefaultValue = 4;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used when the AI knows about the threat. Can be either a distance or a multiplier for the AI view distance depending on the Type.")]
        public float AlertValue = 10;

        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("OnEnterDarkness", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            other.SendMessage("OnLeaveDarkness", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}