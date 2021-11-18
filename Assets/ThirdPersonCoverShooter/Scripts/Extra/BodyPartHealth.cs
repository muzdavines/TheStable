using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Acts similarly to Character Health, but passed the taken damage to a first found Character Health component in the hierarchy.
    /// </summary>
    public class BodyPartHealth : MonoBehaviour
    {
        /// <summary>
        /// Target CharacterHealth.
        /// </summary>
        public CharacterHealth Target
        {
            get { return TargetOveride == null ? _target : TargetOveride; }
        }

        /// <summary>
        /// By default target is the first found parent object with CharacterHealth. Setting TargetOverride overrides it.
        /// </summary>
        [Tooltip("By default target is the first found parent object with CharacterHealth. Setting TargetOverride overrides it.")]
        public CharacterHealth TargetOveride;

        /// <summary>
        /// Multiplies taken damage before applying it to the target CharacterHealth.
        /// </summary>
        [Tooltip("Multiplies taken damage before applying it to the target CharacterHealth.")]
        public float DamageScale = 1.0f;

        private CharacterHealth _target;

        private static Dictionary<GameObject, BodyPartHealth> _map = new Dictionary<GameObject, BodyPartHealth>();

        public static BodyPartHealth Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

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

        private void Awake()
        {
            var obj = transform;

            while (obj != null && _target == null)
            {
                _target = obj.GetComponent<CharacterHealth>();
                obj = obj.transform.parent;
            }
        }

        /// <summary>
        /// Reduce health on bullet hit.
        /// </summary>
        public void OnHit(Hit hit)
        {
            var target = TargetOveride;

            if (target == null)
                target = _target;

            if (target == null)
            {
                var obj = transform;

                while (obj != null && target == null)
                {
                    target = obj.GetComponent<CharacterHealth>();
                    obj = obj.transform.parent;
                }

                _target = target;
            }

            if (target == null)
                return;

            hit.Damage *= DamageScale;

            target.SendMessage("OnHit",
                               hit,
                               SendMessageOptions.DontRequireReceiver);
        }
    }
}
