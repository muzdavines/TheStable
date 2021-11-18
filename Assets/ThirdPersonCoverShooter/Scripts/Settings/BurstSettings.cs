using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Settings for bursts of fire.
    /// </summary>
    [Serializable]
    public struct Bursts
    {
        /// <summary>
        /// Time in seconds to wait for another burst of fire.
        /// </summary>
        [Tooltip("Time in seconds to wait for another burst of fire.")]
        public float Wait;

        /// <summary>
        /// Duration in seconds of a burst of fire.
        /// </summary>
        [Tooltip("Duration in seconds of a burst of fire.")]
        public float Duration;

        /// <summary>
        /// Total duration of the whole cycle.
        /// </summary>
        public float CycleDuration { get { return Wait + Duration; } }

        /// <summary>
        /// Default settings.
        /// </summary>
        public static Bursts Default()
        {
            var result = new Bursts();
            result.Wait = 0.25f;
            result.Duration = 1.0f;

            return result;
        }
    }

    /// <summary>
    /// Settings for bursts of fire when in cover.
    /// </summary>
    [Serializable]
    public struct CoverBursts
    {
        /// <summary>
        /// Time in seconds to wait in cover for another burst of fire.
        /// </summary>
        [Tooltip("Time in seconds to wait in cover for another burst of fire.")]
        public float Wait;

        /// <summary>
        /// Duration in seconds of a burst.
        /// </summary>
        [Tooltip("Duration in seconds of a burst.")]
        public float Duration;

        /// <summary>
        /// Time in seconds for AI to stand without firing before a burst.
        /// </summary>
        [Tooltip("Time in seconds for AI to stand without firing before a burst.")]
        public float IntroDuration;

        /// <summary>
        /// Time in seconds for AI to stand without firing after a burst.
        /// </summary>
        [Tooltip("Time in seconds for AI to stand without firing after a burst.")]
        public float OutroDuration;

        /// <summary>
        /// Total duration of the whole cycle.
        /// </summary>
        public float CycleDuration { get { return Wait + Duration + IntroDuration + OutroDuration; } }

        /// <summary>
        /// Total duration of peeking.
        /// </summary>
        public float PeekDuration { get { return Duration + IntroDuration + OutroDuration; } }

        /// <summary>
        /// Default settings.
        /// </summary>
        public static CoverBursts Default()
        {
            var result = new CoverBursts();
            result.Wait = 1.0f;
            result.Duration = 0.9f;
            result.IntroDuration = 0.35f;
            result.OutroDuration = 0.35f;

            return result;
        }
    }
}
