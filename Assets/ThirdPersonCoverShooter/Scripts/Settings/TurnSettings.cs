using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Speed and angle settings for CharacterMotor.
    /// </summary>
    [Serializable]
    public struct TurnSettings
    {
        /// <summary>
        /// How quickly the character model is orientated towards the standing direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the standing direction.")]
        [Range(0, 50)]
        public float StandingRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards the running direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the running direction.")]
        [Range(0, 50)]
        public float RunningRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards a direction when in melee but not attacking.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards a direction when in melee but not attacking.")]
        [Range(0, 50)]
        public float MeleeIdleRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards a direction when in melee.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards a direction when in melee.")]
        [Range(0, 50)]
        public float MeleeAttackRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards the running direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the running direction.")]
        [Range(0, 50)]
        public float SprintingRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards the throw direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the throw direction.")]
        [Range(0, 50)]
        public float GrenadeRotationSpeed;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static TurnSettings Default()
        {
            var result = new TurnSettings();
            result.StandingRotationSpeed = 5;
            result.RunningRotationSpeed = 5;
            result.MeleeIdleRotationSpeed = 7;
            result.MeleeAttackRotationSpeed = 10;
            result.SprintingRotationSpeed = 20;
            result.GrenadeRotationSpeed = 20;

            return result;
        }
    }
}
