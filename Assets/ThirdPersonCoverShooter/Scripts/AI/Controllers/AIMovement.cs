using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Takes movement commands from other components and uses pathfinding to walk the character motor towards a destination.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class AIMovement : AIBase
    {
        #region Properties

        /// <summary>
        /// Position the AI is walking towards. Returns current position when circling or retreating.
        /// </summary>
        public Vector3 Destination
        {
            get
            {
                if (_mode == Mode.toPosition)
                    return _target;
                else
                    return transform.position;
            }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// The AI will roll if it is ordered to sprint from a position closer than this property.
        /// </summary>
        [Tooltip("The AI will roll if it is ordered to sprint from a position closer than this property.")]
        public float RollTriggerDistance = 2;

        /// <summary>
        /// Seconds to wait before changing the direction of circling.
        /// </summary>
        [Tooltip("Seconds to wait before changing the direction of circling.")]
        public float CircleDelay = 2;

        /// <summary>
        /// The AI will move out of the way of other actors that are closer than the threshold.
        /// </summary>
        [Tooltip("The AI will move out of the way of other actors that are closer than the threshold.")]
        public float ObstructionRadius = 1.5f;

        /// <summary>
        /// Should a line to destination be drawn in the editor.
        /// </summary>
        [Tooltip("Should a line to destination be drawn in the editor.")]
        public bool DebugDestination = false;

        /// <summary>
        /// Should a path to destination be drawn in the editor.
        /// </summary>
        [Tooltip("Should a path to destination be drawn in the editor.")]
        public bool DebugPath = false;

        #endregion

        #region Private fields

        enum Mode
        {
            none,
            inDirection,
            toPosition,
            fromPosition,
            circle
        }

        private CharacterMotor _motor;
        private Actor _actor;

        private Mode _mode;
        private Vector3 _target;

        private float _speed = 1.0f;

        private float _inDirectionLeft;

        private NavMeshPath _path;
        private Vector3[] _pathPoints = new Vector3[64];
        private int _pathLength;
        private int _currentPathIndex;

        private Vector3 _direction;

        private bool _isCrouching;

        private int _side;

        private bool _wasMoving;
        private bool _isMoving;

        private NavMeshObstacle _obstacle;

        private bool _isRunningAwayTemp;
        private int _runningAwayFramesLeft;

        private bool _hasCheckedIfReachable = false;
        private Vector3 _positionToCheckIfReachable;

        private float _circleWait;
        private int _futureCircleDirection;

        private static Dictionary<Actor, AIMovement> _movements = new Dictionary<Actor, AIMovement>();
        private bool _isAvoidingMover;

        private float _avoidingTimer;
        private Vector3 _avoidDirection;
        private float _avoidSpeed;

        #endregion

        #region Events

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        /// <param name="value"></param>
        public void OnThreatPosition(Vector3 value)
        {
            if (_mode == Mode.circle || _mode == Mode.fromPosition)
                _target = value;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Enter crouch mode.
        /// </summary>
        public void ToCrouch()
        {
            _isCrouching = true;
        }

        /// <summary>
        /// Exit crouch mode.
        /// </summary>
        public void ToStopCrouching()
        {
            _isCrouching = false;
        }

        /// <summary>
        /// Told by the brains to circle around a threat.
        /// </summary>
        public void ToCircle(Vector3 threat)
        {
            _mode = Mode.circle;
            _target = threat;
            _speed = 0.5f;
            _side = 0;

            Cover cover = null;
            Vector3 position = Vector3.zero;

            if (Util.GetClosestCover(threat, 3, ref cover, ref position))
            {
                var vector = position - transform.position;
                var offset = Vector3.Dot(cover.Forward, vector);

                if (offset < 2)
                {
                    var right = Vector3.Dot(cover.Right, vector);

                    if (right > 0)
                        _side = -1;
                    else
                        _side = 1;
                }
            }
        }

        /// <summary>
        /// Told to walk in a direction.
        /// </summary>
        public void ToWalkInDirection(Vector3 vector)
        {
            _mode = Mode.inDirection;
            _speed = 0.5f;
            _inDirectionLeft = 0.3f;
            updateDirection(vector, true);
        }

        /// <summary>
        /// Told to run in a direction.
        /// </summary>
        public void ToRunInDirection(Vector3 vector)
        {
            _mode = Mode.inDirection;
            _speed = 1.0f;
            _inDirectionLeft = 0.3f;
            updateDirection(vector, true);
        }

        /// <summary>
        /// Told to sprint in a direction.
        /// </summary>
        public void ToSprintInDirection(Vector3 vector)
        {
            _mode = Mode.inDirection;
            _speed = 2.0f;
            _inDirectionLeft = 0.3f;
            updateDirection(vector, true);
        }

        /// <summary>
        /// Told by the brains to walk to a destination position.
        /// </summary>
        public void ToWalkTo(Vector3 destination)
        {
            moveTo(destination, 0.5f);
        }

        /// <summary>
        /// Told by the brains to run to a destination position.
        /// </summary>
        public void ToRunTo(Vector3 destination)
        {
            moveTo(destination, 1.0f);
        }

        /// <summary>
        /// Told by the brains to sprint to a destination position.
        /// </summary>
        public void ToSprintTo(Vector3 destination)
        {
            moveTo(destination, 2.0f);
        }

        /// <summary>
        /// Told by the brains to walk away from a position.
        /// </summary>
        public void ToWalkFrom(Vector3 target)
        {
            moveFrom(target, 0.5f);
        }

        /// <summary>
        /// Told by the brains to run away from a position.
        /// </summary>
        public void ToRunFrom(Vector3 target)
        {
            moveFrom(target, 0.5f);
        }

        /// <summary>
        /// Told by the brains to sprint away from a position.
        /// </summary>
        public void ToSprintFrom(Vector3 target)
        {
            var vector = transform.position - target;
            var distance = vector.magnitude;

            if (distance < RollTriggerDistance)
            {
                if (distance > 0.01f)
                    _motor.InputRoll(Util.RandomUnobstructedAngle(gameObject, transform.position + Vector3.up * 0.5f, Util.HorizontalAngle(vector), 90, 2));
                else
                    _motor.InputRoll(Util.RandomUnobstructedAngle(gameObject, transform.position + Vector3.up * 0.5f, 2));
            }

            _isRunningAwayTemp = false;
            moveFrom(target, 0.5f);
        }

        /// <summary>
        /// Told by the brains to sprint away from a position. Stops sprinting of not being told so in the following frames.
        /// </summary>
        public void ToKeepSprintingFrom(Vector3 target)
        {
            _isRunningAwayTemp = true;
            _runningAwayFramesLeft = 3;
            moveFrom(target, 0.5f);
        }

        /// <summary>
        /// Told by the brains to walk to stop moving.
        /// </summary>
        public void ToStopMoving()
        {
            _mode = Mode.none;
        }

        #endregion

        #region Behaviour

        private void Awake() 
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
            _obstacle = GetComponent<NavMeshObstacle>();

            _path = new NavMeshPath();
        }

        private void OnEnable()
        {
            _movements.Add(_actor, this);
        }

        private void OnDisable()
        {
            _movements.Remove(_actor);
            _mode = Mode.none;
        }

        private void Update()
        {
            _isMoving = false;

            if (_obstacle != null && _obstacle.enabled)
                _obstacle.enabled = false;

            if (_motor == null || !_motor.IsAlive)
            {
                if (_wasMoving)
                {
                    Message("OnStopMoving");
                    _wasMoving = false;
                }

                _mode = Mode.none;
                return;
            }

            if (_isCrouching)
                _motor.InputCrouch();

            if (_isAvoidingMover)
            {
                if (_avoidingTimer > float.Epsilon)
                {
                    _avoidingTimer -= Time.deltaTime;
                    move(_avoidDirection, 1, false);
                }
                else
                    _isAvoidingMover = false;
            }

            if (_mode == Mode.none)
            {
                if (_wasMoving)
                {
                    Message("OnStopMoving");
                    _wasMoving = false;
                }

                checkIncomingCollisions(Vector3.zero, 1);

                return;
            }

            if (DebugDestination)
                Debug.DrawLine(transform.position, _target, Color.blue);

            if (DebugPath)
                for (int i = 0; i < _pathLength - 1; i++)
                {
                    if (i == _currentPathIndex)
                    {
                        Debug.DrawLine(_pathPoints[i], _pathPoints[i + 1], Color.cyan);
                        Debug.DrawLine(_pathPoints[i + 1], _pathPoints[i + 1] + Vector3.up, Color.cyan);
                    }
                    else
                        Debug.DrawLine(_pathPoints[i], _pathPoints[i + 1], Color.green);
                }

            var vector = _target - transform.position;
            vector.y = 0;

            var direction = vector.normalized;
            var side = Vector3.Cross(direction, Vector3.up);

            if (_futureCircleDirection == 0)
                _circleWait = 0;

            switch (_mode)
            {
                case Mode.inDirection:
                    if (!checkIncomingCollisions(_direction, _speed))
                    {
                        if (canMoveInDirection(_direction))
                            move(_direction, _speed, true);

                        _inDirectionLeft -= Time.deltaTime;

                        if (_inDirectionLeft <= float.Epsilon)
                            _mode = Mode.none;
                    }
                    break;

                case Mode.toPosition:
                    var vectorToPath = Vector3.zero;
                    var isCloseToThePath = false;
                    var distanceToPath = 0f;

                    if (_currentPathIndex <= _pathLength - 1)
                    {
                        vectorToPath = Util.VectorToSegment(transform.position, _pathPoints[_currentPathIndex], _pathPoints[_currentPathIndex + 1]);
                        distanceToPath = vectorToPath.magnitude;
                        isCloseToThePath = distanceToPath < 0.5f;
                    }

                    if (!isCloseToThePath)
                        updatePath();

                    var isLastStepOnPartialPath = _currentPathIndex >= _pathLength - 2 && _path.status == NavMeshPathStatus.PathPartial;

                    if (_path.status != NavMeshPathStatus.PathInvalid && !_hasCheckedIfReachable)
                    {
                        if (_pathLength == 0 || Vector3.Distance(_pathPoints[_pathLength - 1], _positionToCheckIfReachable) >= 0.2f)
                            Message("OnPositionUnreachable", _positionToCheckIfReachable);

                        _hasCheckedIfReachable = true;
                    }

                    if (vector.magnitude >= 0.3f && !isLastStepOnPartialPath)
                    {
                        if (_path.status == NavMeshPathStatus.PathInvalid || _pathLength == 0)
                            updatePath();

                        var point = _currentPathIndex + 1 < _pathLength ? _pathPoints[_currentPathIndex + 1] : _target;

                        if (!AIUtil.IsPositionOnNavMesh(point))
                            updatePath();

                        direction = point - transform.position;
                        direction.y = 0;

                        var distanceToPoint = direction.magnitude;

                        if (distanceToPoint > float.Epsilon)
                            direction /= distanceToPoint;

                        if (!checkIncomingCollisions(direction, _speed))
                        {
                            if (distanceToPoint < 0.2f && _currentPathIndex + 1 < _pathLength)
                            {
                                var index = _currentPathIndex;

                                if (distanceToPoint > 0.07f && _currentPathIndex + 2 < _pathLength)
                                {
                                    if (Vector3.Dot(point - transform.position, _pathPoints[_currentPathIndex + 2] - transform.position) <= 0.1f)
                                        _currentPathIndex++;
                                }
                                else
                                    _currentPathIndex++;
                            }

                            if (distanceToPath > 0.12f)
                                direction = (direction + vectorToPath).normalized;

                            if (Vector3.Dot(direction, _direction) < 0.9f)
                                updateDirection(direction, false);

                            move(direction, _speed, false);
                        }
                    }
                    else
                    {
                        if (vector.magnitude > 0.03f)
                        {
                            if (!checkIncomingCollisions(direction, _speed))
                            {
                                if (vector.magnitude < 0.2f)
                                {
                                    _motor.InputImmediateCoverSearch();
                                    transform.position = Util.Lerp(transform.position, _target, 6);
                                }
                                else
                                {
                                    if (_motor.IsInCover)
                                        move(direction, 1.0f, false);
                                    else
                                        move(direction, 0.5f, false);
                                }
                            }
                        }
                        else
                        {
                            _motor.transform.position = _target;
                            _mode = Mode.none;
                        }
                    }
                    break;

                case Mode.fromPosition:
                    _pathLength = 0;

                    if (!checkIncomingCollisions(-direction, _speed))
                    {
                        direction = -direction;

                        if (canMoveInDirection(direction))
                        {
                            _motor.InputMovement(new CharacterMovement(direction, 1.0f));
                            _isMoving = true;
                        }
                        else
                        {
                            if (_side == 0)
                            {
                                if (Random.Range(0, 10) < 5 && _motor.IsFreeToMove(side, 0.5f, 0.25f))
                                    _side = 1;
                                else
                                    _side = -1;
                            }

                            if (!canMoveInDirection(side * _side))
                            {
                                if (!_motor.IsFreeToMove(-side * _side, 0.5f, 0.25f))
                                    Message("OnMoveFromFail");
                                else
                                    _side = -_side;
                            }

                            move(side * _side, 1.0f, true);
                        }

                        updateDirection(direction, false);

                        if (_isRunningAwayTemp)
                        {
                            _runningAwayFramesLeft--;

                            if (_runningAwayFramesLeft <= 0)
                                ToStopMoving();
                        }
                    }

                    break;

                case Mode.circle:
                    _pathLength = 0;

                    if (_futureCircleDirection != 0)
                    {
                        if (_circleWait > float.Epsilon)
                            _circleWait -= Time.deltaTime;

                        if (_circleWait < float.Epsilon)
                        {
                            _circleWait = 0;
                            _side = _futureCircleDirection;
                            _futureCircleDirection = 0;
                        }
                    }


                    if (_futureCircleDirection == 0)
                    {
                        if (_side == 0)
                        {
                            if (Random.Range(0, 10) < 5 && canMoveInDirection(side))
                                _side = 1;
                            else
                                _side = -1;
                        }

                        if (!canMoveInDirection(side * _side))
                        {
                            if (!canMoveInDirection(-side * _side))
                                Message("OnCircleFail");
                            else
                            {
                                _futureCircleDirection = -_side;
                                _circleWait = CircleDelay;
                            }
                        }

                        direction = side * _side;

                        if (!checkIncomingCollisions(direction, _speed))
                        {
                            move(direction, 1.0f, true);
                            updateDirection(direction, false);
                        }
                    }
                    break;
            }

            if (_isMoving && !_wasMoving)
                Message("OnMoving");
            else if (!_isMoving && _wasMoving)
                Message("OnStopMoving");

            _wasMoving = _isMoving;
        }

        #endregion

        #region Private methods

        private bool checkIncomingCollisions(Vector3 ownMovement, float speed)
        {
            var count = AIUtil.FindActors(transform.position, ObstructionRadius, _actor);

            var ownMovementMagnitude = ownMovement.magnitude;
            var hasOwnMovement = ownMovementMagnitude > 0.25f;
            var ownDirection = ownMovement / ownMovementMagnitude;

            var closestDot = 0f;
            var closestVector = Vector3.zero;
            var closestDirection = Vector3.zero;
            var closestPosition = Vector3.zero;
            var isClosestPrimary = false;
            var isClosestStatic = false;
            var hasClosest = false;

            for (int i = 0; i < count; i++)
            {
                var other = AIUtil.Actors[i];

                if (other.Side != _actor.Side && other.IsAggressive && _actor.IsAggressive)
                    continue;

                Vector3 velocity;
                float magnitude;

                var movement = _movements.ContainsKey(other) ? _movements[other] : null;
                var isStatic = false;

                if (movement != null && movement._isAvoidingMover)
                {
                    velocity = movement._avoidDirection;
                    magnitude = 1;
                }
                else
                {
                    velocity = other.Motor.MovementDirection;
                    magnitude = velocity.magnitude;

                    if (magnitude < 0.1f)
                    {
                        var body = other.Body;

                        if (body == null)
                            continue;

                        velocity = body.velocity;
                        magnitude = velocity.magnitude;

                        if (magnitude < 0.1f)
                        {
                            if (hasOwnMovement)
                            {
                                isStatic = true;
                                velocity = -ownMovement;
                                magnitude = 1;
                            }
                            else
                                continue;
                        }
                    }
                }

                var direction = velocity / magnitude;

                if (hasOwnMovement && Vector3.Dot(ownDirection, direction) > -0.5f)
                    continue;

                var vector = (transform.position - other.transform.position).normalized;
                var dot = Vector3.Dot(direction, vector);

                if (dot < 0.7f)
                    continue;

                var isPrimary = (movement == null ? true : !movement._isAvoidingMover) && !isStatic;

                if (!hasClosest || (isClosestStatic && !isStatic) || (isPrimary && !isClosestPrimary) || (isPrimary && dot > closestDot) || (!isPrimary && !isClosestPrimary && dot > closestDot))
                {
                    hasClosest = true;
                    isClosestPrimary = isPrimary;
                    isClosestStatic = isStatic;
                    closestPosition = other.transform.position;
                    closestDirection = direction;
                    closestVector = vector;
                    closestDot = dot;
                }
            }

            if (hasClosest)
            {
                if (!isClosestPrimary && !isClosestStatic && !hasOwnMovement && canMoveInDirection(closestVector))
                    return avoid(closestVector, speed);

                var point = Util.FindClosestToPath(closestPosition, closestPosition + closestDirection * 100, transform.position);
                var vector = transform.position - point;
                var distance = vector.magnitude;

                if (distance < 0.1f)
                {
                    var right = Vector3.Cross(closestVector, Vector3.up);
                    var left = -right;

                    if (hasOwnMovement && isClosestStatic)
                    {
                        right = (right + ownMovement).normalized;
                        left = (left + ownMovement).normalized;
                    }

                    if (canMoveInDirection(right))
                        return avoid(right, speed);

                    if (canMoveInDirection(left))
                        return avoid(left, speed);
                }
                else
                {
                    var direction = vector / distance;

                    if (hasOwnMovement && isClosestStatic)
                        direction = (direction + ownMovement).normalized;

                    if (canMoveInDirection(direction))
                        return avoid(direction, speed);
                }

                if (isClosestPrimary && !isClosestStatic && !hasOwnMovement && canMoveInDirection(closestVector))
                    return avoid(closestVector, speed);
            }

            return false;
        }

        private bool avoid(Vector3 direction, float speed)
        {
            _avoidingTimer = 0.15f;
            _isAvoidingMover = true;
            _avoidDirection = direction;
            _avoidSpeed = speed;
            move(direction, speed, true);
            return true;
        }

        private void move(Vector3 direction, float speed, bool noPath)
        {
            var origin = transform.position + Vector3.up * 0.5f;
            var target = origin + direction;

            const float threshold = 0.5f;

            if (!Util.IsFree(gameObject, origin, direction, threshold, false, true))
            {
                if (noPath)
                {
                    var right = Vector3.Cross(direction, Vector3.up);

                    if (Util.IsFree(gameObject, origin, right, 0.5f, false, true))
                        direction = right;
                    else if (Util.IsFree(gameObject, origin, -right, threshold, false, true))
                        direction = -right;
                }
                else
                    updatePath();
            }

            _motor.InputMovement(new CharacterMovement(direction, speed));
            _isMoving = true;
        }

        private void updateDirection(Vector3 value, bool force)
        {
            if (force || Vector3.Dot(_direction, value) < 0.95f)
            {
                _direction = value;
                Message("OnWalkDirection", _direction);
            }
        }

        private bool canMoveInDirection(Vector3 vector)
        {
            if (AIUtil.IsNavigationBlocked(transform.position, transform.position + vector))
                return false;

            return true;
        }

        private void moveTo(Vector3 destination, float speed)
        {
            _mode = Mode.toPosition;

            if (Vector3.Distance(_target, destination) > 0.3f)
            {
                _target = destination;
                updatePath();
            }
            
            _speed = speed;
        }

        private void moveFrom(Vector3 target, float speed)
        {
            _mode = Mode.fromPosition;
            _target = target;
            _speed = speed;
            _side = 0;
        }

        /// <summary>
        /// Sets up the navigation agent to move to the givent position.
        /// </summary>
        private void updatePath()
        {
            AIUtil.Path(ref _path, transform.position, _target);

            _pathLength = _path.GetCornersNonAlloc(_pathPoints);
            _currentPathIndex = 0;

            if (_pathLength > _pathPoints.Length)
                _pathLength = _pathPoints.Length;

            if (_pathLength > 1)
            {
                var vector = _pathPoints[1] - _pathPoints[0];
                var distance = vector.magnitude;

                if (distance > 0.3f)
                    updateDirection(vector / distance, true);
            }

            _hasCheckedIfReachable = false;
            _positionToCheckIfReachable = _target;
        }

        #endregion
    }
}
