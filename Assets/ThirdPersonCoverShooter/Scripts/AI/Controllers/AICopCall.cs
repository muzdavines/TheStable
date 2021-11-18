using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Creates a call event when the AI sees an enemy.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AICopCall : AIBase
    {
        #region Public fields

        /// <summary>
        /// A chance that a call will be made.
        /// </summary>
        [Tooltip("A chance that a call will be made.")]
        [Range(0, 1)]
        public float CallChance = 0.7f;

        /// <summary>
        /// Can the civilian make a call while fleeing the scene.
        /// </summary>
        [Tooltip("Can the civilian make a call while fleeing the scene.")]
        public bool CanCallWhenFleeing = true;

        /// <summary>
        /// Will the AI flee after making a call.
        /// </summary>
        [Tooltip("Will the civilian flee after making a call.")]
        public bool FleeAfterCall = true;

        /// <summary>
        /// Time in seconds after being alerted to make a call.
        /// </summary>
        [Tooltip("Time in seconds after being alerted to make a call.")]
        public float MinCallDelay = 4.0f;

        /// <summary>
        /// Time in seconds after being alerted to make a call.
        /// </summary>
        [Tooltip("Time in seconds after being alerted to make a call.")]
        public float MaxCallDelay = 5.0f;

        /// <summary>
        /// Target object and a message sent during a call.
        /// </summary>
        [Tooltip("Target object and a message sent during a call.")]
        public AICall Call = AICall.Default();

        #endregion

        #region Private fields

        private Actor _actor;

        private bool _isScared;

        private bool _isWaitingForCall;
        private bool _willMakeCall;

        private float _delay;

        #endregion

        #region Commands

        /// <summary>
        /// Registers that the AI is fleeing.
        /// </summary>
        public void ToBecomeScared()
        {
            if (!CanCallWhenFleeing)
                _willMakeCall = false;

            _isScared = true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Registers an event that the call animation has finished and executes the script.
        /// </summary>
        public void OnCallMade()
        {
            if (_isWaitingForCall && isActiveAndEnabled)
            {
                _isWaitingForCall = false;
                Call.Make(_actor);
                Message("OnCopCall");

                if (FleeAfterCall)
                    Message("ToBecomeScared");
            }
        }

        /// <summary>
        /// Registers the fact that the AI has seen an enemy or is fleeing. Makes a random call check.
        /// </summary>
        public void OnAlarmed()
        {
            if (_isScared && !CanCallWhenFleeing)
                return;

            _willMakeCall = Random.Range(0f, 1f) <= CallChance;

            if (_willMakeCall)
                _delay = Random.Range(MinCallDelay, MaxCallDelay);
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            if (_willMakeCall)
            {
                if (_delay <= float.Epsilon)
                {
                    _willMakeCall = false;
                    _isWaitingForCall = true;
                    Message("ToMakeCall");
                }
                else
                    _delay -= Time.deltaTime;
            }
        }

        #endregion
    }
}
