using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Stops bullets and melee attacks from moving past the game object. Works in only one direction.
    /// </summary>
    public class BulletShield : MonoBehaviour
    {
        private static Dictionary<GameObject, BulletShield> _map = new Dictionary<GameObject, BulletShield>();

        /// <summary>
        /// Returns a shield attached to the object.
        /// </summary>
        public static BulletShield Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

        /// <summary>
        /// Returns true if the given object contains a bullet shield component.
        /// </summary>
        public static bool Contains(GameObject gameObject)
        {
            return _map.ContainsKey(gameObject);
        }

        private void OnEnable()
        {
            _map[gameObject] = this;
        }

        private void OnDisable()
        {
            _map.Remove(gameObject);
        }
    }
}
