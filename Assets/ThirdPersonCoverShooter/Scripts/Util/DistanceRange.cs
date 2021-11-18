using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Describes a value that is calculated based on a distance.
    /// </summary>
    [Serializable]
    public struct DistanceRange
    {
        /// <summary>
        /// Minimum interpolated value.
        /// </summary>
        [Tooltip("Minimum interpolated value.")]
        public float Min;

        /// <summary>
        /// Maximum interpolated value. 
        /// </summary>
        [Tooltip("Maximum interpolated value. ")]
        public float Max;

        /// <summary>
        /// Value is set to MinValue when distance to camera is lower than this value.
        /// </summary>
        [Tooltip("Delay is set to MinDelay when distance to camera is lower than this value.")]
        public float MinDistance;

        /// <summary>
        /// Value is set to MaxValue when distance to camera is greater than this value.
        /// </summary>
        [Tooltip("Delay is set to MaxDelay when distance to camera is greater than this value.")]
        public float MaxDistance;

        /// <summary>
        /// Constructs a new distance range.
        /// </summary>
        public DistanceRange(float min, float max, float minDistance, float maxDistance)
        {
            Min = min;
            Max = max;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        /// <summary>
        /// Returns value based on the given position. The distance is calculated between the main camera and the given position.
        /// </summary>
        public float Get(Vector3 position)
        {
            if (CameraManager.Main == null || CameraManager.Main.transform == null)
                return Min;
            else
                return Get(Vector3.Distance(CameraManager.Main.transform.position, position));
        }

        /// <summary>
        /// Returns value based on the given distance.
        /// </summary>
        public float Get(float distance)
        {
            float t = Mathf.Clamp01((distance - MinDistance) / (MaxDistance - MinDistance));
            return Mathf.Lerp(Min, Max, t);
        }
    }
}
