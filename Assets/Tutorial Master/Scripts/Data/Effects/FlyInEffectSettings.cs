using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores settings for a Fly In Effect
    /// </summary>
    /// <seealso cref="EffectSettings" />
    [Serializable]
    public class FlyInEffectSettings : EffectSettings
    {
        /// <summary>
        /// Direction from which Module will fly in.
        /// </summary>
        public FlyDirection FlyDirection;

        /// <summary>
        /// Distance from module's final position.
        /// </summary>
        public float FlyDistance = 100;

        public FlyInEffectSettings()
        {
            Speed = 0.05f;
        }
    }
}