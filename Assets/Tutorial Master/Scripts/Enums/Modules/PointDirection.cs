using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Enumerator used to specify from which direction an Arrow Module is pointing at
    /// </summary>
    [Serializable]
    public enum PointDirection
    {
        /// <summary>
        /// Point from the Top side
        /// </summary>
        Up,

        /// <summary>
        /// Point from the Bottom side
        /// </summary>
        Down,

        /// <summary>
        /// Point from the Left side
        /// </summary>
        Left,

        /// <summary>
        /// Point from the Right side
        /// </summary>
        Right,

        /// <summary>
        /// Point from the Top Left side
        /// </summary>
        TopLeft,

        /// <summary>
        /// Point from the Top Right side
        /// </summary>
        TopRight,

        /// <summary>
        /// Point from the Bottom Left side
        /// </summary>
        BottomLeft,

        /// <summary>
        /// Point from the Bottom Right side
        /// </summary>
        BottomRight,

        /// <summary>
        /// Directly looks at a specified transform
        /// </summary>
        LookAtTransform,
    }
}