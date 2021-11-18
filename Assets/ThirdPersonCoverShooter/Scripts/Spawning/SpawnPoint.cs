using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes a single point in space and prefabs that are by default spanwed here when required.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        /// <summary>
        /// A random prefab is taken to be instantiated during a spawn.
        /// </summary>
        [Tooltip("A random prefab is taken to be instantiated during a spawn.")]
        public GameObject[] Prefabs;

        /// <summary>
        /// Spawns a specific prefab. Returns the clone.
        /// </summary>
        public static GameObject SpawnPrefab(GameObject prefab, Vector3 position, Actor caller)
        {
            var clone = GameObject.Instantiate(prefab);
            clone.transform.SetParent(null);
            clone.transform.position = position;
            clone.SetActive(true);
            clone.SendMessage("OnSpawn", caller);

            return clone;
        }


        /// <summary>
        /// Spawns and returns a clone of a random prefab.
        /// </summary>
        /// <returns></returns>
        public GameObject Spawn(Actor caller)
        {
            if (Prefabs == null || Prefabs.Length == 0)
                return null;

            return SpawnPrefab(Prefabs[Random.Range(0, Prefabs.Length)], transform.position, caller);
        }
    }
}
