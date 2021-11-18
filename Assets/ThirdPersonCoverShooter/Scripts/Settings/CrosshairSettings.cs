using System;
using UnityEngine;

namespace CoverShooter
{
    [Serializable]
    public struct CrosshairSettings
    {
        /// <summary>
        /// Sprites to be used when drawing the crosshair. Used according to min and max aim angle.
        /// </summary>
        [Tooltip("Sprites to be used when drawing the crosshair. Used according to min and max aim angle.")]
        public Sprite[] Sprites;

        /// <summary>
        /// Scale of the drawn sprite.
        /// </summary>
        [Tooltip("Scale of the drawn sprite.")]
        public float Scale;

        /// <summary>
        /// Aim angle at which the first sprite is drawn.
        /// </summary>
        [Tooltip("Aim angle at which the first sprite is drawn.")]
        public float LowAngle;

        /// <summary>
        /// Aim angle at which the last sprite is drawn.
        /// </summary>
        [Tooltip("Aim angle at which the last sprite is drawn.")]
        public float HighAngle;

        /// <summary>
        /// Speed at which the crosshair adapts to new sizes.
        /// </summary>
        [Tooltip("Speed at which the crosshair adapts to new sizes.")]
        public float Adaptation;

        /// <summary>
        /// How much to multiply the angle offset produced by camera shaking.
        /// </summary>
        [Tooltip("How much to multiply the angle offset produced by camera shaking.")]
        public float ShakeMultiplier;

        /// <summary>
        /// How much to multiply the angle offset produced by recoil.
        /// </summary>
        [Tooltip("How much to multiply the angle offset produced by recoil.")]
        public float RecoilMultiplier;

        public static CrosshairSettings Default()
        {
            CrosshairSettings settings;
            settings.Sprites = null;
            settings.Scale = 1;
            settings.LowAngle = 1.5f;
            settings.HighAngle = 10;
            settings.Adaptation = 2;
            settings.ShakeMultiplier = 2;
            settings.RecoilMultiplier = 1;

            return settings;
        }
    }
}
