using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Triggers a regroup when the number of friends falls below a threshold.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(AIMovement))]
    [RequireComponent(typeof(AICommunication))]
    public class AIRegrouperCompany : AIBaseRegrouper
    {
        /// <summary>
        /// Regroup is triggered if number of friends is equal or below the value.
        /// </summary>
        [Tooltip("Regroup is triggered if number of friends is equal or below the value.")]
        public int TriggerCount = 0;

        private bool _wasTriggered;
        private AICommunication _comm;

        protected override void Awake()
        {
            _comm = GetComponent<AICommunication>();
            base.Awake();
        }

        private void Update()
        {
            if (_comm.FriendCount <= TriggerCount && Brain.HasSeenTheEnemy)
            {
                if (!_wasTriggered)
                {
                    _wasTriggered = true;
                    Regroup();
                }
            }
            else
                _wasTriggered = false;
        }
    }
}
