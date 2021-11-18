using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Recoil settings for a gun.
    /// </summary>
    [Serializable]
    public struct GunRecoilSettings
    {
        /// <summary>
        /// Degrees the aim is shifted up after firing the gun.
        /// </summary>
        [Tooltip("Degrees the aim is shifted up after firing the gun.")]
        public float Vertical;

        /// <summary>
        /// Degrees the aim is shifted to the right after firing the gun.
        /// </summary>
        [Tooltip("Degrees to the right the aim is shifted after firing the gun.")]
        public float Horizontal;

        /// <summary>
        /// How easy it is for the motor to recover after recoil. Multiplies their recovery rate.
        /// </summary>
        [Tooltip("How easy it is for the motor to recover after recoil. Multiplies their recovery rate.")]
        public float RecoveryRate;

        /// <summary>
        /// Intensity of the camera shake following a gun fire.
        /// </summary>
        [Tooltip("Intensity of the camera shake following a gun fire.")]
        public float ShakeIntensity;

        /// <summary>
        /// Duration of the camera shake following a gun fire.
        /// </summary>
        [Tooltip("Duration of the camera shake following a gun fire.")]
        public float ShakeTime;

        /// <summary>
        /// Default recoil settings.
        /// </summary>
        public static GunRecoilSettings Default()
        {
            var settings = new GunRecoilSettings();
            settings.Vertical = 1.5f;
            settings.Horizontal = 0.25f;
            settings.RecoveryRate = 1;
            settings.ShakeIntensity = 1;
            settings.ShakeTime = 0.25f;

            return settings;
        }
    }
}