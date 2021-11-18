using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Specifies the direction of the movement
    /// </summary>
    [Serializable]
    public enum Orientation
    {
        /// <summary>
        /// Move horizontally
        /// </summary>
        Horizontal,

        /// <summary>
        /// Move vertically
        /// </summary>
        Vertical,

        /// <summary>
        /// Move diagonally left
        /// </summary>
        DiagonalLeft,

        /// <summary>
        /// Move diagonally right
        /// </summary>
        DiagonalRight,
    }
}