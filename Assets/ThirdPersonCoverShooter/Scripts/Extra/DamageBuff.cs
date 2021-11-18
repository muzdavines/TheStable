using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Multiplies damage dealt by the character's weapons.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class DamageBuff : BaseBuff
    {
        /// <summary>
        /// Damage multiplier for each weapon.
        /// </summary>
        [Tooltip("Damage multiplier for each weapon.")]
        public float Multiplier = 2;

        private float _multiplier;
        private float _original;
        private CharacterMotor _motor;

        public DamageBuff()
        {
            Outline = true;
            OutlineColor = Color.red;
        }

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        protected override void Begin()
        {
            _multiplier = Multiplier;
            _original = _motor.DamageMultiplier;
            _motor.DamageMultiplier *= Multiplier;
        }

        protected override void End()
        {
            if (_multiplier > float.Epsilon || _multiplier < -float.Epsilon)
                _motor.DamageMultiplier /= _multiplier;
            else
                _motor.DamageMultiplier = _original;
        }
    }
}
