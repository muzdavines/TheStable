using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// A version of a grenade that applies a DamageBuff to objects with CharacterMotor.
    /// </summary>
    public class ToxicGrenade : Grenade
    {
        /// <summary>
        /// How much the targets damage is multiplied while the buff is active.
        /// </summary>
        [Tooltip("How much the targets damage is multiplied while the buff is active.")]
        public float DamageMultiplier = 0.5f;

        /// <summary>
        /// Duration of the damage buff applied to the target.
        /// </summary>
        [Tooltip("Duration of the damage buff applied to the target.")]
        public float Duration = 6;

        public ToxicGrenade()
        {
            CenterDamage = 0;
        }

        /// <summary>
        /// Adds a DamageBuff to the target object, but only if it contains CharacterMotor.
        /// </summary>
        protected override void Apply(GameObject target, Vector3 position, Vector3 normal, float fraction)
        {
            base.Apply(target, position, normal, fraction);

            var motor = target.GetComponent<CharacterMotor>();
            if (motor == null) return;

            var buff = target.GetComponent<DamageBuff>();

            if (buff == null || buff.enabled)
                buff = target.gameObject.AddComponent<DamageBuff>();

            buff.Duration = Duration;
            buff.Multiplier = DamageMultiplier;
            buff.Launch();
        }
    }
}
