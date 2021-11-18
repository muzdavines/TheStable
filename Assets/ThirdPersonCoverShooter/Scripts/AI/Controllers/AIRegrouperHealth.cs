using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Triggers a regroup when health falls below a certain threshold.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(AIMovement))]
    [RequireComponent(typeof(CharacterHealth))]
    public class AIRegrouperHealth : AIBaseRegrouper
    {
        /// <summary>
        /// Fraction of health that triggers a regroup.
        /// </summary>
        [Tooltip("Fraction of health that triggers a regroup.")]
        [Range(0, 1)]
        public float Health = 0.4f;

        private bool _wasTriggered;
        private CharacterHealth _health;

        protected override void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            base.Awake();
        }

        private void Update()
        {
            if (_health.Health / _health.MaxHealth <= Health && Brain.Threat != null)
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
