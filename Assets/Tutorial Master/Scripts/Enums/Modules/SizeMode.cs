using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Specifies the size type for a Highlighter module
    /// </summary>
    [Serializable]
    public enum SizeMode
    {
        /// <summary>
        /// Size will be determined based on transform target UI boundaries
        /// </summary>
        BasedOnUITransform,

        /// <summary>
        /// Custom size
        /// </summary>
        CustomSize
    }
}