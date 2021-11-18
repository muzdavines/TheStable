using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Keeps character on top of a moving platform. It doesn’t require a Character Motor or any other component and therefore can be used on any object even if it’s not physical.
    /// </summary>
    public class CharacterPlatform : MonoBehaviour
    {
        /// <summary>
        /// Distance below feet to check for ground.
        /// </summary>
        [Tooltip("Distance below feet to check for ground.")]
        [Range(0, 1)]
        public float Threshold = 0.2f;

        private GameObject _platform;
        private GameObject _previousPlatform;
        private Vector3 _lastLocalPointOnPlatform;
        private Vector3 _lastPlatformRelevantPosition;

        private RaycastHit[] _cache = new RaycastHit[16];

        private void LateUpdate()
        {
            findPlatform();

            if (_platform != null && _platform == _previousPlatform)
            {
                transform.Rotate(0, _platform.transform.eulerAngles.y - _previousPlatform.transform.eulerAngles.y, 0);
                transform.position += _platform.transform.TransformPoint(_lastLocalPointOnPlatform) - _lastPlatformRelevantPosition;

                updatePlatformPoints();
            }

            _previousPlatform = _platform;
        }

        private void updatePlatformPoints()
        {
            _lastPlatformRelevantPosition = transform.position;
            _lastLocalPointOnPlatform = _platform.transform.InverseTransformPoint(transform.position);
        }

        private void findPlatform()
        {
            GameObject newPlatform = null;
            float height = Threshold + 0.1f;

            if (_platform != null)
            {
                var offset = _platform.transform.TransformPoint(_lastLocalPointOnPlatform) - _lastPlatformRelevantPosition;

                if (offset.y < 0)
                    height -= offset.y;
            }

            var count = Physics.RaycastNonAlloc(transform.position + Vector3.up * 0.1f, Vector3.down, _cache, height, Layers.Geometry);

            for (int i = 0; i < count; i++)
            {
                var hit = _cache[i];

                if (!hit.collider.isTrigger)
                    if (hit.collider.gameObject != gameObject)
                        newPlatform = hit.collider.gameObject;
            }

            if (newPlatform != _platform && newPlatform != null)
            {
                _platform = newPlatform;
                updatePlatformPoints();
            }
            else
                _platform = newPlatform;
        }
    }
}