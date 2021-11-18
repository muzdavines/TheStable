using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts for the AI to pick up on various melee events.
    /// </summary>
    [RequireComponent(typeof(BaseMelee))]
    public class MeleeAlerts : MonoBehaviour, IMeleeListener
    {
        /// <summary>
        /// Distance at which any attack can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which any attack can be heard. Alert is not generated if value is zero or negative.")]
        public float Attack = 0;

        /// <summary>
        /// Distance at which a hit can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which a hit can be heard. Alert is not generated if value is zero or negative.")]
        public float Hit = 20;

        /// <summary>
        /// Distance at which a melee moment can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which a melee moment can be heard. Alert is not generated if value is zero or negative.")]
        public float Moment = 0;

        private BaseMelee _melee;
        private Actor _actor;
        private CharacterMotor _cachedMotor;

        private void Awake()
        {
            _melee = GetComponent<BaseMelee>();
        }

        /// <summary>
        /// Generates a melee attack alert.
        /// </summary>
        public void OnMeleeAttack()
        {
            if (Attack <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Attack, true, _actor, true);
        }

        /// <summary>
        /// Generates a melee hit alert.
        /// </summary>
        public void OnMeleeHit()
        {
            if (Hit <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Hit, true, _actor, true);
        }

        /// <summary>
        /// Generates a melee moment alert.
        /// </summary>
        public void OnMeleeMoment()
        {
            if (Moment <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Moment, true, _actor, true);
        }

        private void checkActor()
        {
            if (_melee.Character != _cachedMotor)
            {
                _cachedMotor = _melee.Character;
                _actor = _cachedMotor.GetComponent<Actor>();
            }
        }
    }
}
