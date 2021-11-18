using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Records and stores Unity renderer visibility.
    /// </summary>
    public class Visibility : MonoBehaviour
    {
        /// <summary>
        /// True if object is marked as visible the Unity.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
        }

        private bool _isVisible;

        private void OnBecameVisible()
        {
            _isVisible = true;
        }

        private void OnBecameInvisible()
        {
            _isVisible = false;
        }
    }
}
