using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Settings for a free roam formation when in danger.
    /// </summary>
    [Serializable]
    public struct FreeRoamInDangerSettings
    {
        public float MaxDistance;
        public float FollowDistance;

        public static FreeRoamInDangerSettings Default()
        {
            FreeRoamInDangerSettings settings;
            settings.MaxDistance = 4;
            settings.FollowDistance = 2;

            return settings;
        }
    }

    /// <summary>
    /// Settings for a free roam formation when not in danger.
    /// </summary>
    [Serializable]
    public struct FreeRoamOutOfDangerSettings
    {
        public float MaxDistance;
        public float FollowDistance;

        public static FreeRoamOutOfDangerSettings Default()
        {
            FreeRoamOutOfDangerSettings settings;
            settings.MaxDistance = 8;
            settings.FollowDistance = 2;

            return settings;
        }
    }

    /// <summary>
    /// Settings for a line formation.
    /// </summary>
    [Serializable]
    public struct LineFormationSettings
    {
        public float Distance;

        public static LineFormationSettings Default()
        {
            LineFormationSettings settings;
            settings.Distance = 1;

            return settings;
        }
    }

    /// <summary>
    /// Settings for a square formation.
    /// </summary>
    [Serializable]
    public struct SquareFormationSettings
    {
        public float Distance;

        public static SquareFormationSettings Default()
        {
            SquareFormationSettings settings;
            settings.Distance = 1.5f;

            return settings;
        }
    }

    /// <summary>
    /// Describes a position and distance for the AI to stay from it.
    /// </summary>
    public struct KeepCloseTo
    {
        public Vector3 Position;
        public float Distance;

        public KeepCloseTo(Vector3 position, float distance)
        {
            Position = position;
            Distance = distance;
        }
    }

    public enum FormationType
    {
        Free,
        Square,
        Line
    }

    /// <summary>
    /// Moves the AI to a certain position. One AIFormation is set to be a leader and others adjust their position based on the leader's state.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(FighterBrain))]
    [RequireComponent(typeof(AIMovement))]
    public class AIFormation : AIBase, ICharacterWalkListener
    {
        /// <summary>
        /// Max distance at which the AI can walk around the leader when free roaming in danger.
        /// </summary>
        public float MaxDistance
        {
            get { return FreeRoamInDanger.MaxDistance; }
        }

        /// <summary>
        /// Direction at which the AI is facing.
        /// </summary>
        public Vector3 Direction
        {
            get { return _direction; }
        }

        /// <summary>
        /// Actor component.
        /// </summary>
        public Actor Actor
        {
            get { return _actor; }
        }

        /// <summary>
        /// FighterBrain component.
        /// </summary>
        public FighterBrain Brain
        {
            get { return _brain; }
        }

        /// <summary>
        /// Formation leader.
        /// </summary>
        [Tooltip("Formation leader.")]
        public AIFormation Leader;

        /// <summary>
        /// Formation to take. Followers use the value presented by the leader ignoring their own.
        /// </summary>
        [Tooltip("Formation to take. Followers use the value presented by the leader ignoring their own.")]
        public FormationType Formation = FormationType.Free;

        /// <summary>
        /// Are the members of the formation aggressive (overrides FighterBrain.ImmediateThreatReaction).
        /// </summary>
        [Tooltip("Are the members of the formation aggressive (overrides FighterBrain.ImmediateThreatReaction).")]
        public bool Aggressive = true;

        /// <summary>
        /// Minimum space the followers keep between each other.
        /// </summary>
        [Tooltip("Minimum space the followers keep between each other.")]
        public float FollowerSpacing = 1;

        /// <summary>
        /// The AI will run only if the stamina is this or higher.
        /// </summary>
        [Tooltip("The AI will run only if the stamina is this or higher.")]
        [Range(0, 1)]
        public float RunStaminaFraction = 0.5f;

        /// <summary>
        /// AI will run towards the position if the distance to it is greater.
        /// </summary>
        [Tooltip("AI will run towards the position if the distance to it is greater.")]
        public float MinRunDistance = 4;

        /// <summary>
        /// AI will avoid staying further from the leader than this given distance.
        /// </summary>
        [Tooltip("AI will avoid staying further from the leader than this given distance.")]
        public float CombatDistance = 8;

        /// <summary>
        /// Settings for the free roam (in danger) formation.
        /// </summary>
        [Tooltip("Settings for the free roam (in danger) formation.")]
        public FreeRoamInDangerSettings FreeRoamInDanger = FreeRoamInDangerSettings.Default();

        /// <summary>
        /// Settings for the roam (out of danger) formation.
        /// </summary>
        [Tooltip("Settings for the free roam (out of danger) formation.")]
        public FreeRoamOutOfDangerSettings FreeRoamOutOfDanger = FreeRoamOutOfDangerSettings.Default();

        /// <summary>
        /// Settings for the line formation.
        /// </summary>
        [Tooltip("Settings for the line formation.")]
        public LineFormationSettings Line = LineFormationSettings.Default();

        /// <summary>
        /// Settings for the square formation.
        /// </summary>
        [Tooltip("Settings for the square formation.")]
        public SquareFormationSettings Square = SquareFormationSettings.Default();

        private Actor _actor;
        private FighterBrain _brain;
        private NavMeshObstacle _obstacle;

        private bool _hadPreviousLeaderPosition;
        private Vector3 _previousLeaderPosition;
        private float _pathTimer;
        private Vector3 _pathTarget;

        private List<AIFormation> _followers = new List<AIFormation>();
        private List<AIFormation> _followerBuild = new List<AIFormation>();
        private List<int> _followerToPositionId = new List<int>();
        private List<int> _positionBuild = new List<int>();
        private List<Vector3> _line = new List<Vector3>();

        private bool _isMaintainingPosition;
        private bool _isInProcess;

        private bool _wasTakingCover;
        private float _takeCoverTimer;

        private float _coverSearchTimer;

        private bool _takeCoverCheck;

        private int _registrationId;
        private Vector3 _direction;
        private Vector3 _lastPosition;

        private CharacterStamina _stamina;

        private bool _isRunning;
        private bool _hasThreat;

        private bool _isRegrouping;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _brain = GetComponent<FighterBrain>();
            _stamina = GetComponent<CharacterStamina>();
            _obstacle = GetComponent<NavMeshObstacle>();
            _direction = transform.forward;
            _lastPosition = transform.position;
        }

        /// <summary>
        /// Notified of a threat.
        /// </summary>
        public void OnThreat(Actor value)
        {
            _hasThreat = true;
        }

        /// <summary>
        /// Notified that there is no thret.
        /// </summary>
        public void OnNoThreat()
        {
            _hasThreat = false;
        }

        public void OnStop()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Notified that the AI is running.
        /// </summary>
        public void OnRun()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Notified that the AI is sprinting.
        /// </summary>
        public void OnSprint()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Notified that the AI is walking.
        /// </summary>
        public void OnWalk()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Notified that the AI has found a cover and started moving to it.
        /// </summary>
        public void OnFoundCover()
        {
            _takeCoverCheck = true;
        }

        protected AIFormation GetFollower(int index)
        {
            if (index < _followers.Count)
                return _followers[index];
            else
                return null;
        }

        protected int Register(AIFormation follower)
        {
            var old = _followerBuild.IndexOf(follower);

            if (old >= 0)
                return old;

            var id = _followerBuild.Count;
            _followerBuild.Add(follower);

            return id;
        }

        protected Vector3 Position
        {
            get
            {
                if (_pathTimer > float.Epsilon)
                    return _pathTarget;
                else
                    return transform.position;
            }
        }

        /// <summary>
        /// Notified that the AI is regrouping.
        /// </summary>
        public void ToRegroupFormation()
        {
            _isRegrouping = true;
            Message("ToEnterProcess", _hasThreat);
        }

        /// <summary>
        /// Notified that the AI has entered some process.
        /// </summary>
        public void ToEnterProcess()
        {
            _isInProcess = true;
        }

        /// <summary>
        /// Notified that the AI has exited it's process.
        /// </summary>
        public void ToExitProcess()
        {
            _isInProcess = false;
        }

        /// <summary>
        /// Notified that the AI has exited it's process.
        /// </summary>
        public void ToExitProcessAndMaintainPosition()
        {
            _isInProcess = false;
        }

        /// <summary>
        /// Notified that the AI has started to hold a position.
        /// </summary>
        public void OnHoldPosition(Vector3 position)
        {
            _isMaintainingPosition = true;
            _pathTarget = position;
            _pathTimer = 999999;
        }

        /// <summary>
        /// Notified that the AI has stopped maintaining a position.
        /// </summary>
        public void OnStopHoldingPosition()
        {
            _isMaintainingPosition = false;
            _pathTimer = 0;
        }

        protected void MoveTo(Vector3 position, float duration)
        {
            _pathTarget = position;
            _pathTimer = duration;

            if ((_isRunning || canRun) && ShouldRun(position))
                Message("ToRunTo", position);
            else
                Message("ToWalkTo", position);
        }

        private bool ShouldRun(Vector3 target)
        {
            return Vector3.Distance(transform.position, target) >= MinRunDistance;
        }

        private bool canRun
        {
            get
            {
                if (_stamina == null)
                    return true;

                return _stamina.Stamina / _stamina.MaxStamina >= RunStaminaFraction;
            }
        }

        private void OnDisable()
        {
            _isMaintainingPosition = false;
        }

        private void Update()
        {
            if (Vector3.Distance(transform.position, _lastPosition) > 0.3f)
            {
                _direction = (transform.position - _lastPosition).normalized;
                _lastPosition = transform.position;
            }

            if (!_actor.IsAlive)
                return;

            if (_isMaintainingPosition && Brain.enabled)
                return;

            if (_brain.enabled && (_brain.Threat != null || _hasThreat) && !_isRegrouping)
            {
                if (Leader != null && _brain.LockedThreat != null)
                    Message("ToKeepCloseTo", new KeepCloseTo(Leader.transform.position, CombatDistance));

                return;
            }

            if (this == Leader)
            {
                _brain.ImmediateThreatReaction = Aggressive;

                _followers.Clear();
                _followers.AddRange(_followerBuild);
                _followerBuild.Clear();

                _followerToPositionId.Clear();
                _line.Clear();

                for (int i = 0; i < _followers.Count; i++)
                    _followerToPositionId.Add(-1);

                var implementedFormation = Leader.Formation;
                if (_isRegrouping && implementedFormation == FormationType.Free)
                    implementedFormation = FormationType.Square;

                switch (implementedFormation)
                {
                    case FormationType.Line:
                        for (int positionId = 0; positionId < _followers.Count; positionId++)
                        {
                            var followerId = -1;
                            var refDist = 0f;

                            var linePosition = Vector3.zero;

                            for (int id = 0; id < _followers.Count; id++)
                                if (_followerToPositionId[id] < 0)
                                {
                                    var position = followerLinePositionAt(positionId, _followers[id].transform.position);
                                    var dist = Vector3.Distance(_followers[id].transform.position, position);

                                    if (followerId < 0 || dist < refDist)
                                    {
                                        refDist = dist;
                                        followerId = id;
                                        linePosition = position;
                                    }
                                }

                            _line.Add(linePosition);
                            _followerToPositionId[followerId] = positionId;
                        }
                        break;

                    case FormationType.Square:
                        {
                            var minDist = 99999999f;

                            _positionBuild.Clear();

                            for (int i = 0; i < _followers.Count; i++)
                                _positionBuild.Add(-1);

                            recursiveEvaluate(implementedFormation, 0, ref minDist);
                        } break;
                }

                for (int i = 0; i < _followers.Count - 1; i++)
                    for (int j = i + 1; j < _followers.Count; j++)
                    {
                        if (implementedFormation == FormationType.Free)
                        {
                            var distance = Vector3.Distance(_followers[i].Position, _followers[j].Position);
                            var hasMoved = false;

                            if (distance < FollowerSpacing)
                                for (int a = 0; a < 6; a++)
                                {
                                    var dist = UnityEngine.Random.Range(FollowerSpacing, _followers[i].MaxDistance);
                                    var angle = UnityEngine.Random.Range(0, 360);
                                    var position = transform.position + Util.HorizontalVector(angle) * dist;
                                    var isOk = true;

                                    if (!_followers[a]._isInProcess || _followers[a]._isRegrouping)
                                        for (int k = j; k < _followers.Count; k++)
                                            if (!_followers[k]._isInProcess || !_followers[k]._isRegrouping)
                                                if (Vector3.Distance(position, _followers[k].Position) < FollowerSpacing)
                                                {
                                                    isOk = false;
                                                    break;
                                                }

                                    if (isOk)
                                    {
                                        _followers[i].MoveTo(position, 1);
                                        hasMoved = true;
                                        break;
                                    }
                                }

                            if (hasMoved)
                                break;
                        }
                    }

                _hadPreviousLeaderPosition = false;
            }
            else if (Leader != null)
            {
                _brain.ImmediateThreatReaction = Leader.Aggressive;
                _registrationId = Leader.Register(this);

                var isBusy = _brain.State == FighterState.maintainPosition ||
                             _brain.State == FighterState.avoidGrenade ||
                             _brain.State == FighterState.process;

                if (!_isRegrouping && !isBusy)
                    _brain.SetIdleAlertedState();

                if (_takeCoverTimer > float.Epsilon)
                    _takeCoverTimer -= Time.deltaTime;

                if (_coverSearchTimer > float.Epsilon)
                    _coverSearchTimer -= Time.deltaTime;

                if (_pathTimer > float.Epsilon)
                    _pathTimer -= Time.deltaTime;
                else if (!isBusy || _isRegrouping)
                {
                    var formation = Leader.Formation;
                    if (_isRegrouping && formation == FormationType.Free)
                        formation = FormationType.Square;

                    if (!_isInProcess || _isRegrouping)
                    {
                        switch (formation)
                        {
                            case FormationType.Free:
                                if (Leader._brain.IsInDanger)
                                    updateFreeRoamInDanger();
                                else
                                    updateFreeRoamOutOfDanger();
                                break;

                            case FormationType.Line:
                            case FormationType.Square:
                                updatePositional(formation);
                                break;
                        }
                    }
                }

                _previousLeaderPosition = Leader.transform.position;
                _hadPreviousLeaderPosition = true;
            }
        }

        private void recursiveEvaluate(FormationType formation, int index, ref float minDist)
        {
            for (int i = 0; i < _followers.Count; i++)
                if (!containsInPositionBuild(index, i))
                {
                    _positionBuild[index] = i;

                    if (index + 1 == _followers.Count)
                        evaluatePositionBuild(formation, ref minDist);
                    else
                        recursiveEvaluate(formation, index + 1, ref minDist);
                }
        }

        private bool containsInPositionBuild(int index, int value)
        {
            if (index <= 0)
                return false;

            for (int i = 0; i < index; i++)
                if (_positionBuild[i] == value)
                    return true;

            return false;
        }

        private void evaluatePositionBuild(FormationType formation, ref float minDist)
        {
            var dist = 0f;

            for (int id = 0; id < _followers.Count; id++)
                dist += Vector3.Distance(_followers[id].transform.position, followerPositionAt(formation, _positionBuild[id]));

            if (dist < minDist)
            {
                minDist = dist;

                for (int id = 0; id < _followers.Count; id++)
                    _followerToPositionId[id] = _positionBuild[id];
            }
        }

        private Vector3 followerPositionFor(FormationType formation, int index)
        {
            if (_followerToPositionId.Count > index)
                return followerPositionAt(formation, _followerToPositionId[index]);
            else
            {
                Debug.Assert(false);
                return followerPositionAt(formation, index);
            }
        }

        private Vector3 followerLinePositionAt(int index, Vector3 current)
        {
            Debug.Assert(index == 0 || _line.Count >= index);

            var head = index <= 0 ? transform.position : _line[index - 1];
            return head + (current - head).normalized * Line.Distance;
        }

        private Vector3 followerPositionAt(FormationType formation, int index)
        {
            switch (formation)
            {
                case FormationType.Square:
                    {
                        var right = Vector3.Cross(Direction, Vector3.up);

                        Vector3 position;

                        switch (index)
                        {
                            case 0: position = transform.position - (right + Direction).normalized * Square.Distance; break;
                            case 1: position = transform.position - (-right + Direction).normalized * Square.Distance; break;
                            default: position = transform.position - Direction * Square.Distance * index; break;
                        }

                        var ideal = position;

                        AIUtil.GetClosestStandablePosition(ref position);

                        var closest = Vector3.zero;
                        var hasClosest = false;
                        var minDist = 0f;

                        for (int i = 0; i < _followers.Count; i++)
                        {
                            var dist = Vector3.Distance(_followers[i].transform.position, ideal);

                            if (_followers[i]._obstacle != null && _followers[i]._obstacle.enabled && dist <= _followers[i]._obstacle.radius + float.Epsilon)
                                dist = 0;

                            if (!hasClosest || dist < minDist)
                            {
                                closest = _followers[i].transform.position;
                                minDist = dist;
                                hasClosest = true;
                            }
                        }

                        if (hasClosest && Vector3.Distance(position, ideal) > minDist)
                            position = closest;

                        return position;
                    }

                case FormationType.Line:
                    return _line[index];

                default:
                    Debug.Assert(false);
                    return Vector3.zero;
            }
        }

        private void updatePositional(FormationType formation)
        {
            if (Leader._followerToPositionId.Count < Leader._followers.Count ||
                Leader._followers.Count <= _registrationId)
                return;

            var target = Leader.followerPositionFor(formation, _registrationId);

            if (Vector3.Distance(target, transform.position) < 0.2f)
            {
                if (_isRegrouping)
                {
                    _isRegrouping = false;
                    Message("ToExitProcess");
                }

                return;
            }

            if ((ShouldRun(target) || Leader._isRunning) && (_isRunning || canRun))
                Message("ToRunTo", target);
            else
                Message("ToWalkTo", target);
        }

        private void updateFreeRoamInDanger()
        {
            if (_wasTakingCover && _takeCoverTimer <= float.Epsilon)
                _wasTakingCover = false;

            bool hasToReposition;

            if (_wasTakingCover || _actor.Cover != null)
                hasToReposition = Vector3.Distance(Leader.transform.position, transform.position) > FreeRoamInDanger.MaxDistance;
            else
                hasToReposition = true;

            if (hasToReposition)
            {
                var hasToMoveAlong = true;

                if (_coverSearchTimer <= float.Epsilon)
                {
                    _takeCoverCheck = false;
                    Message("OnMaxCoverPivotDistance", FreeRoamInDanger.MaxDistance);
                    Message("ToFindDefenseCover", Leader.transform.position);

                    if (_takeCoverCheck)
                    {
                        _wasTakingCover = true;
                        _takeCoverTimer = 2;
                    }
                    else
                        hasToMoveAlong = true;

                    _coverSearchTimer = 0.5f;
                }

                if (hasToMoveAlong)
                {
                    var shift = Vector3.zero;

                    if (_hadPreviousLeaderPosition)
                        shift = Leader.transform.position - _previousLeaderPosition;

                    var vector = Leader.transform.position - transform.position;
                    var distance = vector.magnitude;

                    if (distance > MaxDistance ||
                        (distance > FreeRoamInDanger.FollowDistance && shift.magnitude > 0.1f))
                    {
                        Vector3 direction;

                        if (shift.magnitude > 0.1f &&
                            Util.DistanceToSegment(Leader.transform.position, transform.position, transform.position + shift * 1000) < MaxDistance)
                            direction = shift.normalized;
                        else
                            direction = vector.normalized;

                        if (!Util.IsFree(gameObject, transform.position + Vector3.up * 0.25f, direction, 2, false, true))
                            MoveTo(Leader.transform.position - vector.normalized * FreeRoamInDanger.FollowDistance, 0.5f);
                        else if (!_isRegrouping)
                        {
                            if (Leader._isRunning && (_isRunning || canRun))
                                Message("ToRunInDirection", direction);
                            else
                                Message("ToWalkInDirection", direction);
                        }
                    }
                }
            }
        }

        private void updateFreeRoamOutOfDanger()
        {
            var shift = Vector3.zero;

            if (_hadPreviousLeaderPosition)
                shift = Leader.transform.position - _previousLeaderPosition;

            var vector = Leader.transform.position - transform.position;
            var distance = vector.magnitude;

            if (distance > MaxDistance ||
                (distance > FreeRoamOutOfDanger.FollowDistance && shift.magnitude > 0.1f))
            {
                Vector3 direction;

                if (shift.magnitude > 0.1f &&
                    Util.DistanceToSegment(Leader.transform.position, transform.position, transform.position + shift * 1000) < MaxDistance)
                    direction = shift.normalized;
                else
                    direction = vector.normalized;

                if (!Util.IsFree(gameObject, transform.position + Vector3.up * 0.25f, direction, 2, false, true))
                    MoveTo(Leader.transform.position - vector.normalized * FreeRoamOutOfDanger.FollowDistance, 0.5f);
                else if (!_isRegrouping)
                {
                    if (Leader._isRunning && (_isRunning || canRun))
                        Message("ToRunInDirection", direction);
                    else
                        Message("ToWalkInDirection", direction);
                }
            }
        }
    }
}
