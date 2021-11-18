using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Determines type of action which would be executed
    /// </summary>
    [Serializable]
    public enum ActionType
    {
        /// <summary>
        /// Plays the next stage
        /// </summary>
        PlayNextStage,

        /// <summary>
        /// Stops currently active tutorial
        /// </summary>
        StopTutorial,

        /// <summary>
        /// Does nothing
        /// </summary>
        Nothing
    }
}