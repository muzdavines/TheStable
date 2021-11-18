using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// An action that has an attached effect that is executed at the target position.
    /// </summary>
    [Serializable]
    public abstract class EffectAction : AIAction
    {
        /// <summary>
        /// Hit effect prefab that is instantiated on hit.
        /// </summary>
        [Tooltip("Hit effect prefab that is instantiated on hit.")]
        public GameObject EffectPrefab;

        /// <summary>
        /// Random sounds played on hit.
        /// </summary>
        [Tooltip("Random sounds played on hit.")]
        public AudioClip[] Sounds;

        /// <summary>
        /// Plays the effect at a location. If parent is given the effect is attached to it.
        /// </summary>
        protected void PlayEffect(Actor parent, Vector3 position)
        {
            if (EffectPrefab != null)
            {
                var obj = GameObject.Instantiate(EffectPrefab);
                obj.transform.position = position;

                if (parent != null)
                    obj.transform.SetParent(parent.transform, true);

                obj.SetActive(true);

                GameObject.Destroy(obj, 3);
            }

            if (Sounds.Length > 0)
            {
                var clip = Sounds[UnityEngine.Random.Range(0, Sounds.Length)];
                AudioSource.PlayClipAtPoint(clip, position);
            }
        }
    }
}
