using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores settings for a Floating Effect
    /// </summary>
    /// <seealso cref="EffectSettings" />
    [Serializable]
    public class FloatEffectSettings : EffectSettings
    {
        /// <summary>
        /// The custom pattern for this floating effect
        /// </summary>
        public AnimationCurve CustomPattern;

        /// <summary>
        /// Specifies the direction in which the floating would occur
        /// </summary>
        public Orientation Direction = Orientation.Vertical;

        /// <summary>
        /// The pattern for this floating effect
        /// </summary>
        public WaveType FloatPattern = WaveType.Sine;

        /// <summary>
        /// The minimum range offset of floating
        /// </summary>
        public float FloatRange = 20;

        public FloatEffectSettings()
        {
            // Set a Custom Pattern to form a Sine Wave
            CustomPattern = new AnimationCurve
            (
                new Keyframe(0, 0, 0, 0),
                new Keyframe(1, 1, 0, 0),
                new Keyframe(2, 0, 0, 0))
            {
                preWrapMode = WrapMode.Loop,
                postWrapMode = WrapMode.Loop
            };
        }
    }
}