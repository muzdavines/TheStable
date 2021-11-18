using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Type of damage dealt. Relative means relative to MaxHealth. Constant is always the same no matter the character that triggers.
    /// </summary>
    public enum DamageType
    {
        Relative,
        Constant
    }

    /// <summary>
    /// Deals damage to Character Health components attached to objects that enter its trigger area.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DamageTrigger : MonoBehaviour
    {
        /// <summary>
        /// Type of damage dealt. Relative means relative to MaxHealth. Constant is always the same no matter the character that triggers.
        /// </summary>
        [Tooltip("Type of damage dealt. Relative means relative to MaxHealth. Constant is always the same no matter the character that triggers.")]
        public DamageType Type = DamageType.Relative;

        /// <summary>
        /// Amount of damage. When damage type is set to relative this is a fraction of MaxHealth.
        /// </summary>
        [Tooltip("Amount of damage. When damage type is set to relative this is a fraction of MaxHealth.")]
        public float Damage = 0.3f;

        /// <summary>
        /// Is shut down after the first trigger.
        /// </summary>
        [Tooltip("Is shut down after the first trigger.")]
        public bool OnlyOnce = false;

        /// <summary>
        /// Sound to be played on trigger.
        /// </summary>
        [Tooltip("Sound to be played on trigger.")]
        public AudioClip Sound;

        private bool _wasTriggered = false;

        private void OnEnable()
        {
            _wasTriggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_wasTriggered && OnlyOnce)
                return;

            var health = other.GetComponent<CharacterHealth>();

            if (health != null)
            {
                if (Type == DamageType.Constant)
                    health.Deal(Damage);
                else if (Type == DamageType.Relative)
                    health.Deal(Damage * health.MaxHealth);

                if (Sound != null)
                    AudioSource.PlayClipAtPoint(Sound, transform.position);

                _wasTriggered = true;
            }
        }
    }
}
