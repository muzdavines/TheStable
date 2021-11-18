using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Sets the AI to attack enemies that attack the Target. Requires AIThreatControl to function.
    /// </summary>
    public class AIProtector : AIBase
    {
        /// <summary>
        /// Supported actor.
        /// </summary>
        [Tooltip("Supported actor.")]
        public Actor Target;

        private Actor _previousTarget;

        private void OnEnable()
        {
            Message("ToSetThreatReference", Target);
            _previousTarget = Target;
        }

        private void Update()
        {
            if (Target != _previousTarget)
            {
                Message("ToSetThreatReference", Target);
                _previousTarget = Target;
            }
        }
    }
}
