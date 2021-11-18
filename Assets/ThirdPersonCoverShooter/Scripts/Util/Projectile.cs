using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// An object that flies a distance and then destroys itself.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        /// <summary>
        /// Speed of the projectile in meters per second.
        /// </summary>
        [Tooltip("Speed of the projectile in meters per second.")]
        public float Speed = 10;

        [HideInInspector]
        public float Distance = 1;

        [HideInInspector]
        public Vector3 Direction;

        [HideInInspector]
        public GameObject Target;

        [HideInInspector]
        public Hit Hit;

        private float _path = 0;

        private void OnEnable()
        {
            _path = 0;
        }

        private void Update()
        {
            transform.position += Direction * Speed * Time.deltaTime;
            _path += Speed * Time.deltaTime;

            if (_path >= Distance)
            {
                if (Target != null)
                    Target.SendMessage("OnHit", Hit, SendMessageOptions.DontRequireReceiver);

                GameObject.Destroy(gameObject);
            }
        }
    }
}