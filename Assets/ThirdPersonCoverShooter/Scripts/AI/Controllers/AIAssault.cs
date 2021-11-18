using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Makes the AI run towards the enemy as an assault.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(BaseBrain))]
    public class AIAssault : AIBase, ICharacterSuccessfulHitListener
    {
        #region Public fields

        /// <summary>
        /// Distance at which the AI stops it's assault if attacking to fire.
        /// </summary>
        [Tooltip("Distance at which the AI stops it's assault if attacking to fire.")]
        public float NonMeleeDistance = 8;

        /// <summary>
        /// Distance at which the AI will start to try and hit the enemy.
        /// </summary>
        [Tooltip("Distance at which the AI will start to try and hit the enemy.")]
        public float MaxMeleeDistance = 1.5f;

        /// <summary>
        /// Distance which the AI tries to maintain against the enemy.
        /// </summary>
        [Tooltip("Distance which the AI tries to maintain against the enemy.")]
        public float MinMeleeDistance = 1.0f;

        /// <summary>
        /// Successful melee hits required before the assault is stopped.
        /// </summary>
        [Tooltip("Successful melee hits required before the assault is stopped.")]
        public int MeleeHits = 3;

        /// <summary>
        /// Maximum time in seconds the AI will perform the assault before moving on to other tactics.
        /// </summary>
        [Tooltip("Maximum time in seconds the AI will perform the assault before moving on to other tactics.")]
        public float MaxDuration = 20;

        /// <summary>
        /// Chance the AI takes cover after assaulting an enemy.
        /// </summary>
        [Tooltip("Chance the AI takes cover after assaulting an enemy.")]
        [Range(0, 1)]
        public float TakeCoverChance = 0.5f;

        /// <summary>
        /// Should the AI attack the enemy with melee. If there are no it will approach and fire from close range.
        /// </summary>
        [Tooltip("Should the AI attack the enemy with melee. If there are no it will approach and fire from close range.")]
        public bool UseMeleeIfPossible = true;

        /// <summary>
        /// Minimum time the AI wait blocking before an attack.
        /// </summary>
        [Tooltip("Minimum time the AI wait blocking before an attack.")]
        public float MinBlockDuration = 2;

        /// <summary>
        /// Maximum time the AI will block before attacking, even if the enemy is attacking as well.
        /// </summary>
        [Tooltip("Maximum time the AI will block before attacking, even if the enemy is attacking as well.")]
        public float MaxBlockDuration = 6;

        /// <summary>
        /// Amount of time before a melee attack to not block incoming melee attacks.
        /// </summary>
        [Tooltip("Amount of time before a melee attack to not block incoming melee attacks.")]
        public float PreHitFreezeDuration = 0.5f;

        /// <summary>
        /// Amount of time after a melee attack to not block incoming melee attacks.
        /// </summary>
        [Tooltip("Amount of time after a melee attack to not block incoming melee attacks.")]
        public float PostHitFreezeDuration = 0.5f;

        #endregion

        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;
        private BaseBrain _brain;

        private bool _isAssaulting;
        private Vector3 _targetPosition;
        private Vector3 _threatPosition;

        private bool _isKeepingCloseTo;
        private KeepCloseTo _keepCloseTo;

        private int _hits = 0;
        private float _wait = 0;

        private bool _wasInMeleeRange;
        private bool _wasEverInMeleeRange;
        private bool _isInMelee;

        private float _blockWait = 0;
        private float _postHitTime = 0;
        private float _preHitTime = 0;
        private bool _isGoingToHit = false;

        #endregion

        #region Events

        /// <summary>
        /// Responds with an answer to a brain enquiry.
        /// </summary>
        public void AssaultCheck(Vector3 position)
        {
            if (!isActiveAndEnabled)
                return;

            if (isMelee)
                Message("AssaultResponse");
            else if (isGun && Vector3.Distance(transform.position, position) > NonMeleeDistance)
                Message("AssaultResponse");
        }

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        public void OnThreatPosition(Vector3 position)
        {
            if (!_isAssaulting || !isActiveAndEnabled)
                return;

            _threatPosition = position;

            if (Vector3.Distance(position, _targetPosition) > 0.5f)
            {
                _targetPosition = position;

                if (AIUtil.GetClosestStandablePosition(ref position))
                    Message("ToRunTo", position);
                else
                    OnPositionUnreachable(position);
            }
        }

        /// <summary>
        /// Notified that the target position is unreachable.
        /// </summary>
        public void OnPositionUnreachable(Vector3 position)
        {
            if (_isAssaulting && Vector3.Distance(transform.position, position) > MaxMeleeDistance)
                ToStopAssault();
        }

        /// <summary>
        /// Notified of a successful melee hit.
        /// </summary>
        public void OnSuccessfulHit(Hit hit)
        {
            if (!hit.IsMelee)
                return;

            var target = Actors.Get(hit.Target);

            if (target == null)
                return;

            if (target.Side == _actor.Side)
                return;

            _hits++;

            if (_hits >= MeleeHits)
                ToStopAssault();
        }

        #endregion

        #region Commands

        public void ToKeepCloseTo(KeepCloseTo value)
        {
            _isKeepingCloseTo = true;
            _keepCloseTo = value;
        }

        /// <summary>
        /// Commanded to start an assault towards a threat position.
        /// </summary>
        public void ToStartAssault(Vector3 position)
        {
            _targetPosition = position;

            if (!isActiveAndEnabled)
                return;

            var wasAssaulting = _isAssaulting;

            if (isMelee || Vector3.Distance(transform.position, position) > NonMeleeDistance)
            {
                _hits = 0;
                _isAssaulting = true;
                _wait = 0;
                _wasInMeleeRange = false;
                _wasEverInMeleeRange = false;
                _isGoingToHit = false;
                _preHitTime = 0;
                _postHitTime = 0;
                runTo(position);

                if (!wasAssaulting)
                    Message("OnAssaultStart");
            }
        }

        /// <summary>
        /// Commanded to stop an assault.
        /// </summary>
        public void ToStopAssault()
        {
            if (_isAssaulting)
            {
                _hits = 0;
                _isAssaulting = false;
                Message("ToStopMoving");

                if (Random.Range(0f, 1f) <= TakeCoverChance)
                    Message("ToFindCover");

                Message("OnAssaultStop");
            }
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
            _brain = GetComponent<BaseBrain>();
        }

        private void Update()
        {
            if (!_isAssaulting)
                return;

            _wait += Time.deltaTime;

            if (_wait >= MaxDuration)
            {
                ToStopAssault();
            }
            else if (isMelee)
            {
                if (_motor.IsPerformingMelee)
                {
                    if (_brain.Threat != null)
                    {
                        var vector = _brain.Threat.transform.position - transform.position;
                        var distance = vector.magnitude;

                        if (distance <= MaxMeleeDistance)
                            _motor.InputCombo(_brain.Threat.transform.position);
                    }

                    _postHitTime = PostHitFreezeDuration;
                    _blockWait = 0;
                }
                else
                {
                    if (_wasEverInMeleeRange)
                        _blockWait += Time.deltaTime;

                    if (_isGoingToHit)
                        _preHitTime -= Time.deltaTime;
                    else if (_postHitTime > 0)
                        _postHitTime -= Time.deltaTime;

                   var isInRange = false;

                    const float minMovementDuration = 0.2f;

                    if (_brain.Threat != null)
                    {
                        var vector = _brain.Threat.transform.position - transform.position;
                        var distance = vector.magnitude;
                        var direction = vector.normalized;

                        const float MaxThreshold = 1;

                        var targetIsLow = _brain.Threat.Motor == null ? false : _brain.Threat.Motor.IsLow;

                        if (_wasInMeleeRange && distance <= MaxMeleeDistance + MaxThreshold)
                        {
                            if (_isGoingToHit)
                            {
                                if (_preHitTime <= float.Epsilon)
                                {
                                    if (targetIsLow && distance < MinMeleeDistance && _motor.IsFreeToMove(-direction))
                                    {
                                        if (distance < MinMeleeDistance * 0.35f)
                                            _motor.InputMovement(new CharacterMovement(-direction, 1, minMovementDuration));
                                        else
                                            _motor.InputMovement(new CharacterMovement(-direction, 0.5f, minMovementDuration));
                                    }
                                    else
                                    {
                                        _isGoingToHit = false;
                                        _motor.InputMelee(_brain.Threat.transform.position);
                                    }
                                }
                            }
                            else if (_postHitTime <= float.Epsilon)
                                _motor.InputBlock();
                        }

                        if (distance <= MaxMeleeDistance)
                        {
                            if (targetIsLow)
                                _motor.InputVerticalMeleeAngle(30);
                            else
                                _motor.InputVerticalMeleeAngle(0);

                            isInRange = true;
                            SendMessage("ToTurnAt", _brain.Threat.transform.position);

                            if (distance < MinMeleeDistance && _motor.IsFreeToMove(-direction))
                            {
                                if (distance < MinMeleeDistance * 0.35f)
                                    _motor.InputMovement(new CharacterMovement(-direction, 1, minMovementDuration));
                                else
                                    _motor.InputMovement(new CharacterMovement(-direction, 0.5f, minMovementDuration));
                            }
                            else
                            {
                                var wasGoingToHit = _isGoingToHit;

                                if (_blockWait > MaxBlockDuration)
                                    _isGoingToHit = true;
                                else if (_blockWait > MinBlockDuration)
                                {
                                    var other = Characters.Get(_brain.Threat.gameObject);

                                    if (other.Motor == null || !other.Motor.IsPerformingMelee)
                                        _isGoingToHit = true;
                                }

                                if (_isGoingToHit && !wasGoingToHit)
                                    _preHitTime = PreHitFreezeDuration;
                            }

                            SendMessage("ToStopMoving");
                        }
                        else if (_wasInMeleeRange && distance < MaxMeleeDistance + MaxThreshold)
                        {
                            if (_postHitTime < 0)
                                _motor.InputBlock();

                            isInRange = true;
                            _motor.InputMovement(new CharacterMovement(vector / distance, 0.5f, minMovementDuration));
                        }
                        else if (distance > MaxMeleeDistance * 10)
                            _wasEverInMeleeRange = false;
                    }

                    if (isInRange)
                    {
                        _wasEverInMeleeRange = true;
                        _wasInMeleeRange = true;
                    }
                    else
                    {
                        _motor.InputVerticalMeleeAngle(0);

                        if (_wasInMeleeRange)
                        {
                            _isGoingToHit = false;
                            _blockWait = 0;
                            _wasInMeleeRange = false;
                            _targetPosition = _threatPosition;
                            runTo(_targetPosition);
                        }
                    }
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, _targetPosition) <= NonMeleeDistance)
                    ToStopAssault();
            }
        }

        private void runTo(Vector3 position)
        {
            if (_isKeepingCloseTo && Vector3.Distance(position, _keepCloseTo.Position) > _keepCloseTo.Distance)
                position = _keepCloseTo.Position + (position - _keepCloseTo.Position).normalized * _keepCloseTo.Distance;

            if (AIUtil.GetClosestStandablePosition(ref position))
                Message("ToRunTo", position);
            else
                OnPositionUnreachable(position);
        }

        #endregion

        #region Private properties

        private bool isMelee
        {
            get { return _motor.Weapon.HasMelee; }
        }

        private bool isGun
        {
            get { return _motor.Weapon.Gun != null; }
        }

        #endregion
    }
}
