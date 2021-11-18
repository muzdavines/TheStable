using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores settings for a Scale Pulsing Effect
    /// </summary>
    /// <seealso cref="EffectSettings" />
    [Serializable]
    public class ScalePulsingEffectSettings : EffectSettings
    {
        /// <summary>
        /// The maximum size offset
        /// </summary>
        public float HeightRange = 2;

        /// <summary>
        /// The minimum size offset
        /// </summary>
        public float WidthRange = 2;
    }
}