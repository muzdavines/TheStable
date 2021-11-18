using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Character roll settings.
    /// </summary>
    [Serializable]
    public struct RollSettings
    {
        /// <summary>
        /// Character's capsule height during a roll.
        /// </summary>
        [Tooltip("Character's capsule height during a roll.")]
        [Range(0, 10)]
        public float CapsuleHeight;

        /// <summary>
        /// How fast the character turns towards the roll direction.
        /// </summary>
        [Tooltip("How fast the character turns towards the roll direction.")]
        public float RotationSpeed;

        /// <summary>
        /// Default jump settings.
        /// </summary>
        public static RollSettings Default()
        {
            RollSettings settings;
            settings.CapsuleHeight = 1.0f;
            settings.RotationSpeed = 10;

            return settings;
        }
    }
}