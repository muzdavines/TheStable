using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores base settings for an Effect.
    /// </summary>
    [Serializable]
    public abstract class EffectSettings
    {
        /// <summary>
        /// If false, all interactivity will be disabled while this effect is active.
        /// </summary>
        public bool CanInteract = true;

        /// <summary>
        /// Determines whether or not this Effect is enabled.
        /// </summary>
        public bool IsEnabled;

        /// <summary>
        /// Determines the speed of the effect
        /// </summary>
        public float Speed = 3.5f;
    }
}