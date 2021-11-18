using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Specifies the type of position the Module assumes
    /// </summary>
    [Serializable]
    public enum PositionMode
    {
        /// <summary>
        /// Position of the Module is determined by the position of the given transform
        /// </summary>
        TransformBased,

        /// <summary>
        /// Position of the Module is specified manually
        /// </summary>
        Manual
    }
}