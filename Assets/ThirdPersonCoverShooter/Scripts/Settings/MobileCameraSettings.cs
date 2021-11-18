using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Min and max distances for enemy to be in position for camera to zoom out.
    /// </summary>
    [Serializable]
    public struct EnemyDistanceRange
    {
        /// <summary>
        /// Minimum distance for the enemy to be in zoom range.
        /// </summary>
        [Tooltip("Minimum distance for the enemy to be in zoom range.")]
        public float Min;

        /// <summary>
        /// Maximum distance for the enemy to be in zoom range.
        /// </summary>
        [Tooltip("Maximum distance for the enemy to be in zoom range.")]
        public float Max;

        /// <summary>
        /// Constructs the range.
        /// </summary>
        public EnemyDistanceRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Camera target offsets.
    /// </summary>
    [Serializable]
    public struct MobileCameraTargetOffsets
    {
        /// <summary>
        /// Target position relative to the player for camera to look at when the character is facing up.
        /// </summary>
        [Tooltip("Target position relative to the player for camera to look at when the character is facing up.")]
        public Vector3 Up;

        /// <summary>
        /// Target position relative to the player for camera to look at when the character is facing down.
        /// </summary>
        [Tooltip("Target position relative to the player for camera to look at when the character is facing down.")]
        public Vector3 Down;

        /// <summary>
        /// Target position relative to the player for camera to look at when the character is facing left.
        /// </summary>
        [Tooltip("Target position relative to the player for camera to look at when the character is facing left.")]
        public Vector3 Left;

        /// <summary>
        /// Target position relative to the player for camera to look at when the character is facing right.
        /// </summary>
        [Tooltip("Target position relative to the player for camera to look at when the character is facing right.")]
        public Vector3 Right;

        public MobileCameraTargetOffsets(Vector3 up, Vector3 down, Vector3 left, Vector3 right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }
    }
}
