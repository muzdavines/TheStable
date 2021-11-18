using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Describes enemy positions for the AI to aim at.
    /// </summary>
    [Serializable]
    public struct ActorTarget
    {
        /// <summary>
        /// Target ground position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Current top position relative to the ground.
        /// </summary>
        public Vector3 RelativeTopPosition;

        /// <summary>
        /// Potential top position relative to the ground.
        /// </summary>
        public Vector3 RelativeStandingTopPosition;

        public ActorTarget(Vector3 position, Vector3 relativeTopPosition, Vector3 relativeStandingTopPosition)
        {
            Position = position;
            RelativeTopPosition = relativeTopPosition;
            RelativeStandingTopPosition = relativeStandingTopPosition;
        }
    }

    /// <summary>
    /// Manages head, body and arm direction for both civilians and fighters.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class AIAim : AIBase
    {
        #region Enums

        private enum BodyMode
        {
            none,
            actor,
            position,
            direction,
            walk,
            scan
        }

        private enum AimMode
        {
            none,
            actor,
            position,
            direction,
            walk,
            scan
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Speed at which the AI turns.
        /// </summary>
        [Tooltip("Speed at which the AI turns.")]
        public float Speed = 6;

        /// <summary>
        /// Speed at which the AI turns when in slow mode.
        /// </summary>
        [Tooltip("Speed at which the AI turns when in slow mode.")]
        public float SlowSpeed = 2;

        /// <summary>
        /// Duration of sweeping in a single direction. Afterwards a new direction is picked.
        /// </summary>
        [Tooltip("Duration of sweeping in a single direction. Afterwards a new direction is picked.")]
        public float MinScanDuration = 3.5f;

        /// <summary>
        /// Duration of sweeping in a single direction. Afterwards a new direction is picked.
        /// </summary>
        [Tooltip("Duration of sweeping in a single direction. Afterwards a new direction is picked.")]
        public float MaxScanDuration = 5;

        /// <summary>
        /// Minimal unobstructed distance in a direction for it to be scanned.
        /// </summary>
        [Tooltip("Minimal unobstructed distance in a direction for it to be scanned.")]
        public float MinScanDistance = 6;

        /// <summary>
        /// Duration of a single sweep.
        /// </summary>
        [Tooltip("Duration of a single sweep.")]
        public float MinSweepDuration = 4;

        /// <summary>
        /// Duration of a single sweep.
        /// </summary>
        [Tooltip("Duration of a single sweep.")]
        public float MaxSweepDuration = 6;

        /// <summary>
        /// How wide is a single sweep in degrees.
        /// </summary>
        [Tooltip("How wide is a single sweep in degrees.")]
        public float SweepFOV = 30;

        /// <summary>
        /// Maximum degrees of error the AI can make when firing.
        /// </summary>
        [Tooltip("Maximum degrees of error the AI can make when firing.")]
        public float AccuracyError = 2;

        /// <summary>
        /// Position of the enemy the AI is aiming at.
        /// </summary>
        [Tooltip("Position of the enemy the AI is aiming at.")]
        public AITargetSettings Target = new AITargetSettings(0.5f, 0.8f);

        /// <summary>
        /// Should a debug rays be displayed.
        /// </summary>
        [Tooltip("Should a debug rays be displayed.")]
        public bool DebugAim = false;

        #endregion

        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private Vector3 _body;
        private Vector3 _aim;
        private ActorTarget _target;
        private bool _hasBodyAim;
        private bool _hasAim;

        private bool _isAimingSlowly;
        private bool _isTurningSlowly;

        private Vector3 _currentAim;

        private BodyMode _bodyMode;
        private AimMode _aimMode;

        private Vector3 _walkDirection;
        private bool _hasWalkDirection;

        private float _aimDelay = 0;

        private float _currentTargetHeight;
        private float _targetHeight;
        private float _targetHeightTime;

        private Vector3 _lastWalkPosition;

        private float _scanLeft;
        private float _scanAngle;
        private float _sweepDelta;
        private int _sweepDirection;
        private float _sweepDuration;
        private float _sweepFOV;

        #endregion

        #region Events

        /// <summary>
        /// Notified of a new walk direction.
        /// </summary>
        public void OnWalkDirection(Vector3 value)
        {
            _walkDirection = value;
            _hasWalkDirection = true;

            if (_aimMode == AimMode.scan)
            {
                var distance = Vector3.Distance(transform.position, _lastWalkPosition);

                if (distance > 1)
                {
                    _lastWalkPosition = transform.position;

                    if (Mathf.Abs(Mathf.DeltaAngle(Util.HorizontalAngle(value), _scanAngle)) > 30)
                        scanAtWalkDirection();
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Told to start scanning the surrounding area.
        /// </summary>
        public void ToScan()
        {
            if (_bodyMode != BodyMode.scan)
                findNewScanDirection();

            _bodyMode = BodyMode.scan;
            _aimMode = AimMode.scan;
            _hasBodyAim = true;
            _hasAim = true;
        }

        /// <summary>
        /// Told to turn both body and arms towards the walk direction.
        /// </summary>
        public void ToFaceWalkDirection()
        {
            _bodyMode = BodyMode.walk;
            _aimMode = AimMode.walk;
            _hasBodyAim = true;
            _hasAim = true;
        }

        /// <summary>
        /// Told to turn the body towards the walk direction.
        /// </summary>
        public void ToTurnToWalkDirection()
        {
            _bodyMode = BodyMode.walk;
            _hasBodyAim = true;
        }

        /// <summary>
        /// Told by the brains to turn the body at a position.
        /// </summary>
        public void ToTurnAt(Vector3 position)
        {
            _body = position;
            _bodyMode = BodyMode.position;
            _hasBodyAim = true;
            _isTurningSlowly = false;

            if (!_hasAim)
                ToAimAt(position);
        }

        /// <summary>
        /// Told by the brains to turn the body to a direection.
        /// </summary>
        public void ToTurnTo(Vector3 direction)
        {
            _body = direction;
            _bodyMode = BodyMode.direction;
            _hasBodyAim = true;
            _isTurningSlowly = false;

            if (!_hasAim)
                ToAimTo(direction);
        }

        /// <summary>
        /// Told by the brains to aim the gun at a position.
        /// </summary>
        public void ToAimAt(Vector3 position)
        {
            aimAt(position);
            _isAimingSlowly = false;
            _aimDelay = 0;
        }

        /// <summary>
        /// Told by the brains to aim the gun to a direction.
        /// </summary>
        public void ToAimTo(Vector3 direction)
        {
            aimTo(direction);
            _isAimingSlowly = false;
            _aimDelay = 0;
        }

        /// <summary>
        /// Told by the brains to aim at an actor.
        /// </summary>
        public void ToTarget(ActorTarget target)
        {
            _target = target;
            _bodyMode = BodyMode.actor;
            _aimMode = AimMode.actor;
            _hasBodyAim = true;
            _hasAim = true;
            _isTurningSlowly = false;
        }

        /// <summary>
        /// Told by the brains to slowly turn the body at a position.
        /// </summary>
        public void ToSlowlyTurnAt(Vector3 position)
        {
            ToTurnAt(position);
            _isTurningSlowly = true;
        }

        /// <summary>
        /// Told by the brains to slowly turn the body to a direction.
        /// </summary>
        public void ToSlowlyTurnTo(Vector3 direction)
        {
            ToTurnTo(direction);
            _isTurningSlowly = true;
        }

        /// <summary>
        /// Told by the brains to slowly aim the gun at a position.
        /// </summary>
        public void ToSlowlyAimAt(Vector3 position)
        {
            aimAt(position);
            _isAimingSlowly = true;
            _aimDelay = 0.5f;
        }

        /// <summary>
        /// Told by the brains to slowly aim the gun to a direction.
        /// </summary>
        public void ToSlowlyAimTo(Vector3 direction)
        {
            aimTo(direction);
            _isAimingSlowly = true;
            _aimDelay = 0.5f;
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
            _walkDirection = transform.position;
            _currentAim = transform.forward;

            _targetHeight = UnityEngine.Random.Range(Target.Min, Target.Max);
            _currentTargetHeight = _targetHeight;
            _targetHeightTime = Time.timeSinceLevelLoad;
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            if (Time.timeSinceLevelLoad - _targetHeightTime > 3)
            {
                _targetHeight = UnityEngine.Random.Range(Target.Min, Target.Max);
                _targetHeightTime = Time.timeSinceLevelLoad;
            }

            Util.Lerp(ref _currentTargetHeight, _targetHeight, 1);

            if (!_hasBodyAim)
                ToTurnTo(transform.forward);

            switch (_bodyMode)
            {
                case BodyMode.actor: aimBodyTo(_target.Position + _target.RelativeTopPosition * _currentTargetHeight, _isTurningSlowly ? SlowSpeed : Speed); break;
                case BodyMode.position: aimBodyTo(_body, _isTurningSlowly ? SlowSpeed : Speed); break;
                case BodyMode.direction: aimBodyTo(transform.position + _body * 8, _isTurningSlowly ? SlowSpeed : Speed); break;
                case BodyMode.walk: aimBodyTo(transform.position + _walkDirection * 8, (_isTurningSlowly ? SlowSpeed : Speed) * 2); break;

                case BodyMode.scan:
                    _scanLeft -= Time.deltaTime;

                    if (_scanLeft < float.Epsilon)
                        findNewScanDirection();

                    aimBodyTo(transform.position + Util.HorizontalVector(_scanAngle) * 100, Speed);

                    if (DebugAim)
                        Debug.DrawLine(transform.position, _motor.BodyLookTarget, Color.black);
                    break;
            }

            switch (_aimMode)
            {
                case AimMode.actor: turn(ref _currentAim, _target.Position + _target.RelativeTopPosition * _currentTargetHeight, _isAimingSlowly); break;
                case AimMode.position: turn(ref _currentAim, _aim, _isAimingSlowly); break;
                case AimMode.direction: turn(ref _currentAim, transform.position + _aim * 8, _isAimingSlowly); break;
                case AimMode.walk: turn(ref _currentAim, transform.position + _walkDirection * 8, _isAimingSlowly); break;

                case AimMode.scan:
                    var angle = _scanAngle + (_sweepDelta - 0.5f) * _sweepFOV;
                    var target = transform.position + Util.HorizontalVector(angle) * 100;

                    if (Mathf.Abs(Mathf.DeltaAngle(Util.HorizontalAngle(_currentAim - transform.position), angle)) > 20)
                        turn(ref _currentAim, target, false);
                    else
                        _currentAim = target;

                    _sweepDelta += _sweepDirection * Time.deltaTime / _sweepDuration;

                    if (_sweepDelta >= 1 && _sweepDirection > 0) _sweepDirection = -1;
                    else if (_sweepDelta <= -1 && _sweepDirection < 0) _sweepDirection = 1;

                    _motor.InputAim();
                    break;
            }

            aimMotorAt(_currentAim);

            if (_aimDelay >= 0)
                _aimDelay -= Time.deltaTime;
        }

        #endregion

        #region Helpers

        private void aimBodyTo(Vector3 position, float speed)
        {
            if (!_hasWalkDirection)
            {
                _motor.SetBodyTarget(position, speed);
                return;
            }

            var axis = Util.HorizontalAngle(_walkDirection);
            var angle = Util.HorizontalAngle(position - transform.position);

            for (int i = 0; i < _snaps.Length; i++)
                _snapWork[i] = Mathf.Abs(Mathf.DeltaAngle(axis + _snaps[i], angle));

            for (int i = 0; i < _snaps.Length; i++)
            {
                var isOk = true;

                for (int j = i + 1; j < _snaps.Length; j++)
                    if (_snapWork[j] < _snapWork[i])
                    {
                        isOk = false;
                        break;
                    }

                if (isOk)
                {
                    angle = axis + _snaps[i];
                    break;
                }
            }

            _motor.SetBodyTarget(transform.position + Util.HorizontalVector(angle) * 10, speed);
        }

        private float[] _snapWork = new float[_snaps.Length];
        private static float[] _snaps = new float[] { 0, -90, 90, 180 };

        private void scanAtWalkDirection()
        {
            _scanAngle = Util.HorizontalAngle(_walkDirection);
            _scanLeft = UnityEngine.Random.Range(MinScanDuration, MaxScanDuration);

            _sweepDuration = UnityEngine.Random.Range(MinSweepDuration, MaxSweepDuration);
            _sweepDelta = 0;
            _sweepFOV = 0;
        }

        private void findNewScanDirection()
        {
            _scanAngle = Util.RandomUnobstructedAngle(gameObject, transform.position + Vector3.up * 1.5f, _scanAngle, SweepFOV, MinScanDistance);
            _scanLeft = UnityEngine.Random.Range(MinScanDuration, MaxScanDuration);

            var delta = Mathf.DeltaAngle(_scanAngle, Util.HorizontalAngle(_currentAim));

            if (delta < 0)
                _sweepDirection = 1;
            else
                _sweepDirection = -1;

            _sweepDuration = UnityEngine.Random.Range(MinSweepDuration, MaxSweepDuration);
            _sweepDelta = Mathf.Clamp01((delta - 0.5f) / SweepFOV);
            _sweepFOV = SweepFOV;
        }

        public void aimAt(Vector3 position)
        {
            _aim = position;
            _aimMode = AimMode.position;
            _hasAim = true;

            if (!_hasBodyAim)
                ToTurnAt(position);
        }

        public void aimTo(Vector3 direction)
        {
            _aim = direction;
            _aimMode = AimMode.direction;
            _hasAim = true;

            if (!_hasBodyAim)
                ToTurnTo(direction);
        }

        private void turn(ref Vector3 current, Vector3 target, bool isSlow, float multiplier = 1.0f)
        {
            if (_motor.IsInCover && !_motor.IsAimingGun && !_motor.IsAimingTool)
                current = target;
            else if (_aimDelay <= float.Epsilon)
            {
                var speed = (isSlow ? SlowSpeed : Speed) * multiplier;

                var currentVector = current - transform.position;
                var targetVector = target - transform.position;

                var currentHorizontalAngle = Util.HorizontalAngle(currentVector);
                var targetHorizontalAngle = Util.HorizontalAngle(targetVector);

                var currentVerticalAngle = Util.VerticalAngle(currentVector);
                var targetVerticalAngle = Util.VerticalAngle(targetVector);

                var move = speed * Time.deltaTime * 60;
                var horizontalDelta = Mathf.DeltaAngle(currentHorizontalAngle, targetHorizontalAngle);
                var verticalDelta = Mathf.DeltaAngle(currentVerticalAngle, targetVerticalAngle);

                var travel = horizontalDelta + verticalDelta;

                Vector3 vector;

                if (travel > float.Epsilon)
                {
                    var t = move / travel;
                    if (t > 1)
                        t = 1;
                    vector = Vector3.Slerp(currentVector, targetVector, speed);
                }
                else
                    vector = targetVector;

                current = transform.position + vector;
            }

            if (DebugAim)
            {
                Debug.DrawLine(transform.position, current, Color.magenta);
                Debug.DrawLine(transform.position, target, Color.green);
            }
        }

        private void aimMotorAt(Vector3 position)
        {
            if (_motor.IsInTallCover)
            {
                var vector = position - transform.position;

                if (Vector3.Dot(_motor.Cover.Forward, vector) > 0)
                {
                    var isNearLeft = _motor.IsNearLeftCorner;
                    var isNearRight = _motor.IsNearRightCorner;

                    if (isNearLeft && isNearRight)
                    {
                        if (Vector3.Dot(_motor.Cover.Left, vector) > 0)
                        {
                            position = transform.position + (_motor.Cover.Forward + _motor.Cover.Left).normalized * 8;
                            _motor.InputStandLeft();
                        }
                        else
                        {
                            position = transform.position + (_motor.Cover.Forward + _motor.Cover.Right).normalized * 8;
                            _motor.InputStandRight();
                        }
                    }
                    else if (isNearLeft)
                    {
                        position = transform.position + (_motor.Cover.Forward + _motor.Cover.Left).normalized * 8;
                        _motor.InputStandLeft();
                    }
                    else
                    {
                        position = transform.position + (_motor.Cover.Forward + _motor.Cover.Right).normalized * 8;
                        _motor.InputStandRight();
                    }
                }
            }

            _motor.SetAimTarget(position);

            if (DebugAim)
                Debug.DrawLine(_motor.GunOrigin, position, Color.red);

            var gun = _motor.EquippedWeapon.Gun;

            if (gun != null)
                gun.AddErrorThisFrame(AccuracyError);
        }

        #endregion
    }
}
