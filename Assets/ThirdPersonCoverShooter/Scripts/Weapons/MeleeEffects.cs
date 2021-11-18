using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Spawns effects prefabs on various melee events.
    /// </summary>
    [RequireComponent(typeof(BaseMelee))]
    public class MeleeEffects : MonoBehaviour, IMeleeListener
    {
        /// <summary>
        /// Object to instantiate when attacking.
        /// </summary>
        [Tooltip("Object to instantiate when attacking.")]
        public GameObject Attack;

        /// <summary>
        /// Object to instantiate when hitting.
        /// </summary>
        [Tooltip("Object to instantiate when hitting.")]
        public GameObject Hit;

        /// <summary>
        /// Object to instantiate when attacking during a specific animation moment.
        /// </summary>
        [Tooltip("Object to instantiate when attacking during a specific animation moment.")]
        public GameObject Moment;

        private List<GameObject> _particles = new List<GameObject>();

        /// <summary>
        /// Play melee attack effect.
        /// </summary>
        public void OnMeleeAttack()
        {
            instantiate(Attack, null, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Play melee hit effect.
        /// </summary>
        public void OnMeleeHit()
        {
            instantiate(Hit, null, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Play melee attack effect.
        /// </summary>
        public void OnMeleeMoment()
        {
            instantiate(Moment, null, transform.position, Quaternion.identity);
        }

        private void LateUpdate()
        {
            {
                int i = 0;
                while (i < _particles.Count)
                {
                    if (_particles[i] == null)
                        _particles.RemoveAt(i);
                    else
                        i++;
                }
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];

                if (particle == null)
                    _particles.RemoveAt(i);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];

                if (particle != null)
                    GameObject.Destroy(particle);
            }

            _particles.Clear();
        }

        private void instantiate(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, float destroyAfter = 4f)
        {
            if (prefab == null)
                return;

            var particle = GameObject.Instantiate(prefab);
            particle.transform.SetParent(parent);
            particle.transform.localPosition = position;
            particle.transform.localRotation = rotation;
            particle.SetActive(true);
            _particles.Add(particle);
            GameObject.Destroy(particle, destroyAfter);
        }
    }
}