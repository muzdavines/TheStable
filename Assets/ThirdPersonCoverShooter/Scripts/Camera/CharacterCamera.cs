using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// All cameras are to be able to update at certain moments in a frame.
    /// </summary>
    public abstract class CharacterCamera : MonoBehaviour
    {
        /// <summary>
        /// Update performed after the character motor does it's thing.
        /// </summary>
        public abstract void UpdateForCharacterMotor();

        /// <summary>
        /// Asks the camera to call UpdateAfterCamera on the given controller after the camera does it's update.
        /// </summary>
        public abstract void DeferUpdate(ICharacterController controller);
    }
}
