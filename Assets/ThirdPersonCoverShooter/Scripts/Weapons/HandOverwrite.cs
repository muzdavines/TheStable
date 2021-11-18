using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Defines hand position markers to overwrite the default marker.
    /// </summary>
    [Serializable]
    public struct HandOverwrite
    {
        /// <summary>
        /// Marker to use when the character is aiming.
        /// </summary>
        [Tooltip("Marker to use when the character is aiming.")]
        public Transform Aim;

        /// <summary>
        /// Marker to use when the character is crouching.
        /// </summary>
        [Tooltip("Marker to use when the character is crouching.")]
        public Transform Crouch;

        /// <summary>
        /// Marker to use when the character is standing in a low cover facing right.
        /// </summary>
        [Tooltip("Marker to use when the character is standing in a low cover.")]
        public Transform LowCover;

        /// <summary>
        /// Marker to use when the character is standing in a tall cover facing right.
        /// </summary>
        [Tooltip("Marker to use when the character is standing in a tall cover.")]
        public Transform TallCover;
    }
}
