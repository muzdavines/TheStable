using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Used to specify when to play an Audio Clip
    /// </summary>
    [Serializable]
    public enum AudioTiming
    {
        /// <summary>
        /// Audio never plays for this ActiveStage
        /// </summary>
        Never,

        /// <summary>
        /// Audio plays when Tutorial Master enters the ActiveStage
        /// </summary>
        OnStageEnter,

        /// <summary>
        /// Audio plays when Tutorial Master leaves the ActiveStage
        /// </summary>
        OnStageExit
    }
}