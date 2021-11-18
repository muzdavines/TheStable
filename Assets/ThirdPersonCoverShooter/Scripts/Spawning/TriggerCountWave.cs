using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// A version of SpawnGroup that triggers when a certain team has less members than specified.
    /// </summary>
    public class TriggerCountWave : SpawnGroup
    {
        /// <summary>
        /// Team whose members are counted to check the trigger.
        /// </summary>
        [Tooltip("Team whose members are counted to check the trigger.")]
        public int TriggerSide = 0;

        /// <summary>
        /// Number of actors left to trigger a new wave.
        /// </summary>
        [Tooltip("Number of AIs left to trigger a new wave.")]
        public int TriggerCount = 0;

        /// <summary>
        /// Time in seconds to wait before spawning a new wave.
        /// </summary>
        [Tooltip("Time in seconds to wait before spawning a new wave.")]
        public float TriggerDelay = 1.0f;

        private float _buildup = 0;

        private void Update()
        {
            int count = 0;

            foreach (var actor in Actors.All)
                if (actor.IsAlive && actor.Side == TriggerSide && actor.isActiveAndEnabled && actor.IsAlive)
                    count++;

            if (count <= TriggerCount)
            {
                _buildup += Time.deltaTime;

                if (_buildup >= TriggerDelay)
                {
                    Spawn(null);
                    _buildup = 0;
                }
            }
            else
                _buildup = 0;
        }
    }
}
