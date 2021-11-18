using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Relative positions for the camera.
    /// </summary>
    [Serializable]
    public struct ShoulderSettings
    {
        /// <summary>
        /// Relative right shoulder position when the character is standing up.
        /// </summary>
        [Tooltip("Relative right shoulder position when the character is standing up.")]
        public Vector3 Standing;

        /// <summary>
        /// Relative right shoulder position when the character is crouching.
        /// </summary>
        [Tooltip("Relative right shoulder position when the character is crouching.")]
        public Vector3 Crouching;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static ShoulderSettings Default()
        {
            var result = new ShoulderSettings();
            result.Standing = new Vector3(0, 1.4f, 0);
            result.Crouching = new Vector3(0, 0.9f, 0);

            return result;
        }
    }
}
