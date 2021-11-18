using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Spawns effects prefabs on various gun events like reloads or gunfire.
    /// </summary>
    [RequireComponent(typeof(BaseGun))]
    public class GunEffects : MonoBehaviour, IGunListener
    {
        /// <summary>
        /// Object to instantiate when ejecting a magazine.
        /// </summary>
        [Tooltip("Object to instantiate when ejecting a magazine.")]
        public GameObject Eject;

        /// <summary>
        /// Object to instantiate when a magazine is put inside the gun.
        /// </summary>
        [Tooltip("Object to instantiate when a magazine is put inside the gun.")]
        public GameObject Rechamber;

        /// <summary>
        /// Object to instantiate on each bullet fire.
        /// </summary>
        [Tooltip("Object to instantiate on each bullet fire.")]
        public GameObject Fire;

        /// <summary>
        /// Object to instantiate on each shotgun pump.
        /// </summary>
        [Tooltip("Object to instantiate on each shotgun pump.")]
        public GameObject Pump;

        /// <summary>
        /// Object to instantiate on each fire attempt with an empty magazine.
        /// </summary>
        [Tooltip("Object to instantiate on each fire attempt with an empty magazine.")]
        public GameObject EmptyFire;

        /// <summary>
        /// Object to instantiate to simulate shell ejection.
        /// </summary>
        [Tooltip("Object to instantiate to simulate shell ejection.")]
        public GameObject Shell;

        private List<GameObject> _particles = new List<GameObject>();
        private List<Coroutine> _coroutines = new List<Coroutine>();
        private BaseGun _gun;

        private void Awake()
        {
            _gun = GetComponent<BaseGun>();
        }

        /// <summary>
        /// Play magazine eject effect.
        /// </summary>
        public void OnEject()
        {
            instantiate(Eject, null, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Play magazine load effect.
        /// </summary>
        public void OnRechamber()
        {
            instantiate(Rechamber, null, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Play shotgun pump effect.
        /// </summary>
        public void OnPump()
        {
            instantiate(Pump, null, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Play fire effects delayed by the given amount of time in seconds.
        /// </summary>
        /// <param name="delay">Time to delay the creation of effects.</param>
        public void OnFire(float delay)
        {
            if (_gun != null && _gun.Aim != null)
                _coroutines.Add(StartCoroutine(play(delay, Fire, _gun.Aim.transform, Vector3.zero, Quaternion.identity)));

            _coroutines.Add(StartCoroutine(play(delay, Shell, null, transform.position, Quaternion.identity)));
        }

        /// <summary>
        /// Play an effect when the gun fails to fire.
        /// </summary>
        public void OnEmptyFire()
        {
            if (EmptyFire == null)
                return;

            instantiate(EmptyFire, null, transform.position, Quaternion.identity, 3);
        }

        public void OnFullyLoaded() { }
        public void OnBulletLoad() { }
        public void OnPumpStart() { }
        public void OnMagazineLoadStart() { }
        public void OnBulletLoadStart() { }

        private void LateUpdate()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];

                if (particle == null)
                    _particles.RemoveAt(i);
            }

            for (int i = _coroutines.Count - 1; i >= 0; i--)
            {
                var coroutine = _coroutines[i];

                if (coroutine == null)
                    _coroutines.RemoveAt(i);
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

            for (int i = 0; i < _coroutines.Count; i++)
            {
                var coroutine = _coroutines[i];

                if (coroutine != null)
                    StopCoroutine(coroutine);
            }

            _coroutines.Clear();
        }

        private IEnumerator play(float delay, GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, float destroyAfter = 4f)
        {
            if (prefab == null)
                yield break;

            yield return new WaitForSeconds(delay);

            instantiate(prefab, parent, position, rotation, destroyAfter);
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