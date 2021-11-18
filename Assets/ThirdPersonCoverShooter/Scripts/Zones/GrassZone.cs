using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes an area with grass. Adjusts the AI visiblity when looking at objects inside it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GrassZone : Zone<GrassZone>
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
        public float DefaultValue = 1;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used when the AI knows about the threat. Can be either a distance or a multiplier for the AI view distance depending on the Type.")]
        public float AlertValue = 6;

        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("OnEnterGrass", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            other.SendMessage("OnLeaveGrass", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}