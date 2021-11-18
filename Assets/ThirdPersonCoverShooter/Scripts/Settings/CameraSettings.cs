using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Settings for each camera state.
    /// </summary>
    [Serializable]
    public struct CameraStates
    {
        /// <summary>
        /// Camera state used when the character is unarmed.
        /// </summary>
        [Tooltip("Camera state used when the character is unarmed.")]
        public CameraState Default;

        /// <summary>
        /// Camera state to use when the character is standing and aiming.
        /// </summary>
        [Tooltip("Camera state to use when the character is standing and aiming.")]
        public CameraState Aim;

        /// <summary>
        /// Camera state to use when the controller is in melee mode.
        /// </summary>
        [Tooltip("Camera state to use when the controller is in melee mode.")]
        public CameraState Melee;

        /// <summary>
        /// Camera state to use when the character is crouching.
        /// </summary>
        [Tooltip("Camera state to use when the character is crouching.")]
        public CameraState Crouch;

        /// <summary>
        /// Camera state to use when the character is in low cover and not aiming.
        /// </summary>
        [Tooltip("Camera state to use when the character is in low cover and not aiming.")]
        public CameraState LowCover;

        /// <summary>
        /// Camera state to use when the character is low cover and grenade mode.
        /// </summary>
        [Tooltip("Camera state to use when the character is low cover and grenade mode.")]
        public CameraState LowCoverGrenade;

        /// <summary>
        /// Camera state to use when the character is in tall cover.
        /// </summary>
        [Tooltip("Camera state to use when the character is in tall cover.")]
        public CameraState TallCover;

        /// <summary>
        /// Camera state to use when the character is in tall cover and camera is facing back.
        /// </summary>
        [Tooltip("Camera state to use when the character is in tall cover and camera is facing back.")]
        public CameraState TallCoverBack;

        /// <summary>
        /// Camera state to use when the character is ready to fire from the right corner of a cover.
        /// </summary>
        [Tooltip("Camera state to use when the character is ready to fire from a corner of a cover.")]
        public CameraState Corner;

        /// <summary>
        /// Camera state to use when the character is climbing.
        /// </summary>
        [Tooltip("Camera state to use when the character is climbing.")]
        public CameraState Climb;

        /// <summary>
        /// Camera state to use when the character is dead.
        /// </summary>
        [Tooltip("Camera state to use when the character is dead.")]
        public CameraState Dead;

        /// <summary>
        /// Camera state to use when the character is using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is using zoom.")]
        public CameraState Zoom;

        /// <summary>
        /// Camera state to use when the character is aiming from a corner in low cover and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is aiming from a corner in low cover and using zoom.")]
        public CameraState LowCornerZoom;

        /// <summary>
        /// Camera state to use when the character is aiming from a corner in tall cover and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is aiming from a corner in tall cover and using zoom.")]
        public CameraState TallCornerZoom;

        /// <summary>
        /// Default camera settings.
        /// </summary>
        public static CameraStates GetDefault()
        {
            var states = new CameraStates();
            states.Default = CameraState.Default();
            states.Aim = CameraState.Aim();
            states.Melee = CameraState.Melee();
            states.Crouch = CameraState.Crouch();
            states.LowCover = CameraState.LowCover();
            states.LowCoverGrenade = CameraState.LowCoverGrenade();
            states.TallCover = CameraState.TallCover();
            states.TallCoverBack = CameraState.TallCoverBack();
            states.Corner = CameraState.Corner();
            states.Climb = CameraState.Climb();
            states.Dead = CameraState.Dead();
            states.Zoom = CameraState.Zoom();
            states.LowCornerZoom = CameraState.LowCornerZoom();
            states.TallCornerZoom = CameraState.TallCornerZoom();

            return states;
        }
    }

    public enum Pivot
    {
        constant,
        rightShoulder,
        leftShoulder
    }

    /// <summary>
    /// Defines camera state for a single gameplay situation.
    /// </summary>
    [Serializable]
    public struct CameraState
    {
        /// <summary>
        /// Offset from the pivot. The offset is rotated using camera's Horizontal and Vertical values.
        /// </summary>
        [Tooltip("Offset from the pivot. The offset is rotated using camera's Horizontal and Vertical values.")]
        public Vector3 Offset;

        /// <summary>
        /// Final rotation of the camera once it is in position.
        /// </summary>
        [Tooltip("Final rotation of the camera once it is in position.")]
        public Vector3 Orientation;

        /// <summary>
        /// Position kind around which the camera is rotated.
        /// </summary>
        [Tooltip("Position around which the camera is rotated.")]
        public Pivot Pivot;

        /// <summary>
        /// Position around which the camera is rotated if pivot is set to constant.
        /// </summary>
        public Vector3 ConstantPivot;

        /// <summary>
        /// Field of view.
        /// </summary>
        [Tooltip("Field of view.")]
        [Range(0, 360)]
        public float FOV;

        /// <summary>
        /// Minimum allowed vertical angle in degrees.
        /// </summary>
        [Tooltip("Minimum allowed vertical angle in degrees.")]
        public float MinAngle;

        /// <summary>
        /// Maximum allow vertical angle in degrees.
        /// </summary>
        [Tooltip("Maximum allow vertical angle in degrees.")]
        public float MaxAngle;

        public static CameraState Default()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.64f, 0.1f, -2.5f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState Aim()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.75f, -0.25f, -1.7f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState Melee()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.3f, -0.3f, -4f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState Crouch()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.75f, -0.8f, -1f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState LowCover()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(0.5f, -0.3f, -1.5f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState LowCoverGrenade()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.23f, -0.23f, -2.2f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState TallCover()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.5f, -0.8f, -1.5f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState TallCoverBack()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 1.6f, 0);
            settings.Offset = new Vector3(0.65f, -0.2f, -1.5f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState Corner()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(1.2f, -0.2f, -2.4f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState Climb()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.75f, -0.25f, -1.7f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState Dead()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 0, 0);
            settings.Offset = new Vector3(0f, 1f, -2.5f);
            settings.FOV = 60;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState Zoom()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.rightShoulder;
            settings.ConstantPivot = new Vector3(0, 0, 0);
            settings.Offset = new Vector3(0.5f, 0.15f, -1f);
            settings.FOV = 40;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState LowCornerZoom()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 1.35f, 0);
            settings.Offset = new Vector3(1.2f, -0.2f, -1.2f);
            settings.FOV = 40;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }

        public static CameraState TallCornerZoom()
        {
            var settings = new CameraState();
            settings.Pivot = Pivot.constant;
            settings.ConstantPivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(0.8f, -0.2f, -1.2f);
            settings.FOV = 40;
            settings.MinAngle = -65;
            settings.MaxAngle = 45;

            return settings;
        }
    }
}