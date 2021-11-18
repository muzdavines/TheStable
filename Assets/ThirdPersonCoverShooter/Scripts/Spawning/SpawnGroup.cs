using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages a group of spawn points. Can override prefabs set inside the spawn points.
    /// </summary>
    public class SpawnGroup : MonoBehaviour
    {
        /// <summary>
        /// Positions at which the characters are spawned. Taken from child objects if not specified.
        /// </summary>
        [Tooltip("Positions at which the characters are spawned. Taken from child objects if not specified.")]
        public SpawnPoint[] SpawnPointOverride;

        /// <summary>
        /// Prefabs to use when spawning. Overrides the ones specified in the SpawnPoints.
        /// </summary>
        [Tooltip("Prefabs to use when spawning. Overrides the ones specified in the SpawnPoints.")]
        public GameObject[] PrefabOverride;

        /// <summary>
        /// Should the PrefabOverride be used only on the spawn points that have no prefabs specified.
        /// </summary>
        [Tooltip("Should the PrefabOverride be used only on the spawn points that have no prefabs specified.")]
        public bool OnlyOverrideEmptySpawnPoints = true;

        /// <summary>
        /// Another spawn group to trigger instead by random chance.
        /// </summary>
        [Tooltip("Another spawn group to trigger instead by random chance.")]
        public SpawnGroup Secondary;

        /// <summary>
        /// Chance that a secondary spawn group will be used. Ignored if it not set to any.
        /// </summary>
        [Tooltip("Chance that a secondary spawn group will be used. Ignored if it not set to any.")]
        [Range(0, 1)]
        public float SecondaryChance = 0.5f;

        /// <summary>
        /// Time in seconds before another call to spawn is allowed. Ignored by SpawnPicker.
        /// </summary>
        [Tooltip("Time in seconds before another call to spawn is allowed. Ignored by SpawnPicker.")]
        public float MinInterval = 1;

        private List<SpawnPoint> _points = new List<SpawnPoint>();
        private float _lastTime;

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var point = transform.GetChild(i).GetComponent<SpawnPoint>();

                if (point != null)
                    _points.Add(point);
            }
        }

        /// <summary>
        /// Returns active spawn points.
        /// </summary>
        public IEnumerable<SpawnPoint> Points
        {
            get
            {
                if (SpawnPointOverride != null && SpawnPointOverride.Length > 0)
                    return SpawnPointOverride;
                else
                    return _points;
            }
        }

        /// <summary>
        /// Instantiates prefabs on all spawn points.
        /// </summary>
        public void Spawn(Actor caller)
        {
            foreach (var actor in SpawnAndReturn(int.MaxValue, caller)) { }
        }

        /// <summary>
        /// Instantiates and returns prefabs on all spawn points.
        /// </summary>
        public IEnumerable<GameObject> SpawnAndReturn(int max, Actor caller, bool isForced = false)
        {
            if (Secondary != null && Random.Range(0f, 1f) <= SecondaryChance)
            {
                foreach (var actor in Secondary.SpawnAndReturn(max, caller))
                    yield return actor;
            }
            else
            {
                if (!isForced && Time.timeSinceLevelLoad - _lastTime < MinInterval)
                    yield break;

                _lastTime = Time.timeSinceLevelLoad;

                foreach (var point in Points)
                {
                    if (PrefabOverride == null || PrefabOverride.Length == 0)
                        yield return point.Spawn(caller);
                    else if (point.Prefabs == null || point.Prefabs.Length == 0 || !OnlyOverrideEmptySpawnPoints)
                        yield return SpawnPoint.SpawnPrefab(PrefabOverride[Random.Range(0, PrefabOverride.Length)], point.transform.position, caller);
                    else
                        yield return point.Spawn(caller);
                }
            }
        }

        /// <summary>
        /// Instantiates and returns given prefabs on all spawn points.
        /// </summary>
        public IEnumerable<GameObject> SpawnAndReturn(int max, GameObject[] prefabs, Actor caller, bool isForced = false)
        {
            if (Secondary != null && Random.Range(0f, 1f) <= SecondaryChance)
            {
                foreach (var actor in Secondary.SpawnAndReturn(max, prefabs, caller))
                    yield return actor;
            }
            else
            {
                if (!isForced && Time.timeSinceLevelLoad - _lastTime < MinInterval)
                    yield break;

                _lastTime = Time.timeSinceLevelLoad;

                int i = 0;

                foreach (var point in Points)
                {
                    if (i >= max)
                        break;

                    i++;
                    yield return SpawnPoint.SpawnPrefab(prefabs[Random.Range(0, prefabs.Length)], point.transform.position, caller);
                }
            }
        }
    }
}
