using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterZoomListener
    {
        /// <summary>
        /// Character is zooming in.
        /// </summary>
        void OnZoom();

        /// <summary>
        /// Character is no longer zooming in.
        /// </summary>
        void OnUnzoom();

        /// <summary>
        /// Character starts using scope.
        /// </summary>
        void OnScope();

        /// <summary>
        /// Character is no longer using scope.
        /// </summary>
        void OnUnscope();
    }
}