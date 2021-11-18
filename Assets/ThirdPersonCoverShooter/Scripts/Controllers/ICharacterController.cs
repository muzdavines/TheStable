using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Requires all character controllers to be able to be updated after camera.
    /// </summary>
    public interface ICharacterController
    {
        /// <summary>
        /// Update the controller after camera has adjusted it's position.
        /// </summary>
        void UpdateAfterCamera();
    }
}
