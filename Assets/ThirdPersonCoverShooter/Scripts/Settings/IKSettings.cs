using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Character's IK settings.
    /// </summary>
    [Serializable]
    public struct IKSettings
    {
        /// <summary>
        /// Is IK enabled.
        /// </summary>
        [Tooltip("Is IK enabled.")]
        public bool Enabled;

        /// <summary>
        /// Minimum amount of iterations performed for each IK chain.
        /// </summary>
        [Tooltip("Minimum amount of iterations performed for each IK chain.")]
        [Range(1, 10)]
        public int MinIterations;

        /// <summary>
        /// The IK will be performed till either the target state is reached or maximum amount of iterations are performed.
        /// </summary>
        [Tooltip("The IK will be performed till either the target state is reached or maximum amount of iterations are performed.")]
        [Range(1, 10)]
        public int MaxIterations;

        /// <summary>
        /// Time in seconds to wait between IK updates.
        /// </summary>
        [Tooltip("Time in seconds to wait between IK updates.")]
        public DistanceRange Delay;

        /// <summary>
        /// Position of a left hand to maintain on a gun.
        /// </summary>
        [Tooltip("Position of a left hand to maintain on a gun.\nBones defined in the LeftBones property are adjusted till LeftHand is in the intended position.\nFor this to work LeftHand must be in the same hierarchy as those bones.")]
        public Transform LeftHandOverride;

        /// <summary>
        /// Position of a right hand to adjust by recoil.
        /// </summary>
        [Tooltip("Position of a right hand to adjust by recoil.\nBones defined in the RightBones property are adjusted till RightHand is in the intended position.\nFor this to work RightHand must be in the same hierarchy as those bones.")]
        public Transform RightHandOverride;

        /// <summary>
        /// Bone to adjust when a character is hit.
        /// </summary>
        [Tooltip("Bone to adjust when a character is hit.")]
        public Transform HitBoneOverride;

        /// <summary>
        /// Transform to manipulate so it is facing towards a target. Used when aiming a head.
        /// </summary>
        [Tooltip("Transform to manipulate so it is facing towards a target. Used when aiming a head.\nBones defined in the LookBones are modified till Look is pointing at the intended direction.\nTherefore it Look must be in the same hierarchy as thsoe bones.")]
        public Transform SightOverride;

        /// <summary>
        /// Default IK settings.
        /// </summary>
        public static IKSettings Default()
        {
            var settings = new IKSettings();
            settings.Enabled = true;
            settings.MinIterations = 2;
            settings.MaxIterations = 10;

            return settings;
        }
    }
}