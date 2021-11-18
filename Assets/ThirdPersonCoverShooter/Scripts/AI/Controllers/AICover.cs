using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Finds covers when asked by the brains.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterMotor))]
    public class AICover : AIBase
    {
        #region Public fields

        /// <summary>
        /// Maximum angle of a low cover relative to the enemy.
        /// </summary>
        [Tooltip("Maximum angle of a low cover relative to the enemy.")]
        public float MaxLowCoverThreatAngle = 60;

        /// <summary>
        /// Maximum angle of a tall cover relative to the enemy.
        /// </summary>
        [Tooltip("Maximum angle of a tall cover relative to the enemy.")]
        public float MaxTallCoverThreatAngle = 40;

        /// <summary>
        /// Maximum angle of a cover relative to the defense position.
        /// </summary>
        [Tooltip("Maximum angle of a cover relative to the defense position.")]
        public float MaxDefenseAngle = 85;

        /// <summary>
        /// If an enemy is on the same cover side as the AI, the cover will be taken if only the distance is greater than this value.
        /// </summary>
        [Tooltip("If an enemy is on the same cover side as the AI, the cover will be taken if only the distance is greater than this value.")]
        public float MinDefenselessDistance = 30;

        /// <summary>
        /// Maximum distance of a cover for AI to take.
        /// </summary>
        [Tooltip("Maximum distance of a cover for AI to take.")]
        public float MaxCoverDistance = 30;

        /// <summary>
        /// AI won't switch to cover positions closer than this distance.
        /// </summary>
        [Tooltip("AI won't switch to cover positions closer than this distance.")]
        public float MinSwitchDistance = 6;

        /// <summary>
        /// AI avoids taking covers that are closer to the enemy.
        /// </summary>
        [Tooltip("AI avoids taking covers that are closer to the enemy.")]
        public float AvoidDistance = 6;

        #endregion

        #region Private fields

        enum TakeMode
        {
            none,
            takeCover,
            takeCoverAgainst,
            takeDefense,
            switchCover,
            takeCloseTo
        }

        private Actor _actor;
        private CharacterMotor _motor;

        private NavMeshPath _path;
        private Vector3[] _corners = new Vector3[32];

        private bool _isRunning = true;
        private Cover _targetCover;
        private Vector3 _targetPosition;
        private int _targetDirection;
        private Vector3 _pivotPosition;
        private bool _hasPivot;
        private bool _isPivotThreat;
        private Cover _registeredCover;
        private CoverCache _covers = new CoverCache();
        private bool _hasAskedToStopMoving;
        private bool _hasTakenTheRightCover;

        private bool _hasMaxPivotDistance = true;
        private float _maxPivotDistance;

        private bool _takeAnyCover;

        private bool _isKeepingCloseTo;
        private KeepCloseTo _keepCloseTo;

        private Actor[] _cachedEnemies;
        private bool _hasCachedEnemies;
        private int _cachedEnemyCount;

        private TakeMode _mode;
        private int _modeWait;

        private Cover _unreachableCover;

        private float _takeCloseToRadius;
        private Vector3 _takeCloseToPosition;

        #endregion

        #region Events

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        /// <param name="position"></param>
        public void OnThreatPosition(Vector3 position)
        {
            _pivotPosition = position;
            _isPivotThreat = true;
        }

        /// <summary>
        /// Registers the max distance to search for covers.
        /// </summary>
        public void OnMaxCoverPivotDistance(float value)
        {
            _hasMaxPivotDistance = true;
            _maxPivotDistance = value;
        }

        /// <summary>
        /// Registers that there is no max cover distance.
        /// </summary>
        public void OnNoMaxCoverPivotDistance()
        {
            _hasMaxPivotDistance = false;
        }

        /// <summary>
        /// Registers that the AI is holding a position. Removes target cover.
        /// </summary>
        public void OnHoldPosition(Vector3 position)
        {
            _takeAnyCover = true;
            nullCover();
            updateRegistration();
        }

        /// <summary>
        /// Registers that the current target cover is unreachable.
        /// </summary>
        public void OnPositionUnreachable(Vector3 position)
        {
            if (_targetCover != null && Vector3.Distance(_targetPosition, position) <= 0.2f)
                _unreachableCover = _targetCover;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Registers that the AI is told to keep close to a position.
        /// </summary>
        public void ToKeepCloseTo(KeepCloseTo value)
        {
            _isKeepingCloseTo = true;
            _keepCloseTo = value;
        }

        /// <summary>
        /// Sets the component to run towards future covers.
        /// </summary>
        public void ToRunToCovers()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Sets the component to walk towards future covers.
        /// </summary>
        public void ToWalkToCovers()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Told by the brains to take the cloest cover.
        /// </summary>
        public void ToTakeCover()
        {
            _hasPivot = false;
            _takeAnyCover = false;
            nullCover();

            if (isActiveAndEnabled)
            {
                if (_isKeepingCloseTo)
                    _covers.Reset(_keepCloseTo.Position, _keepCloseTo.Distance);
                else
                    _covers.Reset(transform.position, MaxCoverDistance);

                _mode = TakeMode.takeCover;
                _modeWait = 2;
                Message("OnCoverSearch");
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to take the closest suitable cover.
        /// </summary>
        public void ToTakeCoverAgainst(Vector3 position)
        {
            _hasPivot = true;
            _pivotPosition = position;
            _takeAnyCover = false;
            _isPivotThreat = true;
            nullCover();

            if (isActiveAndEnabled)
            {
                if (_isKeepingCloseTo)
                    _covers.Reset(_keepCloseTo.Position, _keepCloseTo.Distance);
                else
                    _covers.Reset(transform.position, MaxCoverDistance);

                _mode = TakeMode.takeCoverAgainst;
                _modeWait = 2;
                Message("OnCoverSearch");
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to take a defensive cover near a position.
        /// </summary>
        public void ToTakeDefenseCover(Vector3 position)
        {
            _hasPivot = true;
            _pivotPosition = position;
            _takeAnyCover = false;
            _isPivotThreat = false;
            nullCover();

            if (isActiveAndEnabled)
            {
                if (_isKeepingCloseTo)
                    _covers.Reset(_keepCloseTo.Position, _keepCloseTo.Distance);
                else
                    _covers.Reset(transform.position, MaxCoverDistance);

                _mode = TakeMode.takeDefense;
                _modeWait = 2;
                Message("OnCoverSearch");
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to take a cover closer to the enemy.
        /// </summary>
        public void ToSwitchCover()
        {
            _takeAnyCover = false;
            nullCover();

            if (isActiveAndEnabled)
            {
                if (_isKeepingCloseTo)
                    _covers.Reset(_keepCloseTo.Position, _keepCloseTo.Distance);
                else
                    _covers.Reset(transform.position, MaxCoverDistance);

                _mode = TakeMode.switchCover;
                _modeWait = 2;
                Message("OnCoverSearch");
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to take a cover closer to the enemy.
        /// </summary>
        public void ToTakeCoverCloseTo(AIBaseRegrouper regrouper)
        {
            _takeAnyCover = false;
            nullCover();

            if (isActiveAndEnabled)
            {
                _takeCloseToRadius = regrouper.Radius;

                var movement = regrouper.GetComponent<AIMovement>();
                _takeCloseToPosition = regrouper.transform.position;

                if (movement != null)
                    _takeCloseToPosition = movement.Destination;

                _covers.Reset(_takeCloseToPosition, MaxCoverDistance);

                _mode = TakeMode.takeCloseTo;
                _modeWait = 2;
                Message("OnCoverSearch");
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to stop moving towards a cover.
        /// </summary>
        public void ToStopMoving()
        {
            if (_actor.Cover != _targetCover)
            {
                _takeAnyCover = false;
                nullCover();
            }
        }

        /// <summary>
        /// Told by the brains to exit cover.
        /// </summary>
        public void ToLeaveCover()
        {
            _takeAnyCover = false;
            nullCover();
            updateRegistration();
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();

            _path = new NavMeshPath();
        }

        private void Update()
        {
            if (_actor == null || !_actor.IsAlive)
                return;

            updateRegistration();

            if (_modeWait > 0)
                _modeWait--;
            else
            {
                switch (_mode)
                {
                    case TakeMode.none: break;

                    case TakeMode.takeCover:
                        {
                            _mode = TakeMode.none;
                            _targetCover = null;

                            var weapon = _motor.EquippedWeapon;

                            if (weapon.IsNull || !weapon.PreventCovers)
                            {
                                for (int i = 0; i < _covers.Items.Count; i++)
                                {
                                    var item = _covers.Items[i];

                                    if (isValidCover(item.Cover, item.Position, item.Direction, true, false))
                                    {
                                        takeCover(item.Cover, item.Position, item.Direction, 1);
                                        break;
                                    }
                                }
                            }

                            if (_targetCover != null)
                                Message("OnFoundCover");
                            else
                                Message("OnNoCover");
                        }
                        break;

                    case TakeMode.takeCoverAgainst:
                        {
                            _mode = TakeMode.none;
                            _targetCover = null;

                            var weapon = _motor.EquippedWeapon;

                            if (weapon.IsNull || !weapon.PreventCovers)
                            {
                                var isAlreadyClose = Vector3.Distance(transform.position, _pivotPosition) < AvoidDistance;

                                for (int i = 0; i < _covers.Items.Count; i++)
                                {
                                    var item = _covers.Items[i];

                                    if (isValidCover(item.Cover, item.Position, item.Direction, true, !isAlreadyClose))
                                    {
                                        takeCover(item.Cover, item.Position, item.Direction, 1);
                                        break;
                                    }
                                }
                            }

                            if (_targetCover != null)
                                Message("OnFoundCover");
                            else
                                Message("OnNoCover");
                        }
                        break;
                    case TakeMode.takeDefense:
                        {
                            _mode = TakeMode.none;
                            _targetCover = null;

                            var weapon = _motor.EquippedWeapon;

                            if (weapon.IsNull || !weapon.PreventCovers)
                            {
                                for (int i = 0; i < _covers.Items.Count; i++)
                                {
                                    var item = _covers.Items[i];

                                    if (isValidCover(item.Cover, item.Position, item.Direction, true, false))
                                    {
                                        takeCover(item.Cover, item.Position, item.Direction, 1);
                                        break;
                                    }
                                }
                            }

                            if (_targetCover != null)
                                Message("OnFoundCover");
                            else
                                Message("OnNoCover");
                        }
                        break;
                    case TakeMode.switchCover:
                        {
                            _mode = TakeMode.none;
                            _targetCover = null;

                            var weapon = _motor.EquippedWeapon;

                            if (weapon.IsNull || !weapon.PreventCovers)
                            {
                                var currentDistance = Vector3.Distance(transform.position, _pivotPosition);
                                var isAlreadyClose = currentDistance < AvoidDistance;

                                for (int i = 0; i < _covers.Items.Count; i++)
                                {
                                    var item = _covers.Items[i];

                                    if (Vector3.Distance(transform.position, item.Position) >= MinSwitchDistance &&
                                        Vector3.Distance(item.Position, _pivotPosition) < currentDistance &&
                                        isValidCover(item.Cover, item.Position, item.Direction, true, !isAlreadyClose))
                                    {
                                        takeCover(item.Cover, item.Position, item.Direction, 1);
                                        break;
                                    }
                                }
                            }

                            if (_targetCover != null)
                                Message("OnFoundCover");
                            else
                                Message("OnNoCover");
                        }
                        break;
                    case TakeMode.takeCloseTo:
                        {
                            _mode = TakeMode.none;
                            _targetCover = null;

                            var weapon = _motor.EquippedWeapon;

                            if (weapon.IsNull || !weapon.PreventCovers)
                            {
                                for (int i = 0; i < _covers.Items.Count; i++)
                                {
                                    var item = _covers.Items[i];

                                    if (Vector3.Distance(item.Position, _takeCloseToPosition) <= _takeCloseToRadius)
                                        if (isValidCover(item.Cover, item.Position, item.Direction, true, false))
                                        {
                                            takeCover(item.Cover, item.Position, item.Direction, 1);
                                            break;
                                        }
                                }
                            }

                            if (_targetCover != null)
                                Message("OnFoundCover");
                            else
                                Message("OnNoCover");
                        }
                        break;
                }
            }

            if (_targetCover == null)
            {
                if (_takeAnyCover && _motor.PotentialCover != null)
                {
                    _targetCover = _motor.PotentialCover;
                    _motor.InputTakeCover();
                }
                else if (!_takeAnyCover && _motor.Cover != null)
                    _motor.InputLeaveCover();
            }

            if (_motor.Cover != _targetCover && _targetCover != null)
            {
                if (_motor.PotentialCover != null &&
                    (_motor.PotentialCover == _targetCover ||
                     _motor.PotentialCover.LeftAdjacent == _targetCover ||
                     _motor.PotentialCover.RightAdjacent == _targetCover))
                {
                    _motor.InputTakeCover();
                }
                else if (_motor.Cover != null && _motor.Cover.LeftAdjacent != _targetCover && _motor.Cover.RightAdjacent != _targetCover)
                    _motor.InputLeaveCover();
                else if (Vector3.Distance(transform.position, _targetPosition) < 0.5f)
                    _motor.InputImmediateCoverSearch();
            }

            if (_targetCover != null)
            {
                var weapon = _motor.EquippedWeapon;

                if (!weapon.IsNull && weapon.PreventCovers)
                {
                    _targetCover = null;
                    Message("OnInvalidCover");
                }
                else if (_hasPivot)
                    if (!isValidCover(_targetCover, _targetPosition, _targetDirection, false))
                        Message("OnInvalidCover");
            }

            if (_targetCover != null && _targetCover == _motor.Cover && _targetCover.IsTall)
            {
                if (_targetDirection > 0)
                {
                    _motor.InputStandRight();

                    if (!_motor.IsNearRightCorner)
                        _motor.InputMovement(new CharacterMovement(_targetCover.Right, 0.5f));
                }
                else
                {
                    _motor.InputStandLeft();

                    if (!_motor.IsNearLeftCorner)
                        _motor.InputMovement(new CharacterMovement(_targetCover.Left, 0.5f));
                }
            }

            if (_targetCover != null && _motor.Cover != null && Vector3.Distance(_actor.transform.position, _targetPosition) < 0.5f &&
                (_motor.Cover == _targetCover ||
                 _motor.Cover.LeftAdjacent == _targetCover ||
                 _motor.Cover.RightAdjacent == _targetCover))
            {
                if (!_hasTakenTheRightCover)
                {
                    _hasTakenTheRightCover = true;
                    Message("OnFinishTakeCover");
                }

                if (!_hasAskedToStopMoving)
                {
                    _hasAskedToStopMoving = true;
                    Message("ToStopMoving");
                }
            }
            else
                _hasAskedToStopMoving = false;

            _hasCachedEnemies = false;
        }

        #endregion

        #region Private methods

        private void updateRegistration()
        {
            if (_registeredCover != _targetCover)
            {
                if (_registeredCover != null)
                    _registeredCover.UnregisterUser(_actor);

                _registeredCover = _targetCover;
            }

            if (_registeredCover != null)
            {
                if (_actor.Cover == _registeredCover)
                    _registeredCover.RegisterUser(_actor, _actor.transform.position);
                else
                    _registeredCover.RegisterUser(_actor, _targetPosition);
            }
        }

        private void takeCover(Cover cover, Vector3 position, int direction, float speed)
        {
            _targetPosition = position;
            _targetCover = cover;
            _targetDirection = direction;
            _hasTakenTheRightCover = false;
            _hasAskedToStopMoving = false;
            updateRegistration();

            if (_isRunning)
                Message("ToRunTo", position);
            else
                Message("ToWalkTo", position);
        }

        private void nullCover()
        {
            if (_targetCover != null)
                _targetCover = null;
        }

        private bool isValidCover(Cover cover, Vector3 position, int direction, bool checkPath, bool avoidPivot = true)
        {
            if (cover == _unreachableCover)
                return false;

            if (_isKeepingCloseTo && Vector3.Distance(position, _keepCloseTo.Position) > _keepCloseTo.Distance)
                return false;

            if (!_hasPivot)
            {
                if (!AIUtil.IsCoverPositionFree(cover, position, 1, _actor))
                    return false;

                return true;
            }

            if (!_hasCachedEnemies)
            {
                _cachedEnemyCount = 0;
                _hasCachedEnemies = true;

                var totalActorCount = AIUtil.FindActors(position, MinDefenselessDistance, _actor);

                if (totalActorCount > 0)
                {
                    var enemyCount = 0;
                    for (int i = 0; i < totalActorCount; i++)
                        if (AIUtil.Actors[i].Side != _actor.Side)
                            enemyCount++;

                    if (enemyCount > 0)
                    {
                        if (_cachedEnemies == null || _cachedEnemies.Length < enemyCount)
                            _cachedEnemies = new Actor[enemyCount];

                        var index = 0;

                        for (int i = 0; i < totalActorCount; i++)
                            if (AIUtil.Actors[i].Side != _actor.Side)
                                _cachedEnemies[index++] = AIUtil.Actors[i];

                        _cachedEnemyCount = index;
                    }
                }
            }

            for (int i = 0; i < _cachedEnemyCount; i++)
            {
                var enemy = _cachedEnemies[i];

                if (enemy.Side != _actor.Side)
                {
                    var enemyPosition = enemy.transform.position;
                    var distance = Vector3.Distance(position, enemyPosition);

                    if (distance < AvoidDistance)
                        return false;

                    if (!AIUtil.IsGoodAngle(MaxTallCoverThreatAngle,
                                            MaxLowCoverThreatAngle,
                                            cover,
                                            position,
                                            enemyPosition,
                                            cover.IsTall))
                        return false;
                }
            }

            if (_isPivotThreat)
            {
                var distance = Vector3.Distance(position, _pivotPosition);

                if (_hasMaxPivotDistance && distance > _maxPivotDistance)
                    return false;

                var aimPosition = position;

                if (AIUtil.IsObstructed(aimPosition + (_actor.StandingTopPosition - transform.position),
                                        _pivotPosition + Vector3.up * 2))
                    return false;
            }
            else
            {
                var distance = Vector3.Distance(position, _pivotPosition);

                if (_hasMaxPivotDistance && distance > _maxPivotDistance)
                    return false;

                if (!AIUtil.IsGoodAngle(MaxDefenseAngle,
                                        MaxDefenseAngle,
                                        cover,
                                        _pivotPosition,
                                        position,
                                        cover.IsTall))
                    return false;
            }

            if (!AIUtil.IsCoverPositionFree(cover, position, 1, _actor))
                return false;

            if (checkPath)
            {
                if (NavMesh.CalculatePath(transform.position, position, 1, _path))
                {
                    if (avoidPivot)
                    {
                        var count = _path.GetCornersNonAlloc(_corners);

                        for (int i = 0; i < count; i++)
                        {
                            var a = i == 0 ? transform.position : _corners[i - 1];
                            var b = _corners[i];

                            var closest = Util.FindClosestToPath(a, b, _pivotPosition);

                            if (Vector3.Distance(closest, _pivotPosition) < AvoidDistance)
                                return false;
                        }
                    }
                }
                else
                    return false;
            }

            return true;
        }

        #endregion
    }
}
