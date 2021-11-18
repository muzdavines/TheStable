using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Speed and angle settings for CharacterMotor.
    /// </summary>
    [Serializable]
    public struct AimSettings
    {
        /// <summary>
        /// Maximum allowed angle between aim direction and body direction.
        /// </summary>
        [Tooltip("Maximum allowed angle between aim direction and body direction.")]
        public float MaxAimAngle;

        /// <summary>
        /// Accuracy error when walking.
        /// </summary>
        [Tooltip("Accuracy error when walking.")]
        public float WalkError;

        /// <summary>
        /// Accuracy error when running.
        /// </summary>
        [Tooltip("Accuracy error when running.")]
        public float RunError;

        /// <summary>
        /// Accuracy error when sprinting.
        /// </summary>
        [Tooltip("Accuracy error when sprinting.")]
        public float SprintError;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AimSettings Default()
        {
            var result = new AimSettings();
            result.MaxAimAngle = 60;
            result.WalkError = 2.5f;
            result.RunError = 4;
            result.SprintError = 6;

            return result;
        }
    }
}
