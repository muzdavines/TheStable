using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Holds the camera object. Usually it's Camera.main, but can be overriden.
    /// </summary>
    public static class CameraManager
    {
        /// <summary>
        /// Main camera object. Usually it's Camera.main, but can be overriden.
        /// </summary>
        public static Camera Main
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;

                return _camera;
            }

            set
            {
                _camera = value;
            }
        }

        private static Camera _camera;

        /// <summary>
        /// Set's camera object to be Camera.main.
        /// </summary>
        public static void Update()
        {
            _camera = Camera.main;
        }
    }
}
