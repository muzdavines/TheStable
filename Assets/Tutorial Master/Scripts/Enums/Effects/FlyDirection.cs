using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Determines the direction of the effect
    /// </summary>
    [Serializable]
    public enum FlyDirection
    {
        /// <summary>
        /// The top
        /// </summary>
        Top,

        /// <summary>
        /// The bottom
        /// </summary>
        Bottom,

        /// <summary>
        /// Fly In from the left
        /// </summary>
        Left,

        /// <summary>
        /// The right
        /// </summary>
        Right,

        /// <summary>
        /// The top left
        ///
        /// </summary>
        TopLeft,

        /// <summary>
        /// The top right
        /// </summary>
        TopRight,

        /// <summary>
        /// The bottom left
        /// </summary>
        BottomLeft,

        /// <summary>
        /// The bottom right
        /// </summary>
        BottomRight
    }
}