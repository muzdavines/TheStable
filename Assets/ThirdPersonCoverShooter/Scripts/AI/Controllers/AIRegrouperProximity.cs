using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Triggers a regroup when the enemy is close enough.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(AIMovement))]
    public class AIRegrouperProximity : AIBaseRegrouper
    {
        /// <summary>
        /// Distance to the enemy that triggers a regroup.
        /// </summary>
        [Tooltip("Distance to the enemy that triggers a regroup.")]
        [Range(0, 1)]
        public float Distance = 20;

        private bool _wasTriggered;

        private void OnThreatPosition(Vector3 value)
        {
            if (Vector3.Distance(transform.position, value) <= Distance)
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
