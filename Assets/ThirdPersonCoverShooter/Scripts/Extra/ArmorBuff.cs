using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Multiplies received damage by CharacterHealth.
    /// </summary>
    [RequireComponent(typeof(CharacterHealth))]
    public class ArmorBuff : BaseBuff
    {
        /// <summary>
        /// Incoming damage multiplier.
        /// </summary>
        [Tooltip("Incoming damage multiplier.")]
        public float Multiplier = 0.5f;

        private CharacterHealth _health;
        private float _applied;
        private float _previous;

        public ArmorBuff()
        {
            Outline = true;
            OutlineColor = Color.blue;
        }

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
        }

        protected override void Begin()
        {
            _applied = Multiplier;
            _previous = _health.DamageMultiplier;
            _health.DamageMultiplier *= Multiplier;
        }

        protected override void End()
        {
            if (_applied < -float.Epsilon || _applied > float.Epsilon)
                _health.DamageMultiplier /= _applied;
            else
                _health.DamageMultiplier = _previous;
        }
    }
}
