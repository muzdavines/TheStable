using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes a zone with increased visibility. Increases view distance for the AI.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LightZone : Zone<LightZone>
    {
        /// <summary>
        /// Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.")]
        public VisibilityType Type = VisibilityType.multiplier;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.")]
        public float Value = 1;

        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("OnEnterLight", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            other.SendMessage("OnLeaveLight", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}