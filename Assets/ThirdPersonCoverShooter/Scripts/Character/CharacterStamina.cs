using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Maintains character health.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterStamina : MonoBehaviour, ICharacterWalkListener, ICharacterHealthListener
    {
        /// <summary>
        /// Current health of the character.
        /// </summary>
        [Tooltip("Current health of the character.")]
        public float Stamina = 100f;

        /// <summary>
        /// Max health to regenerate to.
        /// </summary>
        [Tooltip("Max health to regenerate to.")]
        public float MaxStamina = 100f;

        /// <summary>
        /// Amount of health regenerated per second.
        /// </summary>
        [Tooltip("Amount of health regenerated per second.")]
        public float Regeneration = 30f;

        /// <summary>
        /// Stamina consumed per second while running.
        /// </summary>
        [Tooltip("Stamina consumed per second while running.")]
        public float RunCost = 20;

        /// <summary>
        /// Stamina that has to be regenerated before the ability to run returns.
        /// </summary>
        [Tooltip("Stamina that has to be regenerated before the ability to run returns.")]
        public float RunTrigger = 20;

        /// <summary>
        /// Stamina consumed per second while running.
        /// </summary>
        [Tooltip("Stamina consumed per second while sprinting.")]
        public float SprintCost = 50;

        /// <summary>
        /// Stamina that has to be regenerated before the ability to sprint returns.
        /// </summary>
        [Tooltip("Stamina that has to be regenerated before the ability to sprint returns.")]
        public float SprintTrigger = 50;

        private bool _isDead;
        private CharacterMotor _motor;
        private float _consumption;

        private void OnValidate()
        {
            Stamina = Mathf.Max(0, Stamina);
            MaxStamina = Mathf.Max(0, MaxStamina);
        }

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        public void OnStop()
        {
            _consumption = 0;
        }

        /// <summary>
        /// Character is no longer running, stop stamina consumption.
        /// </summary>
        public void OnWalk()
        {
            _consumption = 0;
        }

        /// <summary>
        /// Character is running, set frame by frame stamina consumption to RunCost.
        /// </summary>
        public void OnRun()
        {
            _consumption = RunCost;
        }

        /// <summary>
        /// Character is sprinting, set frame by frame stamina consumption to SprintCost.
        /// </summary>
        public void OnSprint()
        {
            _consumption = SprintCost;
        }

        /// <summary>
        /// Catch the death event and stop stamina regeneration.
        /// </summary>
        public void OnDead()
        {
            _isDead = true;
        }

        /// <summary>
        /// Catch the resurrection and set the state as 'not dead', enabling stamina regeneration.
        /// </summary>
        public void OnResurrect()
        {
            _isDead = false;
        }

        /// <summary>
        /// Reduce stamina by the given amount. If the reduction was possible, return true, signaling that an action can be performed.
        /// </summary>
        public bool Take(float amount)
        {
            if (Stamina > amount)
            {
                Stamina -= amount;

                return true;
            }
            else
            {
                Stamina = 0;
                _motor.CanRun = false;

                return false;
            }
        }

        private void LateUpdate()
        {
            if (!_isDead)
            {
                if (_consumption > float.Epsilon)
                    Take(_consumption * Time.deltaTime);

                Stamina = Mathf.Clamp(Stamina + Regeneration * Time.deltaTime, 0, MaxStamina);

                if (!_motor.CanRun) _motor.CanRun = Stamina >= RunTrigger;
                if (!_motor.CanSprint) _motor.CanSprint = Stamina >= SprintTrigger;
            }
        }
    }
}