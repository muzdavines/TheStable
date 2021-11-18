using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Makes the AI to reset when the enemy is too far away.
    /// </summary>
    public class AILeave : AIBase
    {
        /// <summary>
        /// Distance to the enemy that triggers AI going back to it's initial state.
        /// </summary>
        [Tooltip("Distance to the enemy that triggers AI going back to it's initial state.")]
        public float Distance = 60;

        private Actor _threat;

        /// <summary>
        /// Registers the new threat actor.
        /// </summary>
        private void OnThreat(Actor actor)
        {
            _threat = actor;
        }

        /// <summary>
        /// Registers that there is no threat currently.
        /// </summary>
        public void OnNoThreat()
        {
            _threat = null;
        }

        private void Update()
        {
            if (_threat != null)
                if (Vector3.Distance(transform.position, _threat.transform.position) >= Distance - float.Epsilon)
                    Message("ToForget");
        }
    }
}
