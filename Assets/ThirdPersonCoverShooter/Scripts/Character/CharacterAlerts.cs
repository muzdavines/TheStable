using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts for the AI to pick up on various character events.
    /// </summary>
    public class CharacterAlerts : MonoBehaviour, ICharacterPhysicsListener, ICharacterHealthListener
    {
        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Step = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Hurt = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Death = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Jump = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Land = 10;

        /// <summary>
        /// Distance at which character's resurrection will be heard.Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which character's resurrection will be heard.Alert is not generated if value is zero or negative.")]
        public float Resurrect = 10;

        private Actor _actor;
        private CharacterMotor _motor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
        }

        /// <summary>
        /// Generates a land alert.
        /// </summary>
        public void OnLand()
        {
            Alerts.Broadcast(transform.position, Land, false, _actor, true);
        }

        /// <summary>
        /// Generates a hurt alert.
        /// </summary>
        public void OnHit(Hit hit)
        {
            Alerts.Broadcast(transform.position, Hurt, true, _actor, true);
        }

        /// <summary>
        /// Generates a step alert.
        /// </summary>
        public void OnFootstep(Vector3 position)
        {
            if (_motor == null || (!_motor.IsCrouching && !_motor.IsInCover))
                Alerts.Broadcast(transform.position, Step, false, _actor, true);
        }

        /// <summary>
        /// Generates a death alert.
        /// </summary>
        public void OnDead()
        {
            Alerts.Broadcast(transform.position, Death, true, _actor, true);
        }

        /// <summary>
        /// No resurrection alert.
        /// </summary>
        public void OnResurrect()
        {
            Alerts.Broadcast(transform.position, Resurrect, false, _actor, true);
        }

        /// <summary>
        /// Generates a jump alert.
        /// </summary>
        public void OnJump()
        {
            Alerts.Broadcast(transform.position, Jump, false, _actor, true);
        }
    }
}
