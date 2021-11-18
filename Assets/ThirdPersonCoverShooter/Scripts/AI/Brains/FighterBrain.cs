using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Possible states for the fighting AI to take.
    /// </summary>
    public enum FighterState
    {
        none,
        idle,
        idleButAlerted,
        patrol,
        standAndFight,
        keepCloseTo,
        maintainPosition,
        takeCover,
        takeDefenseCover,
        switchCover,
        retreatToCover,
        fightInCover,
        hideInCover,
        avoidAndFight,
        runAway,
        avoidGrenade,
        circle,
        assault,
        search,
        investigate,
        flee,
        call,
        process
    }

    [Serializable]
    public struct FighterGrenadeAvoidSettings
    {
        /// <summary>
        /// Time in seconds for AI to react to grenades.
        /// </summary>
        [Tooltip("Time in seconds for AI to react to grenades.")]
        public float ReactionTime;

        /// <summary>
        /// Time in seconds to keep running from a threatening grenade.
        /// </summary>
        [Tooltip("Time in seconds to keep running from a threatening grenade.")]
        public float Duration;

        public static FighterGrenadeAvoidSettings Default()
        {
            var settings = new FighterGrenadeAvoidSettings();
            settings.ReactionTime = 1;
            settings.Duration = 2;

            return settings;
        }
    }

    [Serializable]
    public struct FighterRetreatSettings
    {
        /// <summary>
        /// Health value at which the AI will retreat.
        /// </summary>
        [Tooltip("Health value at which the AI will retreat.")]
        public float Health;

        /// <summary>
        /// Duration in seconds the frightened AI will wait and hide in cover before peeking again.
        /// </summary>
        [Tooltip("Duration in seconds the frightened AI will wait and hide in cover before peeking again.")]
        public float HideDuration;

        public static FighterRetreatSettings Default()
        {
            var settings = new FighterRetreatSettings();
            settings.Health = 25;
            settings.HideDuration = 3;

            return settings;
        }
    }

    [Serializable]
    public struct FighterInvestigationWaitSettings
    {
        /// <summary>
        /// Time in seconds to wait before going to inspect last seen covered enemy position.
        /// </summary>
        [Tooltip("Time in seconds to wait before going to inspect last seen covered enemy position.")]
        public float WaitForCovered;

        /// <summary>
        /// Time in seconds to wait before going to inspect last seen uncovered enemy position.
        /// </summary>
        [Tooltip("Time in seconds to wait before going to inspect last seen uncovered enemy position.")]
        public float WaitForUncovered;

        public static FighterInvestigationWaitSettings Default()
        {
            var settings = new FighterInvestigationWaitSettings();
            settings.WaitForCovered = 10;
            settings.WaitForUncovered = 10;

            return settings;
        }
    }

    [Serializable]
    public struct FighterSpeedSettings
    {
        /// <summary>
        /// Should the fighter brain override speed parameters inside CharacterMotor during various states.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// CharacterMotor Speed during a patrol.
        /// </summary>
        public float Patrol;

        /// <summary>
        /// CharacterMotor Speed when taking cover.
        /// </summary>
        public float TakeCover;

        /// <summary>
        /// CharacterMotor Speed when switching covers.
        /// </summary>
        public float SwitchCover;

        /// <summary>
        /// CharacterMotor Speed when retreating to cover.
        /// </summary>
        public float RetreatToCover;

        /// <summary>
        /// CharacterMotor Speed when avoiding (avoiding close contact).
        /// </summary>
        public float Avoid;

        /// <summary>
        /// CharacterMotor Speed when circling.
        /// </summary>
        public float Circle;

        /// <summary>
        /// CharacterMotor Speed when assaulting an enemy.
        /// </summary>
        public float Assault;

        /// <summary>
        /// CharacterMotor Speed when searching for threats.
        /// </summary>
        public float Search;

        /// <summary>
        /// CharacterMotor Speed when investigating.
        /// </summary>
        public float Investigate;

        /// <summary>
        /// CharacterMotor Speed when fleeing.
        /// </summary>
        public float Flee;

        /// <summary>
        /// Constructs and returns default fighter speed settings.
        /// </summary>
        /// <returns></returns>
        public static FighterSpeedSettings Default()
        {
            var settings = new FighterSpeedSettings();
            settings.Enabled = false;
            settings.Patrol = 1.0f;
            settings.TakeCover = 1.0f;
            settings.SwitchCover = 1.0f;
            settings.RetreatToCover = 1.0f;
            settings.Avoid = 1.0f;
            settings.Circle = 1.0f;
            settings.Assault = 1.0f;
            settings.Search = 1.0f;
            settings.Investigate = 1.0f;
            settings.Flee = 1.0f;

            return settings;
        }
    }

    [Serializable]
    public struct Threat
    {
        /// <summary>
        /// Actor component of the threat gameobject.
        /// </summary>
        public Actor Actor;

        /// <summary>
        /// Suspected (or real) position the threat is at.
        /// </summary>
        public Vector3 Position;
    }

    /// <summary>
    /// Simulates a fighter, takes cover, assaults, uses weapons, throws grenades, etc.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterMotor))]
    public class FighterBrain : BaseBrain, IAlertListener, ICharacterHealthListener
    {
        #region Properties

        /// <summary>
        /// Is the AI currently alarmed.
        /// </summary>
        public bool IsAlerted
        {
            get
            {
                var state = State;

                return state != FighterState.none &&
                       state != FighterState.idleButAlerted &&
                       state != FighterState.idle &&
                       state != FighterState.patrol &&
                       (state != FighterState.search || Threat != null);
            }
        }

        /// <summary>
        /// AI state the brain is at.
        /// </summary>
        public FighterState State
        {
            get
            {
                if (_futureSetState != FighterState.none)
                    return _futureSetState;
                else
                    return _state;
            }
        }

        /// <summary>
        /// Time in seconds to wait for inspecting the last seen threat position.
        /// </summary>
        public float InvestigationWait
        {
            get { return ThreatCover ? Investigation.WaitForCovered : Investigation.WaitForUncovered; }
        }

        /// <summary>
        /// Is the AI currently in an agressive mode. Aggressivness determines if the AI immediately attacks threats seen.
        /// </summary>
        public bool IsInAggressiveMode
        {
            get { return _isInAggressiveMode; }
        }

        /// <summary>
        /// Threat actor that the AI is locked on. Other threats are ignored.
        /// </summary>
        public Actor LockedThreat
        {
            get { return _lockedTarget; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Enemy distance to trigger slow retreat.
        /// </summary>
        [Tooltip("Enemy distance to trigger slow retreat.")]
        public float AvoidDistance = 4;

        /// <summary>
        /// Enemy distance to trigger slow retreat.
        /// </summary>
        [Tooltip("AI will avoid standing to allies closer than this distance.")]
        public float AllySpacing = 1.2f;

        /// <summary>
        /// Duration in seconds to stand fighting before changing state.
        /// </summary>
        [Tooltip("Duration in seconds to stand fighting before changing state.")]
        public float StandDuration = 2;

        /// <summary>
        /// Duration in seconds to fight circling the enemy before changing state.
        /// </summary>
        [Tooltip("Duration in seconds to fight circling the enemy before changing state.")]
        public float CircleDuration = 2;

        /// <summary>
        /// Time in seconds for the AI to wait before switching to a better cover.
        /// </summary>
        [Tooltip("Time in seconds for the AI to wait before switching to a better cover.")]
        public float CoverSwitchWait = 10;

        /// <summary>
        /// Distance at which the AI guesses the position on hearing instead of knowing it precisely.
        /// </summary>
        [Tooltip("Distance at which the AI guesses the position on hearing instead of knowing it precisely.")]
        public float GuessDistance = 30;

        /// <summary>
        /// Chance the AI will take cover immediately after learning of existance of an enemy.
        /// </summary>
        [Tooltip("Chance the AI will take cover immediately after learning of existance of an enemy.")]
        [Range(0, 1)]
        public float TakeCoverImmediatelyChance = 0;

        /// <summary>
        /// AI won't go to cover if it is closer to the enemy than this value. Only used when the enemy has been known for awhile.
        /// </summary>
        [Tooltip("AI won't go to cover if it is closer to the enemy than this value. Only used when the enemy has been known for awhile.")]
        public float DistanceToGoToCoverFromStandOrCircle = 6;

        /// <summary>
        /// Should the AI attack threats immedietaly on seeing them.
        /// </summary>
        [Tooltip("Should the AI attack threats immedietaly on seeing them.")]
        public bool ImmediateThreatReaction = true;

        /// <summary>
        /// Should the AI switch to attacking enemies that deal damage to the AI.
        /// </summary>
        [Tooltip("Should the AI switch to attacking enemies that deal damage to the AI.")]
        public bool AttackAggressors = true;

        /// <summary>
        /// Settings for AI startup.
        /// </summary>
        [Tooltip("Settings for AI startup.")]
        public AIStartSettings Start = AIStartSettings.Default();

        /// <summary>
        /// Speed of the motor during various AI states.
        /// </summary>
        [Tooltip("Speed of the motor during various AI states.")]
        public FighterSpeedSettings Speed = FighterSpeedSettings.Default();

        /// <summary>
        /// How accurately the AI guesses the position of an enemy.
        /// </summary>
        [Tooltip("How accurately the AI guesses the position of an enemy.")]
        public AIApproximationSettings Approximation = new AIApproximationSettings(0, 10, 5, 30);

        /// <summary>
        /// Settings for AI retreats.
        /// </summary>
        [Tooltip("Settings for AI retreats.")]
        public FighterRetreatSettings Retreat = FighterRetreatSettings.Default();

        /// <summary>
        /// Settings for how long the AI waits before investigating.
        /// </summary>
        [Tooltip("Settings for how long the AI waits before investigating.")]
        public FighterInvestigationWaitSettings Investigation = FighterInvestigationWaitSettings.Default();

        /// <summary>
        /// Settings for how the fighter avoids other grenades.
        /// </summary>
        [Tooltip("Settings for how the fighter avoids other grenades.")]
        public FighterGrenadeAvoidSettings GrenadeAvoidance = FighterGrenadeAvoidSettings.Default();

        /// <summary>
        /// Settings for AI grenades.
        /// </summary>
        [Tooltip("Settings for AI fighting and aiming.")]
        public AIGrenadeSettings Grenades = AIGrenadeSettings.Default();

        /// <summary>
        /// Should a debug line be drawn towards the current threat.
        /// </summary>
        [Tooltip("Should a debug line be drawn towards the current threat.")]
        public bool DebugThreat = false;

        #endregion

        #region Private fields

        private CharacterMotor _motor;
        private CharacterInventory _inventory;
        private CharacterHealth _health;
        private AISight _sight;

        private bool _isInAggressiveMode;

        public int _thrownGrenadeCount;

        private HashSet<BaseBrain> _friends = new HashSet<BaseBrain>();
        private HashSet<Actor> _friendsThatCanSeeMe = new HashSet<Actor>();
        private HashSet<Actor> _visibleCivilians = new HashSet<Actor>();

        private FighterState _previousState;
        private FighterState _state;
        private float _stateTime;

        private Vector3 _maintainPosition;
        private bool _maintainPositionIndefinitely;
        private float _maintainDuration;
        private bool _hasReachedMaintainPosition;
        private float _maintainPositionReachTime;
        private AIBaseRegrouper _regrouper;

        private Vector3 _defensePosition;

        private bool _failedToAvoidInThisState;

        private Vector3 _avoidedGrenadePosition;
        private float _avoidedGrenadeRange;
        private float _grenadeAvoidReaction;

        private float _grenadeTimer;
        private float _grenadeCheckTimer;
        private bool _hasThrowFirstGrenade;
        private Vector3[] _grenadePath = new Vector3[128];

        private FighterState _futureSetState;
        private bool _hasFailedToFindCover;
        private bool _hasSucceededToFindCover;
        private bool _isLookingForCover;

        private bool _wasAlerted;
        private bool _wasAlarmed;

        private bool _assaultCheck;
        private bool _callCheck;
        private bool _investigationCheck;
        private bool _searchCheck;
        private bool _waypointCheck;

        private bool _isInDarkness;

        private Actor _lockedTarget;

        private bool _isCombatProcess;

        private List<Actor> _visibleActors = new List<Actor>();

        private KeepCloseTo _keepCloseTo;
        private Vector3 _moveTarget;
        private bool _hasMoveTarget;
        private bool _hadMoveTargetBeforeAllySpacing;
        private Vector3 _moveTargetBeforeAllySpacing;
        private bool _hasOpenFire;

        private bool _hasCheckedIfTheLastKnownPositionIsNearCover;

        private NavMeshPath _reachablePath;
        private bool _hasCheckedReachabilityAndFailed;
        private float _reachabilityCheckTime;

        private float _invisibleTime;

        #endregion

        #region Commands

        /// <summary>
        /// Registers that the AI is currently firing.
        /// </summary>
        public void ToOpenFire()
        {
            _hasOpenFire = true;
        }

        /// <summary>
        /// Registers that the AI has stopped firing.
        /// </summary>
        public void ToCloseFire()
        {
            _hasOpenFire = false;
        }

        /// <summary>
        /// Registers that the AI has started sprinting to a location.
        /// </summary>
        public void ToSprintTo(Vector3 position)
        {
            _moveTarget = position;
            _hasMoveTarget = true;
            if (_hadMoveTargetBeforeAllySpacing)
                _moveTargetBeforeAllySpacing = position;
        }

        /// <summary>
        /// Registers that the AI has started running to a location.
        /// </summary>
        public void ToRunTo(Vector3 position)
        {
            _moveTarget = position;
            _hasMoveTarget = true;
            if (_hadMoveTargetBeforeAllySpacing)
                _moveTargetBeforeAllySpacing = position;
        }

        /// <summary>
        /// Registers that the AI has started walking to a location.
        /// </summary>
        public void ToWalkTo(Vector3 position)
        {
            _moveTarget = position;
            _hasMoveTarget = true;
            if (_hadMoveTargetBeforeAllySpacing)
                _moveTargetBeforeAllySpacing = position;
        }

        /// <summary>
        /// Registers that the AI has stopped moving.
        /// </summary>
        public void ToStopMoving()
        {
            _hasMoveTarget = false;
            _hadMoveTargetBeforeAllySpacing = false;
        }

        /// <summary>
        /// Registers that the AI has no target location to move to.
        /// </summary>
        public void ToCircle(Vector3 threat)
        {
            _hasMoveTarget = false;
        }

        /// <summary>
        /// Registers that the AI has no target location to move to.
        /// </summary>
        public void ToWalkInDirection(Vector3 vector)
        {
            _hasMoveTarget = false;
        }

        /// <summary>
        /// Registers that the AI has no target location to move to.
        /// </summary>
        public void ToRunInDirection(Vector3 vector)
        {
            _hasMoveTarget = false;
        }

        /// <summary>
        /// Registers that the AI has no target location to move to.
        /// </summary>
        public void ToSprintInDirection(Vector3 vector)
        {
            _hasMoveTarget = false;
        }

        /// <summary>
        /// Registers that the AI has no target location to move to.
        /// </summary>
        public void ToWalkFrom(Vector3 target)
        {
            _hasMoveTarget = false;
        }

        /// <summary>
        /// Registers that the AI has no target location to move to.
        /// </summary>
        public void ToRunFrom(Vector3 target)
        {
            _hasMoveTarget = false;
        }

        /// <summary>
        /// Registers that the AI has no target location to move to.
        /// </summary>
        public void ToSprintFrom(Vector3 target)
        {
            _hasMoveTarget = false;
        }

        /// <summary>
        /// Registers that the AI is told to keep close to a position.
        /// </summary>
        public void ToKeepCloseTo(KeepCloseTo value)
        {
            _keepCloseTo.Distance = value.Distance;

            if (State == FighterState.avoidGrenade)
                return;

            if (Vector3.Distance(transform.position, value.Position) > value.Distance)
                if (!_hasMoveTarget || Vector3.Distance(_moveTarget, value.Position) > value.Distance)
                {
                    _keepCloseTo.Position = value.Position;
                    setState(FighterState.keepCloseTo, true);
                }
        }

        /// <summary>
        /// Sets the AI state to alerted.
        /// </summary>
        public void SetIdleAlertedState()
        {
            setState(FighterState.idleButAlerted);
        }

        /// <summary>
        /// Sets the target threat and starts either taking cover or assaulting.
        /// </summary>
        public void ToAttack(Actor target)
        {
            _lockedTarget = target;
            setSeenThreat(target, target.transform.position, target.Cover);

            if (State == FighterState.maintainPosition)
                takeCoverOrAssault();
        }

        /// <summary>
        /// Enters a process. Is told if can fire during it.
        /// </summary>
        public void ToEnterProcess(bool isFighting)
        {
            _isCombatProcess = isFighting;
            setState(FighterState.process, true);
        }

        /// <summary>
        /// Told that the process has stopped. Sets the idle but alerted AI state.
        /// </summary>
        public void ToExitProcess()
        {
            if (State == FighterState.process)
                setState(FighterState.idleButAlerted, false, true);
        }

        /// <summary>
        /// Told to stop the process and maintain previous position if there was any.
        /// </summary>
        public void ToExitProcessAndMaintainPosition()
        {
            if (State == FighterState.process)
            {
                if (_previousState == FighterState.maintainPosition)
                    setState(_previousState, false, true);
                else
                    setState(FighterState.idleButAlerted, false, true);
            }
        }

        /// <summary>
        /// Locks the AI on the given threat.
        /// </summary>
        public void ToSetThreat(Threat threat)
        {
            _lockedTarget = threat.Actor;
            setSeenThreat(threat.Actor, threat.Position, null);
        }

        /// <summary>
        /// Starts maintaing the position.
        /// </summary>
        public void ToHoldPosition(Vector3 position)
        {
            _maintainPosition = position;
            _maintainPositionIndefinitely = true;
            setState(FighterState.maintainPosition, true);
        }

        /// <summary>
        /// Removes threat and sets the AI state to idleButAlerted.
        /// </summary>
        public void ToStopActions()
        {
            RemoveThreat();
            setState(FighterState.idleButAlerted);
        }

        /// <summary>
        /// Forget about the enemy and return to patrol or idle state.
        /// </summary>
        public override void ToForget()
        {
            base.ToForget();
            setState(FighterState.patrol);
        }

        /// <summary>
        /// Told by a component to be scared.
        /// </summary>
        public void ToBecomeScared()
        {
            setState(FighterState.flee);
        }

        /// <summary>
        /// Told by a component to make a call.
        /// </summary>
        public void ToMakeCall()
        {
            setState(FighterState.call);
        }

        /// <summary>
        /// Told by a component to find a cover.
        /// </summary>
        public void ToFindCover()
        {
            setState(FighterState.takeCover, true);
        }

        /// <summary>
        /// Told by a component to find a defensive cover.
        /// </summary>
        public void ToFindDefenseCover(Vector3 position)
        {
            _defensePosition = position;
            setState(FighterState.takeDefenseCover, true);
        }

        /// <summary>
        /// Told by an outside command to regroup around a unit.
        /// </summary>
        public void ToRegroupAround(AIBaseRegrouper regrouper)
        {
            _regrouper = regrouper;

            setState(FighterState.takeCover, true);
        }
        
        /// <summary>
        /// Told by an outside command to regroup and maintain a position.
        /// </summary>        
        public void ToFindAndMaintainPosition(AIBaseRegrouper regrouper)
        {
            _regrouper = regrouper;
            _maintainDuration = regrouper.UncoveredDuration;

            var movement = regrouper.GetComponent<AIMovement>();
            var middle = regrouper.transform.position;

            if (movement != null)
                middle = movement.Destination;

            for (int i = 0; i < 6; i++)
            {
                var radius = UnityEngine.Random.Range(1.0f, regrouper.Radius);
                var angle = UnityEngine.Random.Range(0f, 360f);
                var position = middle + Util.HorizontalVector(angle) * radius;

                if (AIUtil.GetClosestStandablePosition(ref position))
                    if (!regrouper.IsPositionTaken(position))
                    {
                        regrouper.TakePosition(position);
                        _maintainPosition = position;
                        _maintainPositionIndefinitely = false;
                        setState(FighterState.maintainPosition, true);
                        break;
                    }
            }
        }

        #endregion

        #region Checks

        /// <summary>
        /// Registers existance of an assault component.
        /// </summary>
        public void AssaultResponse()
        {
            _assaultCheck = true;
        }

        /// <summary>
        /// Registers that a call is being made.
        /// </summary>
        public void CallResponse()
        {
            _callCheck = true;
        }

        /// <summary>
        /// Registers existance of an investigation component.
        /// </summary>
        public void InvestigationResponse()
        {
            _investigationCheck = true;
        }

        /// <summary>
        /// Registers existance of a search component.
        /// </summary>
        public void SearchResponse()
        {
            _searchCheck = true;
        }

        /// <summary>
        /// Returns true if there is a component that handles assaults.
        /// </summary>
        private bool tryAssault()
        {
            _assaultCheck = false;
            Message("AssaultCheck", LastKnownThreatPosition);
            return _assaultCheck;
        }

        /// <summary>
        /// Returns true if there is a component that handles investigations.
        /// </summary>
        private bool tryInvestigate()
        {
            _investigationCheck = false;
            Message("InvestigationCheck");
            return _investigationCheck;
        }

        /// <summary>
        /// Returns true if there is a component that handles searches.
        /// </summary>
        private bool trySearch()
        {
            _searchCheck = false;
            Message("SearchCheck");
            return _searchCheck;
        }

        #endregion

        #region Events

        /// <summary>
        /// Notified that a search has finished. Forgets about the previous threat as nothing was found during the search.
        /// </summary>
        public void OnFinishSearch()
        {
            ToForget();
        }

        /// <summary>
        /// Notified that the AI has been resurected.
        /// </summary>
        public void OnResurrect()
        {
            setState(FighterState.idleButAlerted, true);
        }

        public void OnDead()
        {
        }

        /// <summary>
        /// Notified of an end of an assault.
        /// </summary>
        public void OnAssaultStop()
        {
            if (State == FighterState.assault)
                setState(FighterState.circle);
        }

        /// <summary>
        /// A death was witnessed.
        /// </summary>
        public void OnSeeDeath(Actor actor)
        {
            if (_visibleActors.Contains(actor))
                _visibleActors.Remove(actor);

            if (actor != Threat)
                return;

            RemoveThreat();

            var minDistance = 0f;
            Actor threat = null;

            for (int i = 0; i < _visibleActors.Count; i++)
            {
                var other = _visibleActors[i];

                if (other.Side != Actor.Side && other.IsAlive)
                {
                    var distance = Vector3.Distance(other.transform.position, transform.position);

                    if (distance < minDistance || threat == null)
                    {
                        threat = other;
                        minDistance = distance;
                    }
                }
            }

            if (threat != null)
                setSeenThreat(threat, threat.transform.position, threat.Cover);
            else if (canLeavePosition)
            {
                if (tryInvestigate())
                    setState(FighterState.investigate);
                else if (trySearch())
                    setState(FighterState.search);
                else
                    fightOrRunAway();
            }
        }

        /// <summary>
        /// Notified that the waypoint system has waypoints to visit.
        /// </summary>
        public void OnWaypointsFound()
        {
            _waypointCheck = true;

            if (State == FighterState.patrol && _isInDarkness)
                Message("ToLight");
        }

        /// <summary>
        /// Event called during a spawning process.
        /// </summary>
        public void OnSpawn(Actor caller)
        {
            var brain = caller != null ? caller.GetComponent<BaseBrain>() : null;

            if (brain != null)
                setThreat(false, false, false, true, brain.Threat, brain.LastKnownThreatPosition, brain.ThreatCover, brain.LastSeenThreatTime);
            else if (caller != null)
                setThreat(false, false, false, false, null, caller.transform.position, null, Time.timeSinceLevelLoad);
            else if (trySearch())
                setState(FighterState.search);
            else
                setState(FighterState.patrol);
        }

        /// <summary>
        /// One of the components declares a need for a light.
        /// </summary>
        public void OnNeedLight()
        {
            _isInDarkness = true;

            if (State == FighterState.patrol || State == FighterState.investigate || State == FighterState.search)
                Message("ToLight");
            else
                Message("ToTurnOnLight");
        }

        /// <summary>
        /// One of the components declares that a light is no longer needed.
        /// </summary>
        public void OnDontNeedLight()
        {
            _isInDarkness = false;

            if (State == FighterState.patrol || State == FighterState.investigate || State == FighterState.search)
                Message("ToHideFlashlight");
            else
                Message("ToUnlight");
        }

        /// <summary>
        /// Registers a successful call.
        /// </summary>
        public void OnCallMade()
        {
            if (State == FighterState.call)
                setState(_previousState);
        }

        /// <summary>
        /// Registers damage done by a weapon.
        /// </summary>
        public void OnHit(Hit hit)
        {
            if (hit.Attacker != null && canSetThreat && (Threat == null || !Threat.IsAlive || AttackAggressors))
            {
                var threat = hit.Attacker.GetComponent<Actor>();

                if (threat != null && threat.Side != Actor.Side && canChangeTarget)
                {
                    if (_visibleActors.Contains(threat) || Vector3.Distance(threat.transform.position, transform.position) <= GuessDistance)
                        setSeenThreat(threat, threat.transform.position, threat.Cover);
                    else
                        guessThreat(threat, threat.transform.position, true);
                }
            }

            if (_health != null && _health.Health <= Retreat.Health + float.Epsilon && State != FighterState.flee)
            {
                if ((Actor.Cover == null && canLeavePosition) || Threat == null || !Threat.IsAlive)
                    setState(FighterState.retreatToCover);
                else if (_state == FighterState.hideInCover && _stateTime > 0.5f)
                    setState(FighterState.fightInCover);
                else if (_state == FighterState.fightInCover && _stateTime > 0.5f)
                    setState(FighterState.hideInCover);
            }

            foreach (var friend in _friends)
                friend.Message("OnFriendHit", Actor);
        }

        /// <summary>
        /// Notified that a friend was hit.
        /// </summary>
        public void OnFriendHit(Actor friend)
        {
            if (canSetThreat && (Threat == null || !Threat.IsAlive || AttackAggressors))
            {
                var brain = friend.GetComponent<FighterBrain>();

                if (brain != null)
                {
                    var threat = brain.Threat;

                    if (threat != null && threat.Side != Actor.Side && canChangeTarget)
                    {
                        if (_visibleActors.Contains(threat))
                            setSeenThreat(threat, threat.transform.position, threat.Cover);
                        else if (Vector3.Distance(threat.transform.position, transform.position) <= GuessDistance)
                            setUnseenThreat(false, true, brain.CanSeeTheThreat, threat, threat.transform.position, threat.Cover);
                        else
                            guessThreat(threat, threat.transform.position, false);
                    }
                }
            }
        }

        /// <summary>
        /// Notified by cover AI that target cover was taken.
        /// </summary>
        public void OnFinishTakeCover()
        {
            switch (State)
            {
                case FighterState.takeCover:
                case FighterState.switchCover:
                case FighterState.takeDefenseCover:
                    if (Threat != null && Threat.IsAlive)
                        setState(FighterState.fightInCover);
                    else
                        setState(FighterState.hideInCover);
                    break;

                case FighterState.retreatToCover:
                    setState(FighterState.hideInCover);
                    break;
            }
        }

        /// <summary>
        /// Notified by an alert system of an alert.
        /// </summary>
        public void OnAlert(ref GeneratedAlert alert)
        {
            if (!canChangeTarget)
                return;

            if (alert.Actor != null && alert.Actor.gameObject != gameObject)
            {
                if (alert.Actor.Side != Actor.Side)
                {
                    if (alert.Actor.IsAlive)
                    {
                        if (Threat == null ||
                            !Threat.IsAlive ||
                            (alert.Actor == Threat && !CanSeeTheThreat && alert.IsDirect) ||
                            (InvestigationWait < ThreatAge && !CanSeeTheThreat))
                        {
                            if (Vector3.Distance(alert.Position, transform.position) >= GuessDistance)
                                guessThreat(alert.Actor, alert.Position, true);
                            else
                                setUnseenThreat(true, false, false, alert.Actor, alert.Position, null);
                        }
                    }
                }
                else
                {
                    if (Threat == null || !Threat.IsAlive || InvestigationWait < ThreatAge)
                    {
                        var brain = alert.Actor.GetComponent<BaseBrain>();

                        if (brain != null)
                        {
                            if (brain.Threat != null && brain.CanSeeTheThreat && brain.Threat.IsAlive)
                                setUnseenThreat(false, false, true, brain.Threat, alert.Position, null);
                        }
                        else if (alert.IsHostile)
                            setUnseenThreat(false, false, false, null, alert.Position, null);
                    }
                }
            }
        }

        /// <summary>
        /// Notified by communication AI that a friend was found.
        /// </summary>
        public void OnFoundFriend(Actor friend)
        {
            var brain = friend.GetComponent<FighterBrain>();

            if (brain != null && !_friends.Contains(brain))
            {
                _friends.Add(brain);

                if (brain.Threat != null)
                {
                    if (brain.HasSeenTheEnemy && brain.ThreatAge < brain.InvestigationWait)
                        Message("OnFriendFoundEnemy", friend);
                    else
                        Message("OnFriendKnowsEnemy", friend);
                }
            }
        }

        /// <summary>
        /// Notified by communication AI that a friend got out of range.
        /// </summary>
        public void OnLostFriend(Actor friend)
        {
            var brain = friend.GetComponent<BaseBrain>();

            if (brain != null && _friends.Contains(brain))
                _friends.Remove(brain);
        }

        /// <summary>
        /// Notified that a civilian is alerted.
        /// </summary>
        public void OnCivilianAlerted(Actor actor)
        {
            if (Threat == null || !Threat.IsAlive)
            {
                if (!canSetThreat)
                    return;

                var brain = actor.GetComponent<BaseBrain>();

                if (brain != null)
                    setThreat(false, false, false, false, brain.Threat, actor.transform.position, null, Time.timeSinceLevelLoad);
            }
        }

        /// <summary>
        /// Notified by a friend that they found a new enemy position.
        /// </summary>
        public void OnFriendKnowsEnemy(Actor friend)
        {
            if (friend == null || friend.Side != Actor.Side)
                return;

            if (Threat != null && !Threat.IsAlive)
                return;

            if (!canSetThreat)
                return;

            var brain = friend.GetComponent<FighterBrain>();
            if (brain == null)
                return;

            if (brain.Threat != Threat && brain.Threat != ForgottenThreat)
                setThreat(false, false, brain.IsActualThreatPosition, brain.IsActualThreatPosition, brain.Threat, brain.LastKnownThreatPosition, brain.ThreatCover, brain.LastSeenThreatTime);
        }

        /// <summary>
        /// Notified by a friend that they found a new enemy position.
        /// </summary>
        public void OnFriendFoundEnemy(Actor friend)
        {
            if (friend == null || friend.Side != Actor.Side)
                return;

            var brain = friend.GetComponent<FighterBrain>();
            if (brain == null || !brain.HasSeenTheEnemy || !brain.IsActualThreatPosition)
                return;

            if (Threat != null && Threat.IsAlive && CanSeeTheThreat)
                return;

            if (!canSetThreat)
                return;

            var isOk = false;

            if (Threat == null || !Threat.IsAlive)
                isOk = true;
            else if (!HasSeenTheEnemy)
                isOk = brain.LastSeenThreatTime > LastSeenThreatTime + 0.1f;
            else if (!IsActualThreatPosition)
                isOk = brain.LastSeenThreatTime > LastSeenThreatTime + 0.1f;
            else if (InvestigationWait < ThreatAge && brain.InvestigationWait > brain.ThreatAge)
                isOk = true;

            if (isOk)
                setThreat(false, false, brain.IsActualThreatPosition, brain.IsActualThreatPosition, brain.Threat, brain.LastKnownThreatPosition, brain.ThreatCover, brain.LastSeenThreatTime);
        }

        /// <summary>
        /// Notified by a friend that the AI is seen by them.
        /// </summary>
        public void OnSeenByFriend(Actor friend)
        {
            if (!_friendsThatCanSeeMe.Contains(friend))
                _friendsThatCanSeeMe.Add(friend);
        }

        /// <summary>
        /// Notified by a friend that the AI is no longer visible by them.
        /// </summary>
        public void OnUnseenByFriend(Actor friend)
        {
            if (_friendsThatCanSeeMe.Contains(friend))
                _friendsThatCanSeeMe.Remove(friend);
        }


        /// <summary>
        /// Notified by the sight AI that an actor has entered the view.
        /// </summary>
        public void OnSeeActor(Actor actor)
        {
            _visibleActors.Add(actor);

            if (actor.Side == Actor.Side)
            {
                if (actor.IsAggressive)
                    actor.SendMessage("OnSeenByFriend", Actor, SendMessageOptions.DontRequireReceiver);
                else
                    _visibleCivilians.Add(actor);
            }
            else if (canChangeTarget && canSetThreat)
                if (Threat == null || 
                    !Threat.IsAlive ||
                    InvestigationWait < ThreatAge || 
                    Threat == actor)
                    setSeenThreat(actor, actor.transform.position, actor.Cover);
        }

        /// <summary>
        /// Notified by the sight AI that an actor has dissappeared from the view.
        /// </summary>
        public void OnUnseeActor(Actor actor)
        {
            _visibleActors.Remove(actor);

            if (actor.Side == Actor.Side)
            {
                if (actor.IsAggressive)
                    actor.SendMessage("OnUnseenByFriend", Actor, SendMessageOptions.DontRequireReceiver);
                else
                    _visibleCivilians.Remove(actor);
            }
            else if (Threat == actor)
            {
                UnseeThreat();

                if (State == FighterState.standAndFight && canLeavePosition)
                {
                    if (ThreatCover == null && tryInvestigate())
                        setState(FighterState.investigate);
                    else
                        takeCoverOrAssault();
                }
            }
        }

        /// <summary>
        /// Notified by the cover AI that the current cover is no longer valid.
        /// </summary>
        public void OnInvalidCover()
        {
            if (State == FighterState.takeCover || State == FighterState.fightInCover)
                setState(FighterState.takeCover, true);
            else if (State == FighterState.retreatToCover || State == FighterState.hideInCover)
                setState(FighterState.retreatToCover, true);
        }

        /// <summary>
        /// Notified by the cover AI that a cover was found.
        /// </summary>
        public void OnFoundCover()
        {
            _hasSucceededToFindCover = true;
            _hasFailedToFindCover = false;
        }

        /// <summary>
        /// Notified that no cover was found.
        /// </summary>
        public void OnNoCover()
        {
            _hasSucceededToFindCover = false;
            _hasFailedToFindCover = true;
        }

        /// <summary>
        /// Notified that a component has started to look for covers.
        /// </summary>
        public void OnCoverSearch()
        {
            _isLookingForCover = true;
            _hasSucceededToFindCover = false;
            _hasFailedToFindCover = false;
        }

        /// <summary>
        /// Notified by the movement AI that circling of an enemy is no longer viable.
        /// </summary>
        public void OnCircleFail()
        {
            if (State == FighterState.circle)
            {
                if (tryInvestigate())
                    setState(FighterState.investigate);
                else
                    takeCoverOrAssault();
            }
        }

        /// <summary>
        /// Notified by the movement AI that a position can no longer be retreated from.
        /// </summary>
        public void OnMoveFromFail()
        {
            if (State == FighterState.avoidAndFight)
            {
                _failedToAvoidInThisState = true;

                fightOrRunAway();
            }
        }

        /// <summary>
        /// Notified by the search AI that a position has been investigated.
        /// </summary>
        public void OnPointInvestigated(Vector3 position)
        {
            if (State == FighterState.investigate)
            {
                if (Vector3.Distance(LastKnownThreatPosition, position) < 0.5f)
                {
                    if (IsActualThreatPosition && (Time.timeSinceLevelLoad - LastSeenThreatTime) < 1)
                        fightOrRunAway();
                    else if (trySearch())
                        setState(FighterState.search);
                    else
                        fightOrRunAway();
                }
                else
                    Message("ToInvestigatePosition", LastKnownThreatPosition);
            }
        }

        #endregion

        #region Behaviour

        protected override void Awake()
        {
            base.Awake();
            Actor.IsAggressive = true;

            _health = GetComponent<CharacterHealth>();
            _motor = GetComponent<CharacterMotor>();
            _inventory = GetComponent<CharacterInventory>();

            _sight = GetComponent<AISight>();

            _reachablePath = new NavMeshPath();

            switch (Start.Mode)
            {
                case AIStartMode.idle:
                    _futureSetState = FighterState.idle;
                    break;

                case AIStartMode.alerted:
                    _futureSetState = FighterState.idleButAlerted;
                    break;

                case AIStartMode.patrol:
                    _futureSetState = FighterState.patrol;
                    break;

                case AIStartMode.searchAround:
                case AIStartMode.searchPosition:
                    _futureSetState = FighterState.search;
                    break;

                case AIStartMode.investigate:
                    _futureSetState = FighterState.investigate;
                    break;
            }
        }

       
        private void Update()
        {
            if (Actor == null || !Actor.IsAlive)
                return;

            _stateTime += Time.deltaTime;

            if (_futureSetState != FighterState.none)
            {
                var state = _futureSetState;
                _futureSetState = FighterState.none;
                setStateImmediately(state);
            }

            if (Threat == null || !Threat.IsAlive)
                foreach (var civilian in _visibleCivilians)
                {
                    if (civilian.IsAlerted)
                    {
                        OnCivilianAlerted(civilian);
                        break;
                    }
                }

            if (Threat != null && CanSeeTheThreat)
            {
                if (_sight != null)
                    _sight.DoubleCheck(Threat);

                if (CanSeeTheThreat)
                {
                    setSeenThreat(Threat, Threat.transform.position, Threat.Cover);
                    _invisibleTime = 0;
                }
                else
                    _invisibleTime += Time.deltaTime;
            }
            else
                _invisibleTime += Time.deltaTime;

            if (DebugThreat && Threat != null)
                Debug.DrawLine(transform.position, LastKnownThreatPosition, Color.cyan);

            if (canLeavePosition)
                findGrenades();
           // print(transform.name + " State: " + _state.ToString());
            switch (_state)
            {
                case FighterState.none:
                    setState(FighterState.patrol);
                    break;

                case FighterState.idle:
                    break;

                case FighterState.idleButAlerted:
                    break;

                case FighterState.patrol:
                    break;

                case FighterState.search:
                    break;

                case FighterState.investigate:
                    if (_stateTime > 2.5f && _hasOpenFire)
                        Message("ToCloseFire");
                    break;

                case FighterState.avoidGrenade:
                    if (_stateTime > GrenadeAvoidance.Duration || Vector3.Distance(_avoidedGrenadePosition, transform.position) > _avoidedGrenadeRange)
                    {
                        if (_previousState != FighterState.avoidGrenade)
                            setState(_previousState);
                        else
                            setState(FighterState.idleButAlerted);
                    }
                    else
                        Message("ToKeepSprintingFrom", _avoidedGrenadePosition);
                    break;

                case FighterState.process:
                    if (_isCombatProcess)
                    {
                        turnAndAimAtTheThreat();

                        if (Threat != null)
                        {
                            if (!_hasOpenFire)
                                Message("ToOpenFire");
                        }
                        else if (_hasOpenFire)
                            Message("ToCloseFire");
                    }
                    break;

                case FighterState.standAndFight:
                    if (_stateTime >= StandDuration)
                    {
                        if (Vector3.Distance(transform.position, LastKnownThreatPosition) > DistanceToGoToCoverFromStandOrCircle)
                            takeCoverOrAssault();
                        else if (tryAssault())
                            setState(FighterState.assault);
                        else
                            setState(FighterState.circle);
                    }
                    else
                    {
                        turnAndAimAtTheThreat();

                        if (!_failedToAvoidInThisState)
                        {
                            checkAllySpacingAndMoveIfNeeded();
                            checkAvoidanceAndSetTheState();
                        }
                    }

                    checkInvestigationManageFireAndSetTheState(true, true);
                    checkAndThrowGrenade();
                    break;

                case FighterState.keepCloseTo:
                    if (Vector3.Distance(transform.position, _keepCloseTo.Position) < _keepCloseTo.Distance * 0.9f)
                        takeCoverOrAssault();
                    else
                    {
                        turnAndAimAtTheThreat();

                        if (!_failedToAvoidInThisState)
                            checkAvoidanceAndSetTheState();
                    }

                    checkAndThrowGrenade();
                    break;

                case FighterState.maintainPosition:
                    _regrouper = null;

                    if (!_hasReachedMaintainPosition)
                        if (Vector3.Distance(transform.position, _maintainPosition) < 1)
                        {
                            _maintainPositionReachTime = _stateTime;
                            _hasReachedMaintainPosition = true;
                            Message("ToCrouch");
                        }

                    if (!_maintainPositionIndefinitely && _hasReachedMaintainPosition && (_stateTime > _maintainDuration + _maintainPositionReachTime))
                        takeCoverOrAssault();
                    else
                    {
                        if (Threat != null)
                        {
                            if (checkInvestigationManageFireAndSetTheState(true, true, false))
                            {
                                if (_hasOpenFire)
                                    Message("ToCloseFire");

                                RemoveThreat();
                            }
                            else
                                turnAndAimAtTheThreat();
                        }

                        if (!_failedToAvoidInThisState && !_maintainPositionIndefinitely)
                            checkAvoidanceAndSetTheState();
                    }

                    checkAndThrowGrenade();
                    break;

                case FighterState.takeDefenseCover:
                case FighterState.takeCover:
                    if (_hasSucceededToFindCover)
                    {
                        if (_isLookingForCover)
                        {
                            Message("ToArm");

                            if (Threat != null)
                                Message("ToOpenFire");
                            else
                                Message("ToFaceWalkDirection");

                            _isLookingForCover = false;
                        }

                        turnAndAimAtTheThreat();
                        checkAvoidanceAndSetTheState();
                        checkInvestigationManageFireAndSetTheState(false, true);
                    }
                    else if (_hasFailedToFindCover)
                    {
                        if (_regrouper != null)
                            ToFindAndMaintainPosition(_regrouper);
                        else if (Threat == null || !Threat.IsAlive)
                            setState(FighterState.idleButAlerted);
                        else if (tryAssault())
                            setState(FighterState.assault);
                        else
                            setState(FighterState.circle);
                    }
                    break;

                case FighterState.switchCover:
                    if (_hasSucceededToFindCover)
                    {
                        if (_isLookingForCover)
                        {
                            Message("ToArm");

                            if (Threat != null)
                                Message("ToOpenFire");

                            Message("OnCoverSwitch");

                            _isLookingForCover = false;
                        }

                        turnAndAimAtTheThreat();
                        checkAvoidanceAndSetTheState();
                        checkInvestigationManageFireAndSetTheState(false, true);
                    }
                    else
                    {    
                        if (CanSeeTheThreat || Actor.Cover != null)
                            fightOrRunAway();
                        else if (tryInvestigate())
                            setState(FighterState.investigate);
                        else
                            setState(FighterState.takeCover);
                    }
                    break;

                case FighterState.retreatToCover:
                    turnAndAimAtTheThreat();
                    checkAvoidanceAndSetTheState();
                    break;

                case FighterState.fightInCover:
                    turnAndAimAtTheThreat();

                    if (Actor.Cover == null)
                        setState(FighterState.takeCover);
                    else if (_stateTime > CoverSwitchWait)
                        setState(FighterState.switchCover);

                    checkAvoidanceAndSetTheState();
                    checkAllySpacingAndMoveIfNeeded();
                    checkInvestigationManageFireAndSetTheState(true, true);
                    checkAndThrowGrenade();
                    break;

                case FighterState.hideInCover:
                    if (!checkAvoidanceAndSetTheState() && !checkAllySpacingAndMoveIfNeeded())
                        if (_stateTime > Retreat.HideDuration && Threat != null)
                            setState(FighterState.fightInCover);
                    break;

                case FighterState.avoidAndFight:
                    turnAndAimAtTheThreat();

                    if (_stateTime > 2 && !checkAvoidanceAndSetTheState())
                    {
                        if (tryFire())
                            setState(FighterState.standAndFight);
                        else if (tryAssault())
                            setState(FighterState.assault);
                    }

                    checkInvestigationManageFireAndSetTheState(true, true);
                    break;

                case FighterState.runAway:
                    Message("ToFaceWalkDirection");

                    if (_stateTime > 8)
                    {
                        if (tryFire())
                            setState(FighterState.standAndFight);
                        else if (tryAssault())
                            setState(FighterState.assault);
                    }
                    break;

                case FighterState.circle:
                    if (_stateTime >= CircleDuration)
                    {
                        if (Vector3.Distance(transform.position, LastKnownThreatPosition) > DistanceToGoToCoverFromStandOrCircle)
                            takeCoverOrAssault();
                        else
                            fightOrRunAway();
                    }
                    else
                    {
                        turnAndAimAtTheThreat();
                        checkAvoidanceAndSetTheState();
                        checkInvestigationManageFireAndSetTheState(true, true);
                        checkAndThrowGrenade();
                    }
                    break;

                case FighterState.assault:
                    turnAndAimAtTheThreat();
                    checkInvestigationManageFireAndSetTheState(true, false);
                    break;
            }
        }

        #endregion

        #region State

        private void setState(FighterState state, bool forceRestart = false, bool allowCancelProcess = false)
        {
            if (_state == FighterState.process && !allowCancelProcess)
                return;

            if (state == FighterState.search)
            {
            }
            else if (state == FighterState.investigate)
            {
            }

            if (_futureSetState != FighterState.none ||
                _state != state ||
                forceRestart)
            {
                _futureSetState = state;
            }
        }

        private void setStateImmediately(FighterState state)
        {
            if (_state != state)
                _previousState = _state;

            _failedToAvoidInThisState = false;
            _hasOpenFire = false;

            closeState(_state, state);
            _stateTime = 0;
            _state = state;
            openState(_state, _previousState);

            if (IsAlerted)
            {
                if (!_wasAlerted)
                {
                    _wasAlerted = true;
                    Message("OnAlerted");
                }
            }
            else
                _wasAlerted = false;
        }

        private void openState(FighterState state, FighterState previous)
        {
            switch (state)
            {
                case FighterState.none:
                case FighterState.idle:
                    if (Speed.Enabled) _motor.Speed = Speed.Patrol;
                    Message("ToDisarm");

                    if (Start.ReturnOnIdle && Vector3.Distance(transform.position, StartingLocation) > 0.25f)
                    {
                        Message("ToWalkTo", StartingLocation);
                        Message("ToFaceWalkDirection");
                    }
                    else
                        Message("ToStopMoving");

                    break;

                case FighterState.idleButAlerted:
                    if (Speed.Enabled) _motor.Speed = Speed.Patrol;
                    Message("ToArm");
                    Message("ToStopMoving");
                    Message("ToScan");

                    break;

                case FighterState.flee:
                    if (Speed.Enabled) _motor.Speed = Speed.Flee;
                    Message("ToCloseFire");
                    Message("ToLeaveCover");
                    Message("ToStartFleeing", LastKnownThreatPosition);
                    Message("OnScared");
                    alarm();
                    break;

                case FighterState.patrol:
                    if (Speed.Enabled) _motor.Speed = Speed.Patrol;

                    if (Actor.Cover != null)
                        Message("ToLeaveCover");

                    Message("ToDisarm");

                    _waypointCheck = false;
                    Message("ToStartVisitingWaypoints");

                    if (!_waypointCheck)
                        setState(FighterState.idle);

                    break;

                case FighterState.standAndFight:
                    Message("ToStopMoving");
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    alarm();
                    break;

                case FighterState.process:
                    Message("ToStopMoving");

                    if (_isCombatProcess)
                    {
                        Message("ToArm");

                        if (Threat != null)
                        {
                            turnAndAimAtTheThreat();
                            Message("ToOpenFire");
                        }

                        alarm();
                    }

                    break;

                case FighterState.keepCloseTo:
                    Message("ToRunTo", _keepCloseTo.Position);
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    alarm();
                    break;

                case FighterState.maintainPosition:
                    _hasReachedMaintainPosition = false;
                    Message("ToRunTo", _maintainPosition);
                    Message("ToArm");

                    if (Threat != null)
                    {
                        turnAndAimAtTheThreat();
                        Message("ToOpenFire");
                    }
                    else
                        Message("ToFaceWalkDirection");

                    alarm();

                    if (_maintainPositionIndefinitely)
                        Message("OnHoldPosition", _maintainPosition);
                    break;

                case FighterState.takeDefenseCover:
                case FighterState.takeCover:
                case FighterState.retreatToCover:
                    if (Speed.Enabled)
                    {
                        if (state == FighterState.takeCover)
                            _motor.Speed = Speed.TakeCover;
                        else
                            _motor.Speed = Speed.RetreatToCover;
                    }

                    Message("ToRunToCovers");

                    _isLookingForCover = false;
                    _hasSucceededToFindCover = false;
                    _hasFailedToFindCover = true;

                    if (State == FighterState.takeDefenseCover)
                        Message("ToTakeDefenseCover", _defensePosition);
                    else
                    {
                        Message("OnNoMaxCoverPivotDistance");

                        if (_regrouper != null)
                            Message("ToTakeCoverCloseTo", _regrouper);
                        else if (Threat != null)
                            Message("ToTakeCoverAgainst", LastKnownThreatPosition);
                        else
                            Message("ToTakeCover");
                    }

                    _regrouper = null;
                    alarm();

                    if (state == FighterState.retreatToCover)
                        Message("OnRetreat");

                    break;

                case FighterState.switchCover:
                    if (Speed.Enabled)
                        _motor.Speed = Speed.SwitchCover;

                    _isLookingForCover = false;
                    _hasFailedToFindCover = true;
                    _hasSucceededToFindCover = false;

                    Message("ToRunToCovers");
                    Message("ToSwitchCover");
                    break;

                case FighterState.investigate:
                    if (Speed.Enabled) _motor.Speed = Speed.Investigate;

                    if (!HasSeenTheEnemy && Start.Mode == AIStartMode.investigate)
                        SetThreat(false, false, false, null, Start.Position, null, 0);

                    Message("ToArm");
                    
                    if (_isInDarkness)
                        Message("ToLight");

                    turnAndAimAtTheThreat();
                    Message("ToStartAiming");
                    Message("ToCloseFire");

                    if (ThreatCover != null)
                    {
                        if (IsActualThreatPosition && Threat != null && HasSeenTheEnemy)
                        {
                            Message("ToHideFlashlight");
                            Message("ToArm");
                            Message("ToOpenFire");
                        }

                        Message("ToInvestigatePosition", LastKnownThreatPosition);
                    }
                    else if (HasSeenTheEnemy || HasHeardTheEnemy || Start.Mode == AIStartMode.investigate)
                        Message("ToInvestigatePosition", LastKnownThreatPosition);
                    else if (trySearch())
                        setState(FighterState.search);
                    else
                        fightOrRunAway();
                    break;

                case FighterState.fightInCover:
                    if (Actor.Cover == null)
                        setState(FighterState.takeCover);
                    else
                    {
                        Message("ToArm");
                        turnAndAimAtTheThreat();
                        Message("ToOpenFire");
                        alarm();
                    }
                    break;

                case FighterState.hideInCover:
                    Message("ToCloseFire");
                    Message("ToStopAiming");

                    if (Actor.Cover == null)
                        setState(FighterState.retreatToCover);
                    break;

                case FighterState.avoidAndFight:
                    if (Speed.Enabled) _motor.Speed = Speed.Avoid;
                    Message("ToRunFrom", LastKnownThreatPosition);
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    alarm();
                    break;

                case FighterState.runAway:
                    if (Speed.Enabled) _motor.Speed = Speed.Flee;
                    Message("ToRunFrom", LastKnownThreatPosition);
                    alarm();
                    break;

                case FighterState.avoidGrenade:
                    if (Speed.Enabled) _motor.Speed = Speed.Avoid;
                    Message("ToCloseFire");
                    Message("ToSprintFrom", _avoidedGrenadePosition);
                    alarm();
                    break;

                case FighterState.circle:
                    if (Speed.Enabled) _motor.Speed = Speed.Circle;
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    Message("ToCircle", LastKnownThreatPosition);
                    alarm();
                    break;

                case FighterState.assault:
                    if (Speed.Enabled) _motor.Speed = Speed.Assault;
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    Message("ToStartAssault", LastKnownThreatPosition);
                    alarm();
                    break;

                case FighterState.search:
                    if (Speed.Enabled) _motor.Speed = Speed.Search;

                    if (Threat == null)
                    {
                        if (previous == FighterState.none && Start.Mode == AIStartMode.searchPosition)
                            Message("ToSearchAt", new SearchPoint(Start.Position, (transform.position - Start.Position).normalized, false));
                        else
                            Message("ToSearch");
                    }
                    else
                        Message("ToSearchAt", new SearchPoint(LastKnownThreatPosition, ThreatCover != null ? (-ThreatCover.Forward) : (transform.position - LastKnownThreatPosition).normalized, ThreatCover == null));

                    Message("ToArm");

                    if (_isInDarkness)
                        Message("ToLight");

                    Message("ToStartAiming");
                    Message("OnSearch");
                    break;

                case FighterState.call:
                    Message("ToHideFlashlight");
                    Message("ToDisarm");
                    Message("ToTakeRadio");

                    _callCheck = false;
                    Message("ToCall");

                    if (!_callCheck)
                        setState(_previousState);
                    break;
            }

            if (IsAlerted && _isInDarkness)
                Message("ToTurnOnLight");
        }

        private void closeState(FighterState state, FighterState next)
        {
            switch (state)
            {
                case FighterState.search:
                    if (_isInDarkness)
                        Message("ToHideFlashlight");

                    Message("ToStopSearch");
                    break;

                case FighterState.avoidGrenade:
                    Message("ToStopMoving");
                    break;

                case FighterState.maintainPosition:
                    if (_maintainPositionIndefinitely)
                        Message("OnStopHoldingPosition");

                    Message("ToStopCrouching");
                    break;

                case FighterState.flee:
                    Message("ToStopFleeing");
                    break;

                case FighterState.patrol:
                    if (_isInDarkness)
                        Message("ToHideFlashlight");

                    Message("ToStopVisitingWaypoints");
                    break;

                case FighterState.takeDefenseCover:
                case FighterState.takeCover:
                case FighterState.switchCover:
                case FighterState.retreatToCover:
                case FighterState.fightInCover:
                    if (next != FighterState.fightInCover && next != FighterState.hideInCover)
                        Message("ToLeaveCover");
                    break;

                case FighterState.investigate:
                    if (_isInDarkness)
                        Message("ToHideFlashlight");

                    Message("ToStopInvestigation");
                    break;

                case FighterState.call:
                    Message("ToHideRadio");
                    break;

                case FighterState.assault:
                    Message("ToStopAssault");
                    break;
            }

            switch (state)
            {
                case FighterState.fightInCover:
                case FighterState.standAndFight:
                case FighterState.keepCloseTo:
                case FighterState.maintainPosition:
                case FighterState.circle:
                case FighterState.investigate:
                case FighterState.assault:
                case FighterState.process:
                case FighterState.takeCover:
                case FighterState.takeDefenseCover:
                    Message("ToCloseFire");
                    break;
            }
        }

        #endregion

        #region State checks

        private bool canLeavePosition
        {
            get
            {
                return State != FighterState.maintainPosition || !_maintainPositionIndefinitely;
            }
        }

        private bool canChangeTarget
        {
            get
            {
                return Threat == null || !Threat.IsAlive || Threat != _lockedTarget;
            }
        }

        private bool checkInvestigationManageFireAndSetTheState(bool checkVisibility, bool checkTime, bool startInvestigation = true)
        {
            if (Threat == null)
            {
                if (_hasOpenFire)
                    Message("ToCloseFire");

                return false;
            }

            var needsToInvestigate = false;

            var distance = Vector3.Distance(transform.position, LastKnownThreatPosition);
            var hasTimedOut = (checkTime && InvestigationWait < ThreatAge);
            var isInvisibleAndOutsideCover = checkVisibility && _invisibleTime > 1 && ThreatCover == null;

            if (!hasTimedOut && isInvisibleAndOutsideCover && !_hasCheckedIfTheLastKnownPositionIsNearCover)
            {
                _hasCheckedIfTheLastKnownPositionIsNearCover = true;

                Cover cover = null;
                Vector3 position = LastKnownThreatPosition;

                if (Util.GetClosestCover(position, 3, ref cover, ref position) &&
                    AIUtil.IsObstructed(transform.position + Vector3.up * 2, position + Vector3.up * 0.5f))
                {
                    isInvisibleAndOutsideCover = false;
                    setThreat(false, false, false, false, Threat, position, cover, 0);
                }
            }

            var isInDarkness = _sight != null && _sight.IsInDarkness(Threat);

            if (hasTimedOut || (isInvisibleAndOutsideCover && !isInDarkness))
            {
                var closestPosition = LastKnownThreatPosition;
                var isPossibleToReach = AIUtil.GetClosestStandablePosition(ref closestPosition);

                if (_hasCheckedReachabilityAndFailed && Time.timeSinceLevelLoad - _reachabilityCheckTime > 5)
                    isPossibleToReach = false;

                if (isPossibleToReach)
                {
                    AIUtil.Path(ref _reachablePath, transform.position, closestPosition);
                    isPossibleToReach = _reachablePath.status == NavMeshPathStatus.PathComplete;
                }

                if (isPossibleToReach && tryInvestigate())
                {
                    _hasCheckedReachabilityAndFailed = false;
                    Message("ToClearSearchHistory");
                    setState(FighterState.investigate);
                    needsToInvestigate = true;
                }
                else if (_hasOpenFire)
                {
                    _hasCheckedReachabilityAndFailed = true;
                    _reachabilityCheckTime = Time.timeSinceLevelLoad;
                    Message("ToCloseFire");
                }
            }
            else
            {
                _hasCheckedReachabilityAndFailed = false;
                var sightDistance = (_sight != null && _sight.enabled) ? _sight.Distance : 0;

                if (distance >= sightDistance)
                {
                    if (_hasOpenFire)
                        Message("ToCloseFire");
                }
                else if (!_hasOpenFire && Threat.IsAlive)
                    Message("ToOpenFire");
            }

            return needsToInvestigate;
        }

        private void checkAndThrowGrenade()
        {
           // print(transform.name + " CheckAndThrowGrenade#Grenade#");
            if (Threat == null) {
             //   print(transform.name + " Threat null#Grenade#");
                return;
            }
            if (InvestigationWait < ThreatAge) {
               // print(transform.name + " InvestigationWait#Grenade#");
                return;
            }
            if (_thrownGrenadeCount >= Grenades.GrenadeCount) {
              //  print(transform.name + " GrenadeCount#Grenade#");
                return;
            }

            if (!CanSeeTheThreat && ThreatCover == null && !_isInDarkness) {
               // print(transform.name + " #Grenade# " + "CanSeeThreat: "+CanSeeTheThreat + "  " + "ThreatCover: "+(ThreatCover == null) + "  Is In Darkness: " + _isInDarkness);
                return;
            }

            var doThrow = false;

            if (_hasThrowFirstGrenade)
            {
                if (_grenadeTimer < Grenades.Interval)
                    _grenadeTimer += Time.deltaTime;
                else
                    doThrow = true;
            }
            else
            {
                if (_grenadeTimer < Grenades.FirstCheckDelay)
                    _grenadeTimer += Time.deltaTime;
                else
                    doThrow = true;
            }
            
          //  print(transform.name + "#Grenade# Do Throw: "+doThrow +" Potential: "+ (_motor.PotentialGrenade != null));
            if (doThrow && _motor.PotentialGrenade != null)
            {
                if (_grenadeCheckTimer <= float.Epsilon)
                {
                    GrenadeDescription desc;
                    desc.Gravity = _motor.Grenade.Gravity;
                    desc.Duration = _motor.PotentialGrenade.Timer;
                    desc.Bounciness = _motor.PotentialGrenade.Bounciness;

                    var isOk = true;

                    for (int i = 0; i < GrenadeList.Count; i++)
                    {
                        var grenade = GrenadeList.Get(i);

                        if (Vector3.Distance(grenade.transform.position, LastKnownThreatPosition) < grenade.ExplosionRadius * 0.5f)
                        {
                            isOk = false;
                            break;
                        }
                    }

                    int pathLength = 0;

                    if (isOk)
                    {
                        pathLength = GrenadePath.Calculate(GrenadePath.Origin(_motor, Util.HorizontalAngle(LastKnownThreatPosition - transform.position)),
                                                           LastKnownThreatPosition,
                                                           _motor.Grenade.MaxVelocity,
                                                           desc,
                                                           _grenadePath,
                                                           _motor.Grenade.Step);

                        isOk = Vector3.Distance(_grenadePath[pathLength - 1], LastKnownThreatPosition) < Grenades.MaxRadius;
                    }

                    if (isOk)
                    {
                        var count = AIUtil.FindActors(_grenadePath[pathLength - 1], Grenades.AvoidDistance);

                        for (int i = 0; i < count; i++)
                            if (AIUtil.Actors[i] == Actor || AIUtil.Actors[i].Side == Actor.Side)
                            {
                                isOk = false;
                                break;
                            }
                    }
                    
                    if (isOk)
                    {
                        _motor.InputThrowGrenade(_grenadePath, pathLength, _motor.Grenade.Step);
                        _thrownGrenadeCount++;

                        _grenadeTimer = 0;
                        _hasThrowFirstGrenade = true;
                    }
                    else
                        _grenadeCheckTimer = Grenades.CheckInterval;
                }
                else
                    _grenadeCheckTimer -= Time.deltaTime;
            }
            else
                _grenadeCheckTimer = 0;
        }

        private void findGrenades()
        {
            for (int i = 0; i < GrenadeList.Count; i++)
            {
                var grenade = GrenadeList.Get(i);

                if (Vector3.Distance(grenade.transform.position, transform.position) < grenade.ExplosionRadius)
                {
                    _grenadeAvoidReaction += Time.deltaTime;

                    if (_grenadeAvoidReaction >= GrenadeAvoidance.ReactionTime + float.Epsilon)
                    {
                        _avoidedGrenadePosition = grenade.transform.position;
                        _avoidedGrenadeRange = grenade.ExplosionRadius;
                        setState(FighterState.avoidGrenade);
                    }

                    return;
                }
            }

            _grenadeAvoidReaction = 0;
        }

        private bool checkAllySpacingAndMoveIfNeeded()
        {
            var has = false;
            var closest = 0f;
            var vector = Vector3.zero;

            var count = AIUtil.FindActors(transform.position, AllySpacing, Actor);

            for (int i = 0; i < count; i++)
                if (AIUtil.Actors[i].Side == Actor.Side)
                {
                    var v = AIUtil.Actors[i].transform.position - transform.position;
                    var d = v.magnitude;

                    if (!has || d < closest)
                    {
                        has = true;
                        closest = d;
                        vector = v;
                    }
                }

            if (has)
            {
                _hadMoveTargetBeforeAllySpacing = _hasMoveTarget;
                _moveTargetBeforeAllySpacing = _moveTarget;
                Message("ToWalkInDirection", -vector);
                return true;
            }
            else
            {
                if (_hadMoveTargetBeforeAllySpacing)
                    Message("ToRunTo", _moveTargetBeforeAllySpacing);

                return false;
            }
        }

        private bool checkAvoidanceAndSetTheState()
        {
            if (Threat == null || !Threat.IsAlive || !CanSeeTheThreat || Vector3.Distance(LastKnownThreatPosition, transform.position) > AvoidDistance)
                return false;

            setState(FighterState.avoidAndFight);
            return true;
        }

        private void takeCoverOrAssault()
        {
            if (tryAssault())
                setState(FighterState.assault);
            else
                setState(FighterState.takeCover);
        }

        private void fightOrRunAway()
        {
            if (tryFire())
            {
                if (Actor.Cover != null)
                    setState(FighterState.fightInCover);
                else
                    setState(FighterState.standAndFight);
            }
            else if (tryAssault())
                setState(FighterState.assault);
            else
                setState(FighterState.runAway);
        }

        private void alarm()
        {
            if (!_wasAlarmed)
            {
                _wasAlarmed = true;
                Message("OnAlarmed");
            }
        }

        private bool tryFire()
        {
            if (_motor.Weapon.Gun != null)
                return true;

            if (_inventory != null)
                for (int i = 0; i < _inventory.Weapons.Length; i++)
                    if (_inventory.Weapons[i].Gun != null)
                        return true;

            return false;
        }

        #endregion

        #region Threat

        private bool canSetThreat
        {
            get
            {
                return _isInAggressiveMode || ImmediateThreatReaction;
            }
        }

        private void turnAndAimAtTheThreat()
        {
            if (Threat == null || !Threat.IsAlive)
            {
                Message("ToTurnAt", LastKnownThreatPosition);
                Message("ToAimAt", LastKnownThreatPosition + Vector3.up * 1.0f);
            }
            else
                Message("ToTarget", new ActorTarget(LastKnownThreatPosition, Threat.RelativeTopPosition, Threat.RelativeStandingTopPosition));
        }

        private void guessThreat(Actor threat, Vector3 position, bool isHeard)
        {
            var error = Approximation.Get(Vector3.Distance(transform.position, position));

            if (error < 0.25f)
                setUnseenThreat(isHeard, true, false, threat, position, null);
            else
            {
                var attempts = 0;

                while (attempts < 6)
                {
                    var normal = Util.HorizontalVector(UnityEngine.Random.Range(0f, 360f));
                    var distance = UnityEngine.Random.Range(error * 0.25f, error);
                    var newPosition = position + normal * distance;

                    if (AIUtil.GetClosestStandablePosition(ref newPosition) && Mathf.Abs(newPosition.y - position.y) < 0.2f)
                    {
                        setUnseenThreat(isHeard, false, false, threat, newPosition, null);
                        return;
                    }

                    attempts++;
                }

                setUnseenThreat(isHeard, false, false, threat, position, null);
                return;
            }
        }

        private void setUnseenThreat(bool isHeard, bool isDirect, bool isSeenByFriend, Actor threat, Vector3 position, Cover threatCover)
        {
            setThreat(false, isHeard, isSeenByFriend, isDirect, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        private void setSeenThreat(Actor threat, Vector3 position, Cover threatCover)
        {
            setThreat(true, false, false, true, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        private void setThreat(bool isVisible, bool isHeard, bool isVisibleByFriends, bool isActual, Actor threat, Vector3 position, Cover threatCover, float time)
        {
            var previousThreat = Threat;
            var wasVisible = CanSeeTheThreat;

            if (threat != _lockedTarget)
                _lockedTarget = null;

            if (threat != null)
                _isInAggressiveMode = true;

            _hasCheckedIfTheLastKnownPositionIsNearCover = false;

            SetThreat(isVisible, isHeard, isActual, threat, position, threatCover, time);

            if (CanSeeTheThreat && Threat != null)
                if (!wasVisible || previousThreat != Threat)
                {
                    foreach (var friend in _friendsThatCanSeeMe)
                        friend.SendMessage("OnFriendFoundEnemy", Actor);

                    foreach (var friend in _friends)
                        if (!_friendsThatCanSeeMe.Contains(friend.Actor))
                            friend.Message("OnFriendFoundEnemy", Actor);
                }

            if (!isActiveAndEnabled)
                return;

            if (canLeavePosition)
            {
                if (!IsAlerted && UnityEngine.Random.Range(0f, 1f) <= TakeCoverImmediatelyChance)
                    setState(FighterState.takeCover);
                else if (State == FighterState.investigate)
                {
                    if (isActual)
                    {
                        if (isVisible)
                            fightOrRunAway();
                        else if (isVisibleByFriends)
                            takeCoverOrAssault();
                        else if (tryInvestigate())
                            setState(FighterState.investigate, true);
                        else
                            takeCoverOrAssault();
                    }
                    else
                        Message("ToInvestigatePosition", position);
                }
                else if (State == FighterState.search || !IsAlerted)
                {
                    if (isVisible)
                        fightOrRunAway();
                    else if (isVisibleByFriends)
                        takeCoverOrAssault();
                    else if (tryInvestigate())
                        setState(FighterState.investigate);
                    else
                        takeCoverOrAssault();
                }
            }

            if (State == FighterState.maintainPosition && Threat != null && Threat != previousThreat)
                Message("ToOpenFire");
        }

        #endregion
    }
}
