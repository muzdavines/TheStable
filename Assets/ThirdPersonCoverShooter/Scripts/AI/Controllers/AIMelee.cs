using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(BaseBrain))]
    [RequireComponent(typeof(CharacterMotor))]
    public class AIMelee : AIBase
    {
        /// <summary>
        /// Distance at which the AI will try to hit the enemy.
        /// </summary>
        [Tooltip("Distance at which the AI will try to hit the enemy.")]
        public float Distance = 0.8f;

        private BaseBrain _brain;
        private CharacterMotor _motor;

        private void Awake()
        {
            _brain = GetComponent<BaseBrain>();
            _motor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            if (_brain.Threat == null)
                return;

            if (!_brain.CanSeeTheThreat || !_brain.IsActualThreatPosition)
                return;

            if (Vector3.Distance(transform.position, _brain.LastKnownThreatPosition) <= Distance)
            {
                SendMessage("ToTurnAt", _brain.LastKnownThreatPosition);
                _motor.InputMelee(_brain.LastKnownThreatPosition);

                if (_brain.Threat.Motor.IsLow)
                    _motor.InputVerticalMeleeAngle(30);
                else
                    _motor.InputVerticalMeleeAngle(0);
            }
        }
    }
}