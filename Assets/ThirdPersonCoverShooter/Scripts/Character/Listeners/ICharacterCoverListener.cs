using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterCoverListener
    {
        /// <summary>
        /// Character enters a cover.
        /// </summary>
        void OnEnterCover(Cover cover);

        /// <summary>
        /// Character leaves cover.
        /// </summary>
        void OnLeaveCover();
    }
}