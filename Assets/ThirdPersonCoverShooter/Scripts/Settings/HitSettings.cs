using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Settings for the CharacterMotor IK when hit.
    /// </summary>
    [Serializable]
    public class HitResponseSettings
    {
        /// <summary>
        /// Minimum wait between hits in seconds.
        /// </summary>
        [Tooltip("Minimum wait between hits in seconds.")]
        public float Wait;

        /// <summary>
        /// Angles by which the bone is adjusted on hit.
        /// </summary>
        [Tooltip("Angles by which the bone is adjusted on hit.")]
        public float Strength;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static HitResponseSettings Default()
        {
            var result = new HitResponseSettings();
            result.Wait = 0.5f;
            result.Strength = 20;

            return result;
        }
    }
}
