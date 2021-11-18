using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Type of placement Module will have based on a UI Transform
    /// </summary>
    [Serializable]
    public enum PlacementType
    {
        /// <summary>
        /// In the center of a given transform (same position as a given transform)
        /// </summary>
        Center,

        /// <summary>
        /// Up a given transform
        /// </summary>
        Top,

        /// <summary>
        /// Bottom a given transform
        /// </summary>
        Bottom,

        /// <summary>
        /// On the left side of a given transform
        /// </summary>
        Left,

        /// <summary>
        /// On the right side of a given transform
        /// </summary>
        Right,

        /// <summary>
        /// On the top left of a given transform
        /// </summary>
        TopLeft,

        /// <summary>
        /// On the top right of a given transform
        /// </summary>
        TopRight,

        /// <summary>
        /// On the bottom left of a given transform
        /// </summary>
        BottomLeft,

        /// <summary>
        /// On the bottom right of a given transform
        /// </summary>
        BottomRight
    }
}