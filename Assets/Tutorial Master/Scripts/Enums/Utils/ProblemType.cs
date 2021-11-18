using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Contains common type of problems which may prevent Tutorial Master from not operating correctly.
    /// </summary>
    [Serializable]
    public enum ProblemType
    {
        /// <summary>
        /// Tutorial Data is missing or there's something wrong with it.
        /// </summary>
        BadTutorialData,

        /// <summary>
        /// One or more fields of a Tutorial are null or invalid.
        /// </summary>
        TutorialInvalid,

        /// <summary>
        /// Tutorial of such ID does not exist.
        /// </summary>
        BadTutorialId,

        /// <summary>
        /// There are no Tutorials to play.
        /// </summary>
        NoTutorials,

        /// <summary>
        /// There is already a Tutorial playing.
        /// </summary>
        TutorialPlaying,

        /// <summary>
        /// ActiveStage is disabled.
        /// </summary>
        StageDisabled,

        /// <summary>
        /// One or more fields of a ActiveStage are null or invalid.
        /// </summary>
        StageInvalid,

        /// <summary>
        /// This Tutorial has no Stages.
        /// </summary>
        NoStages,

        /// <summary>
        /// ActiveStage of such ID does not exist.
        /// </summary>
        BadStageId,
    }
}