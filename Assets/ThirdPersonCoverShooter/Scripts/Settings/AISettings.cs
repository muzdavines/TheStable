using System;
using UnityEngine;

namespace CoverShooter
{
    public enum AIStartMode
    {
        idle,
        patrol,
        alerted,
        searchAround,
        searchPosition,
        investigate
    }

    [Serializable]
    public struct AITargetSettings
    {
        /// <summary>
        /// Minimum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.
        /// </summary>
        [Tooltip("Minimum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.")]
        [Range(0, 1)]
        public float Min;

        /// <summary>
        /// Maximum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.
        /// </summary>
        [Tooltip("Maximum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.")]
        [Range(0, 1)]
        public float Max;

        public AITargetSettings(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Describes possible error of the AI guesses.
    /// </summary>
    [Serializable]
    public struct AIApproximationSettings
    {
        /// <summary>
        /// Minimal possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.
        /// </summary>
        [Tooltip("Minimal possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.")]
        public float Min;

        /// <summary>
        /// Maximum possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.
        /// </summary>
        [Tooltip("Maximum possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.")]
        public float Max;

        /// <summary>
        /// Distance at which the AI is using Min value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.
        /// </summary>
        [Tooltip("Distance at which the AI is using Min value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.")]
        public float MinDistance;

        /// <summary>
        /// Distance at which the AI is using Max value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.
        /// </summary>
        [Tooltip("Distance at which the AI is using Max value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.")]
        public float MaxDistance;

        /// <summary>
        /// Constructs a new distance range.
        /// </summary>
        public AIApproximationSettings(float min, float max, float minDistance, float maxDistance)
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

    /// <summary>
    /// Settings for AI start.
    /// </summary>
    [Serializable]
    public struct AIStartSettings
    {
        /// <summary>
        /// Should the AI return to the starting state when becoming idle.
        /// </summary>
        [Tooltip("Should the AI return to the starting state when becoming idle.")]
        public bool ReturnOnIdle;

        /// <summary>
        /// Mode in which the AI starts.
        /// </summary>
        [Tooltip("Mode in which the AI starts.")]
        public AIStartMode Mode;

        /// <summary>
        /// Position the AI should investigate if the mode is set to searchPosition or investigate.
        /// </summary>
        [Tooltip("Position the AI should investigate if the mode is set to searchPosition or investigate.")]
        public Vector3 Position;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIStartSettings Default()
        {
            var result = new AIStartSettings();
            result.ReturnOnIdle = false;
            result.Mode = AIStartMode.patrol;
            result.Position = Vector3.zero;

            return result;
        }
    }

    /// <summary>
    /// Results of an AI call.
    /// </summary>
    [Serializable]
    public struct AICall
    {
        /// <summary>
        /// Target object that will receive the message.
        /// </summary>
        [Tooltip("Target object that will receive the message.")]
        public GameObject Target;

        /// <summary>
        /// Function name in a script that belongs in the target object.
        /// </summary>
        [Tooltip("Function name in a script that belongs in the target object.")]
        public string Message;

        /// <summary>
        /// Should the calling Actor component be passed to the called function as an argument.
        /// </summary>
        [Tooltip("Should the calling Actor component be passed to the called function as an argument.")]
        public bool PassCaller;

        public void Make(Actor caller)
        {
            if (Target != null)
            {
                if (PassCaller)
                    Target.SendMessage(Message, caller, SendMessageOptions.RequireReceiver);
                else
                    Target.SendMessage(Message, SendMessageOptions.RequireReceiver);
            }
        }

        public static AICall Default()
        {
            var result = new AICall();
            result.Target = null;
            result.Message = "Spawn";
            result.PassCaller = true;

            return result;
        }
    }

    /// <summary>
    /// Settings for AI grenades.
    /// </summary>
    [Serializable]
    public struct AIGrenadeSettings
    {
        /// <summary>
        /// Number of grenades the AI will throw.
        /// </summary>
        [Tooltip("Number of grenades the AI will throw.")]
        public int GrenadeCount;

        /// <summary>
        /// Time in seconds since becoming alerted to wait before throwing a grenade.
        /// </summary>
        [Tooltip("Time in seconds since becoming alerted to wait before throwing a grenade.")]
        public float FirstCheckDelay;

        /// <summary>
        /// AI will only throw a grenade if it can hit the enemy. CheckInterval defines the time between checks.
        /// </summary>
        [Tooltip("AI will only throw a grenade if it can hit the enemy. CheckInterval defines the time between checks.")]
        public float CheckInterval;

        /// <summary>
        /// Time in seconds to wait before throwing a grenade after already having thrown one.
        /// </summary>
        [Tooltip("Time in seconds to wait before throwing a grenade after already having thrown one.")]
        public float Interval;

        /// <summary>
        /// Maximum allowed distance between a landed grenade and the enemy. Throws with greater result distance are cancelled.
        /// </summary>
        [Tooltip("Maximum allowed distance between a landed grenade and the enemy. Throws with greater result distance are cancelled.")]
        public float MaxRadius;

        /// <summary>
        /// Distance to maintain against grenades.
        /// </summary>
        [Tooltip("Distance to maintain against grenades.")]
        public float AvoidDistance;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIGrenadeSettings Default()
        {
            var result = new AIGrenadeSettings();
            result.GrenadeCount = 1;
            result.FirstCheckDelay = 5;
            result.Interval = 10;
            result.CheckInterval = 2;
            result.MaxRadius = 5;
            result.AvoidDistance = 8;

            return result;
        }
    }
}
