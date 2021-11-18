using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Definition of an AI patrol waypoint.
    /// </summary>
    [Serializable]
    public struct Waypoint
    {
        /// <summary>
        /// Position of the waypoint.
        /// </summary>
        [Tooltip("Position of the waypoint.")]
        public Vector3 Position;

        /// <summary>
        /// Should the AI run towards the position.
        /// </summary>
        [Tooltip("Should the AI run towards the position.")]
        public bool Run;

        /// <summary>
        /// Duration in seconds of the pause after reaching the waypoint.
        /// </summary>
        [Tooltip("Duration in seconds of the pause after reaching the waypoint.")]
        public float Pause;
    }
}
