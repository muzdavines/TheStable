using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes a trigger zone area.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Zone<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// Width of the box.
        /// </summary>
        public float Width
        {
            get { return _collider.size.x * transform.localScale.x; }
        }

        /// <summary>
        /// Height of the box.
        /// </summary>
        public float Height
        {
            get { return _collider.size.y * transform.localScale.y; }
        }

        /// <summary>
        /// Depth of the box.
        /// </summary>
        public float Depth
        {
            get { return _collider.size.z * transform.localScale.z; }
        }

        /// <summary>
        /// Y coordinate of the block's bottom in world space.
        /// </summary>
        public float Bottom
        {
            get { return _collider.bounds.min.y; }
        }

        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
        }

        private static List<T> _list = new List<T>();
        private static Dictionary<GameObject, T> _map = new Dictionary<GameObject, T>();

        private void OnEnable()
        {
            var t = GetComponent<T>();

            if (!_list.Contains(t))
                _list.Add(t);

            _map[gameObject] = t;
        }

        private void OnDisable()
        {
            var t = GetComponent<T>();

            if (_list.Contains(t))
                _list.Remove(t);

            if (_map.ContainsKey(gameObject))
                _map.Remove(gameObject);
        }

        private void OnDestroy()
        {
            var t = GetComponent<T>();

            if (_list.Contains(t))
                _list.Remove(t);

            if (_map.ContainsKey(gameObject))
                _map.Remove(gameObject);
        }

        /// <summary>
        /// Enumerates through all of the active zones.
        /// </summary>
        public static IEnumerable<T> All
        {
            get { return _list; }
        }

        /// <summary>
        /// Returns the number of zones.
        /// </summary>
        public static int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Returns the zone at index. Use Count to get the number of zones.
        /// </summary>
        public static T Get(int index)
        {
            return _list[index];
        }

        /// <summary>
        /// Returns the component to the attached object.
        /// </summary>
        public static T Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }
    }
}