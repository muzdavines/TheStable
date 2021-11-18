using UnityEngine;
using UnityEngine.Serialization;

namespace CoverShooter
{
    /// <summary>
    /// Spawns an effect upon a hit. Used mostly on static level geometry.    
    /// </summary>
    public class HitEffect : MonoBehaviour
    {
        /// <summary>
        /// Effect to be instantiated on the point of bullet impact.
        /// </summary>
        [Tooltip("Effect to be instantiated on the point of bullet impact.")]
        [FormerlySerializedAs("Effect")]
        public GameObject Bullet;

        /// <summary>
        /// Effect to be instantiated on the point of melee impact.
        /// </summary>
        [Tooltip("Effect to be instantiated on the point of melee impact.")]
        public GameObject Melee;

        /// <summary>
        /// Time to wait before destroying an instantiated effect object.
        /// </summary>
        [Tooltip("Time to wait before destroying an instantiated effect object.")]
        public float DestroyAfter = 5;

        private Collider _collider;

        /// <summary>
        /// Spawns the particle effect on hit position.
        /// </summary>
        public void OnHit(Hit hit)
        {
            var prefab = hit.IsMelee ? Melee : Bullet;

            if (prefab == null)
                return;

            var effect = GameObject.Instantiate(prefab);
            effect.transform.SetParent(null);
            effect.transform.position = hit.Position + hit.Normal * 0.1f;
            effect.SetActive(true);
            GameObject.Destroy(effect, 4);
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnValidate()
        {
            DestroyAfter = Mathf.Max(0, DestroyAfter);
        }
    }
}