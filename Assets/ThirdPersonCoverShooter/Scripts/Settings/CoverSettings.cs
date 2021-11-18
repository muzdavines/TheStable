using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Defines cover settings for a character.
    /// </summary>
    [Serializable]
    public struct CoverSettings
    {
        /// <summary>
        /// Can the character peek from tall cover corners to aim.
        /// </summary>
        [Tooltip("Can the character peek from tall cover corners to aim.")]
        public bool CanUseTallCorners;

        /// <summary>
        /// Can the character peek from low cover corners to aim.
        /// </summary>
        [Tooltip("Can the character peek from low cover corners to aim.")]
        public bool CanUseLowCorners;

        /// <summary>
        /// Back area of a cover in angles, defined as a circle. Moving in this direction exits the cover.
        /// </summary>
        [Tooltip("Back area of a cover in angles, defined as a circle. Moving in this direction exits the cover.")]
        [Range(0, 360)]
        public float ExitBack;

        /// <summary>
        /// Height of character's collision capsule when idle in low cover.
        /// </summary>
        [Tooltip("Height of character's collision capsule when idle in low cover.")]
        [Range(0, 10)]
        public float LowCapsuleHeight;

        /// <summary>
        /// Height of character's collision capsule when aiming back in low cover.
        /// </summary>
        [Tooltip("Height of character's collision capsule when aiming back in low cover.")]
        [Range(0, 10)]
        public float LowAimCapsuleHeight;

        /// <summary>
        /// How quickly the character is orientated towards a direction in a low cover.
        /// </summary>
        [Tooltip("How quickly the character is orientated towards a direction in a low cover.")]
        [Range(0, 50)]
        public float LowRotationSpeed;

        /// <summary>
        /// How quickly the character is orientated towards a direction in a tall cover.
        /// </summary>
        [Tooltip("How quickly the character is orientated towards a direction in a tall cover.")]
        [Range(0, 50)]
        public float TallRotationSpeed;

        /// <summary>
        /// Character enter cover if it is closer than this value. Defined as a distance between a cover and an edge of players capsule.
        /// </summary>
        [Tooltip("Character enter cover if it is closer than this value. Defined as a distance between a cover and an edge of players capsule.")]
        [Range(0, 10)]
        public float EnterDistance;

        /// <summary>
        /// Character exit cover if it is furhter away than this value. Defined as a distance between a cover and an edge of players capsule.
        /// </summary>
        [Tooltip("Character exit cover if it is furhter away than this value. Defined as a distance between a cover and an edge of players capsule.")]
        [Range(0, 10)]
        public float LeaveDistance;

        /// <summary>
        /// Character exit cover if it is furhter away than this value. Defined as a distance between a cover and an edge of players capsule.
        /// </summary>
        [Tooltip("Distance to cover at which the player can prompt a climb or vault action.")]
        [Range(0, 10)]
        public float ClimbDistance;

        /// <summary>
        /// Distance to cover at which the character is fully crouching when near it.
        /// </summary>
        [Tooltip("Distance to cover at which the character is fully crouching when near it.")]
        [Range(0, 10)]
        public float MinCrouchDistance;

        /// <summary>
        /// Distance to cover at which the character begins to transition into the crouch state.
        /// </summary>
        [Tooltip("Distance to cover at which the character begins to transition into the crouch state.")]
        [Range(0, 10)]
        public float MaxCrouchDistance;

        /// <summary>
        /// Controls the location of camera pivot position when in cover. Pivot point does not move beyond this margin.
        /// </summary>
        [Tooltip("Controls the location of camera pivot position when in cover. Pivot point does not move beyond this margin.")]
        [Range(-10, 10)]
        public float PivotSideMargin;

        /// <summary>
        /// Distance from a side of a cover at which player can enter aiming from a corner.
        /// </summary>
        [Tooltip("Distance from a side of a cover at which player can enter aiming from a corner.")]
        [Range(0, 10)]
        public float CornerAimTriggerDistance;

        /// <summary>
        /// Capsule radius used when determining if the character is in front of a tall cover when entering it.
        /// </summary>
        [Tooltip("Capsule radius used when determining if the character is in front of a tall cover when entering it.")]
        [Range(0, 10)]
        public float TallSideEnterRadius;

        /// <summary>
        /// Capsule radius used when determining if the character is in front of a tall cover when entering it.
        /// </summary>
        [Tooltip("Capsule radius used when determining if the character is in front of a tall cover when leaving it.")]
        [Range(0, 10)]
        public float TallSideLeaveRadius;

        /// <summary>
        /// Point relative to a tall corner for the motor to go back to after corner aiming.
        /// </summary>
        [Tooltip("Point relative to a tall corner for the motor to go back to after corner aiming.")]
        [Range(0, 10)]
        public float LeftTallCornerOffset;

        /// <summary>
        /// Point relative to a tall corner for the motor to go back to after corner aiming.
        /// </summary>
        [Tooltip("Point relative to a tall corner for the motor to go back to after corner aiming.")]
        [Range(0, 10)]
        public float RightTallCornerOffset;

        /// <summary>
        /// Capsule radius used when determining if the character is in front of a low cover when entering it.
        /// </summary>
        [Tooltip("Capsule radius used when determining if the character is in front of a low cover when entering it.")]
        [Range(0, 10)]
        public float LowSideEnterRadius;

        /// <summary>
        /// Capsule radius used when determining if the character is in front of a low cover when entering it.
        /// </summary>
        [Tooltip("Capsule radius used when determining if the character is in front of a low cover when leaving it.")]
        [Range(0, 10)]
        public float LowSideLeaveRadius;

        /// <summary>
        /// Point relative to a low corner for the motor to go back to after corner aiming.
        /// </summary>
        [Tooltip("Point relative to a low corner for the motor to go back to after corner aiming.")]
        [Range(0, 10)]
        public float LeftLowCornerOffset;

        /// <summary>
        /// Point relative to a low corner for the motor to go back to after corner aiming.
        /// </summary>
        [Tooltip("Point relative to a low corner for the motor to go back to after corner aiming.")]
        [Range(0, 10)]
        public float RightLowCornerOffset;

        /// <summary>
        /// Time in seconds for player to start moving again after changing direction.
        /// </summary>
        [Tooltip("Time in seconds for player to start moving again after changing direction.")]
        [Range(0, 5)]
        public float DirectionChangeDelay;

        /// <summary>
        /// Approximate position shift done by the corner peek animation. Inverted when peeking left.
        /// </summary>
        [Tooltip("Approximate position shift done by the corner peek animation. Inverted when peeking left.")]
        public Vector3 CornerOffset;

        /// <summary>
        /// Settings for cover update delays.
        /// </summary>
        [Tooltip("Settings for cover update delays.")]
        public CoverUpdateSettings Update;

        /// <summary>
        /// Default cover settings.
        /// </summary>
        public static CoverSettings Default()
        {
            var settings = new CoverSettings();
            settings.CanUseTallCorners = true;
            settings.CanUseLowCorners = false;
            settings.ExitBack = 120;
            settings.LowCapsuleHeight = 0.75f;
            settings.LowAimCapsuleHeight = 1.25f;
            settings.LowRotationSpeed = 10.0f;
            settings.TallRotationSpeed = 15.0f;
            settings.EnterDistance = 0.15f;
            settings.LeaveDistance = 0.25f;
            settings.ClimbDistance = 0.5f;
            settings.MinCrouchDistance = 0.2f;
            settings.MaxCrouchDistance = 1.5f;
            settings.PivotSideMargin = 0.5f;
            settings.CornerAimTriggerDistance = 0.6f;
            settings.TallSideEnterRadius = 0.15f;
            settings.TallSideLeaveRadius = 0.05f;
            settings.LeftTallCornerOffset = 0.2f;
            settings.RightTallCornerOffset = 0.2f;
            settings.LeftLowCornerOffset = 0.4f;
            settings.RightLowCornerOffset = 0.4f;
            settings.LowSideEnterRadius = 0.3f;
            settings.LowSideLeaveRadius = 0.2f;
            settings.DirectionChangeDelay = 0.25f;
            settings.CornerOffset = new Vector3(0.8f, 0, 0);
            settings.Update = CoverUpdateSettings.Default();

            return settings;
        }
    }

    [Serializable]
    public struct CoverUpdateSettings
    {
        /// <summary>
        /// Cover check delay when idle and not in cover.
        /// </summary>
        [Tooltip("Cover check delay when idle and not in cover.")]
        public float IdleNonCover;

        /// <summary>
        /// Cover check delay when idle and in cover.
        /// </summary>
        [Tooltip("Cover check delay when idle and in cover.")]
        public float IdleCover;

        /// <summary>
        /// Cover check delay when moving outside of cover.
        /// </summary>
        [Tooltip("Cover check delay when moving outside of cover.")]
        public float MovingNonCover;

        /// <summary>
        /// Cover check delay when moving in cover.
        /// </summary>
        [Tooltip("Cover check delay when moving in cover.")]
        public float MovingCover;

        /// <summary>
        /// Default cover update settings.
        /// </summary>
        public static CoverUpdateSettings Default()
        {
            var settings = new CoverUpdateSettings();
            settings.IdleNonCover = 10;
            settings.IdleCover = 2;
            settings.MovingNonCover = 0.5f;
            settings.MovingCover = 0.1f;

            return settings;
        }
    }
}