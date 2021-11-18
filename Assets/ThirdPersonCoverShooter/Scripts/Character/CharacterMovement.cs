using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Movement information of a character.
    /// </summary>
    public struct CharacterMovement
    {
        /// <summary>
        /// Direction multiplier by Magnitude.
        /// </summary>
        public Vector3 Value { get { return Direction * Magnitude; } }

        /// <summary>
        /// Is there any movement.
        /// </summary>
        public bool IsMoving { get { return Value.sqrMagnitude > 0.1f; } }

        /// <summary>
        /// Is Magnitude great enough for the character to be running, but not sprinting.
        /// </summary>
        public bool IsRunning { get { return Magnitude > 0.6f & IsMoving && !IsSprinting; } }

        /// <summary>
        /// Is Magnitude great enough for the character to be sprinting.
        /// </summary>
        public bool IsSprinting { get { return Magnitude > 1.1f && IsMoving; } }

        /// <summary>
        /// Direction to move to in world space.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Speed of movement. 1 is running, 0.5 is walking.
        /// </summary>
        public float Magnitude;

        /// <summary>
        /// Minimum duration the input and subsequent inputs are to be held.
        /// </summary>
        public float MinDuration;

        /// <summary>
        /// Is the given movement slowed down by any reason. Value used to tell the motor that it's walking because of the aim mode.
        /// </summary>
        public bool IsSlowedDown;

        /// <summary>
        /// Creates a character movement description.
        /// </summary>
        public CharacterMovement(Vector3 direction, float magnitude, float minDuration = 0, bool isSlowedDown = false)
        {
            Direction = direction;
            Magnitude = magnitude;
            MinDuration = minDuration;
            IsSlowedDown = isSlowedDown;
        }
    }
}