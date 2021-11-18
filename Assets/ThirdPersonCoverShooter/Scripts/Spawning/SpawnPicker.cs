using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages a collection of spanw groups. Can override prefabs instantiated inside those. When spawning, selects the most appropriate group based on the player orientation and position.
    /// </summary>
    public class SpawnPicker : MonoBehaviour
    {
        /// <summary>
        /// A list of areas, the best will be chosen to spawn enemies.
        /// </summary>
        [Tooltip("A list of areas, the best will be chosen to spawn enemies.")]
        public SpawnGroup[] Areas;

        /// <summary>
        /// Minimal number of possible spawn points used in a group.
        /// </summary>
        [Tooltip("Minimal number of possible spawn points used in a group.")]
        public int MinCount = 999;

        /// <summary>
        /// Maximum number of possible spawn points used in a group.
        /// </summary>
        [Tooltip("Maximum number of possible spawn points used in a group.")]
        public int MaxCount = 999;

        /// <summary>
        /// Player whose vision and position are used to determine which area is the best for spawning.
        /// </summary>
        [Tooltip("Player whose vision and position are used to determine which area is the best for spawning.")]
        public CharacterMotor Player;

        /// <summary>
        /// An area won't be considered if it is closer than this distance.
        /// </summary>
        [Tooltip("An area won't be considered if it is closer than this distance.")]
        public float MinDistance = 32;

        /// <summary>
        /// Prefabs to use when spawning. Overrides the ones specified in SpawnGroups.
        /// </summary>
        [Tooltip("Prefabs to use when spawning. Overrides the ones specified in SpawnGroups.")]
        public GameObject[] PrefabOverride;

        /// <summary>
        /// Time in seconds before another call to spawn is allowed.
        /// </summary>
        [Tooltip("Time in seconds before another call to spawn is allowed.")]
        public float MinInterval = 6;

        /// <summary>
        /// Another spawn picker to trigger instead by random chance.
        /// </summary>
        [Tooltip("Another spawn picker to trigger instead by random chance.")]
        public SpawnPicker Secondary;

        /// <summary>
        /// Chance that a secondary picker group will be used. Ignored if it not set to any.
        /// </summary>
        [Tooltip("Chance that a secondary picker group will be used. Ignored if it not set to any.")]
        [Range(0, 1)]
        public float SecondaryChance = 0.5f;

        /// <summary>
        /// By default Secondary Chance is evaluated only if Secondary is not none. Enabling this property allows evaluating even if Secondary is none.
        /// </summary>
        [Tooltip("By default Secondary Chance is evaluated only if Secondary is not none. Enabling this property allows evaluating even if Secondary is none.")]
        public bool UseNoneSecondary = false;

        private List<GameObject> _empty = new List<GameObject>();
        private List<SpawnGroup> _fitting = new List<SpawnGroup>();
        private float _lastTime;

        /// <summary>
        /// Picks an area to spawn enemies at.
        /// </summary>
        public void Spawn(Actor caller)
        {
            if ((UseNoneSecondary || Secondary != null) && Random.Range(0f, 1f) <= SecondaryChance)
            {
                if (Secondary != null)
                    Secondary.Spawn(caller);

                return;
            }

            if (Areas == null || Areas.Length == 0)
                return;

            _fitting.Clear();

            if (Player != null)
            {
                var forward = Util.HorizontalVector(Player.BodyAngle);

                foreach (var area in Areas)
                    if (area != null)
                    {
                        if (Vector3.Distance(area.transform.position, Player.transform.position) < MinDistance)
                            continue;

                        var dot = Vector3.Dot(forward, area.transform.position - Player.transform.position);

                        if (dot < 0)
                            _fitting.Add(area);
                    }
            }

            if (_fitting.Count == 0)
                SpawnSpecific(Areas[Random.Range(0, Areas.Length)], caller);
            else
                SpawnSpecific(_fitting[Random.Range(0, _fitting.Count)], caller);
        }

        /// <summary>
        /// Instantiates prefabs on the specific group.
        /// </summary>
        public void SpawnSpecific(SpawnGroup group, Actor caller, bool isForced = false)
        {
            if (group == null)
                return;

            foreach (var actor in SpawnAndReturn(group, caller, isForced)) { }
        }

        /// <summary>
        /// Instantiates and returns prefabs on all spawn points.
        /// </summary>
        public IEnumerable<GameObject> SpawnAndReturn(SpawnGroup group, Actor caller, bool isForced = false)
        {
            if (!isForced && Time.timeSinceLevelLoad - _lastTime < MinInterval)
                return _empty;

            _lastTime = Time.timeSinceLevelLoad;

            if (PrefabOverride == null || PrefabOverride.Length == 0)
                return group.SpawnAndReturn(Random.Range(MinCount, MaxCount), caller, true);
            else
                return group.SpawnAndReturn(Random.Range(MinCount, MaxCount), PrefabOverride, caller, true);
        }
    }
}
