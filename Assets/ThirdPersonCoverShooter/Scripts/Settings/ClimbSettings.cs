using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Climbing settings for a character.
    /// </summary>
    [Serializable]
    public struct ClimbSettings
    {
        /// <summary>
        /// Capsule height to set during a climb.
        /// </summary>
        [Tooltip("Capsule height to set during a climb.")]
        [Range(0, 10)]
        public float CapsuleHeight;

        /// <summary>
        /// Scale of movement done by the climbing animation in Y axis. The animation usually assumed a 1 meter high obstacle.
        /// </summary>
        [Tooltip("Scale of movement in Y axis. The animation usually assumed a 1 meter high obstacle.")]
        [Range(0, 3)]
        public float VerticalScale;

        /// <summary>
        /// Scale of movement done by the climbing animation in X and Z axes.
        /// </summary>
        [Tooltip("Scale of movement done by the climbing animation in X and Z axes.")]
        [Range(0, 3)]
        public float HorizontalScale;

        /// <summary>
        /// Additional velocity added to the character in the direction of cover.
        /// </summary>
        [Tooltip("Additional velocity added to the character in the direction of cover.")]
        [Range(0, 5)]
        public float Push;

        /// <summary>
        /// Moment in the climbing animation to turn on the push force.
        /// </summary>
        [Tooltip("Moment in the climbing animation to turn on the push force.")]
        [Range(0, 1)]
        public float PushOn;

        /// <summary>
        /// Moment in the climbing animation to turn off the push force.
        /// </summary>
        [Tooltip("Moment in the climbing animation to turn off the push force.")]
        [Range(0, 1)]
        public float PushOff;

        /// <summary>
        /// Moment in the climbing animation to turn off the capsule collider.
        /// </summary>
        [Tooltip("Moment in the climbing animation to turn off the capsule collider.")]
        [Range(0, 1)]
        public float CollisionOff;

        /// <summary>
        /// Moment in the climbing animation to turn back on the capsule collider.
        /// </summary>
        [Tooltip("Moment in the climbing animation to turn back on the capsule collider.")]
        [Range(0, 1)]
        public float CollisionOn;

        /// <summary>
        /// Default character climbing settings.
        /// </summary>
        public static ClimbSettings Default()
        {
            var settings = new ClimbSettings();
            settings.CapsuleHeight = 1.5f;
            settings.VerticalScale = 1.0f;
            settings.HorizontalScale = 1.05f;
            settings.Push = 0.5f;
            settings.PushOn = 0.6f;
            settings.PushOff = 0.9f;
            settings.CollisionOff = 0.3f;
            settings.CollisionOn = 0.7f;

            return settings;
        }
    }

    /// <summary>
    /// Vault settings for a character.
    /// </summary>
    [Serializable]
    public struct VaultSettings
    {
        /// <summary>
        /// Capsule height to set during a vault.
        /// </summary>
        [Tooltip("Capsule height to set during a vault.")]
        [Range(0, 10)]
        public float CapsuleHeight;

        /// <summary>
        /// Scale of movement done by the vault animation in Y axis. The animation usually assumed a 1 meter high obstacle.
        /// </summary>
        [Tooltip("Scale of movement in Y axis. The animation usually assumed a 1 meter high obstacle.")]
        [Range(0, 3)]
        public float VerticalScale;

        /// <summary>
        /// Scale of movement done by the vault animation in X and Z axes.
        /// </summary>
        [Tooltip("Scale of movement done by the climbing animation in X and Z axes.")]
        [Range(0, 3)]
        public float HorizontalScale;

        /// <summary>
        /// Additional velocity added to the character in the direction of cover.
        /// </summary>
        [Tooltip("Additional velocity added to the character in the direction of cover.")]
        [Range(0, 5)]
        public float Push;

        /// <summary>
        /// Moment in the vault animation to turn on the push force.
        /// </summary>
        [Tooltip("Moment in the vault animation to turn on the push force.")]
        [Range(0, 1)]
        public float PushOn;

        /// <summary>
        /// Moment in the vault animation to turn off the push force.
        /// </summary>
        [Tooltip("Moment in the vault animation to turn off the push force.")]
        [Range(0, 1)]
        public float PushOff;

        /// <summary>
        /// Moment in the vault animation to turn off the capsule collider.
        /// </summary>
        [Tooltip("Moment in the climbing animation to turn off the capsule collider.")]
        [Range(0, 1)]
        public float CollisionOff;

        /// <summary>
        /// Moment in the vault animation to turn back on the capsule collider.
        /// </summary>
        [Tooltip("Moment in the climbing animation to turn back on the capsule collider.")]
        [Range(0, 1)]
        public float CollisionOn;

        /// <summary>
        /// Default character climbing settings.
        /// </summary>
        public static VaultSettings Default()
        {
            var settings = new VaultSettings();
            settings.CapsuleHeight = 1.5f;
            settings.VerticalScale = 1.3f;
            settings.HorizontalScale = 1.4f;
            settings.Push = 0.0f;
            settings.PushOn = 0.0f;
            settings.PushOff = 1.0f;
            settings.CollisionOff = 0.1f;
            settings.CollisionOn = 0.7f;

            return settings;
        }
    }
}