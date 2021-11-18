using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Stores possible actions for the AI to take. Can automatically execute actions.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(FighterBrain))]
    public class AIActions : AIBase
    {
        /// <summary>
        /// Is there an action that is currently being performed.
        /// </summary>
        public bool IsPerforming
        {
            get { return _active != null; }
        }

        /// <summary>
        /// Action currently being performed. Null if none.
        /// </summary>
        public AIAction Active
        {
            get { return _active; }
        }

        /// <summary>
        /// Array of possible actions.
        /// </summary>
        [Tooltip("Array of possible actions.")]
        public AIAction[] Actions;

        /// <summary>
        /// An actor that is prioritized over others when performing relevant actions.
        /// </summary>
        [Tooltip("An actor that is prioritized over others when performing relevant actions.")]
        public Actor Priority;

        /// <summary>
        /// Time in seconds to wait after making another automatic action.
        /// </summary>
        [Tooltip("Time in seconds to wait after making another automatic action.")]
        public float AutoCooldown = 8;

        /// <summary>
        /// Distance at which the AI will consider other actors as target of an auto action.
        /// </summary>
        [Tooltip("Distance at which the AI will consider other actors as target of an auto action.")]
        public float AutoDistance = 20;

        /// <summary>
        /// Should the automatic actions against enemies be performed when in cover.
        /// </summary>
        [Tooltip("Should the automatic actions against enemies be performed when in cover.")]
        public bool AutoAttackOnlyFromCover = true;

        /// <summary>
        /// If other AI components are taking cover, should the actions be delayed until they are finished.
        /// </summary>
        [Tooltip("If other AI components are taking cover, should the actions be delayed until they are finished.")]
        public bool WaitForCoverActions = true;

        /// <summary>
        /// Once the AI has taken cover, how many seconds to wait before considering an action.
        /// </summary>
        [Tooltip("Once the AI has taken cover, how many seconds to wait before considering an action.")]
        public float CoverDelay = 2;

        /// <summary>
        /// If an action is being performed and it takes too long it will be cancelled. Some actions have no timeout.
        /// </summary>
        [Tooltip("If an action is being performed and it takes too long it will be cancelled. Some actions have no timeout.")]
        public float Timeout = 4;

        private Actor _actor;
        private FighterBrain _brain;

        private AIAction _active;

        private float _autoWait = 0;
        private float _checkWait = 0;

        private List<Actor> _possibleActionTargets = new List<Actor>();
        private List<Actor> _nearbyActors = new List<Actor>();

        private bool _isTakingCover;
        private float _coverTimer;
        private bool _isInCover;

        private float _actionStart;

        private void Awake()
        {
            _brain = GetComponent<FighterBrain>();
            _actor = GetComponent<Actor>();

            for (int i = 0; i < Actions.Length; i++)
                Actions[i].SetupActor(_actor);
        }

        /// <summary>
        /// Are there any actions that can target allies.
        /// </summary>
        public bool HasAllyActions
        {
            get
            {
                for (int i = 0; i < Actions.Length; i++)
                    if (Actions[i].CanTargetAlly)
                        return true;

                return false;
            }
        }

        /// <summary>
        /// Executes the given action. Does not necessarily have to belong to the Actions list.
        /// </summary>
        public void Execute(AIAction action)
        {
            if (_active != null || action.Wait > float.Epsilon)
                return;

            if (action.Execute(_actor))
                begin(action);
        }

        /// <summary>
        /// Executes the given action targeted at a position. Does not necessarily have to belong to the Actions list.
        /// </summary>
        public void Execute(AIAction action, Vector3 position)
        {
            if (_active != null || action.Wait > float.Epsilon)
                return;

            if (action.Execute(_actor, position))
                begin(action);
        }

        /// <summary>
        /// Executes the given action targeted at an actor. Does not necessarily have to belong to the Actions list.
        /// </summary>
        public void Execute(AIAction action, Actor target)
        {
            if (_active != null || action.Wait > float.Epsilon)
                return;

            if (action.Execute(_actor, target))
                begin(action);
        }

        private void begin(AIAction command)
        {
            _actionStart = Time.timeSinceLevelLoad;
            _active = command;
            Message("ToEnterProcess", false);
        }

        private void end()
        {
            Message("ToExitProcessAndMaintainPosition");
            _active.Stop();
            _active = null;
        }

        /// <summary>
        /// Catches a message that asks to end all actions.
        /// </summary>
        public void ToStopActions()
        {
            if (_active != null)
                end();
        }

        /// <summary>
        /// Catches an animator message and passes it to the current action.
        /// </summary>
        public void OnFinishAction()
        {
            if (_active != null && _active.OnFinishAction())
                end();
        }

        /// <summary>
        /// Catches a throw of a grenade and passes it to the current action.
        /// </summary>
        public void OnThrow()
        {
            if (_active != null && _active.OnThrow())
                end();
        }

        /// <summary>
        /// Catches an AI command to stop moving.
        /// </summary>
        public void ToStopMoving()
        {
            _isTakingCover = false;
        }

        /// <summary>
        /// Registers that the AI has stopped running to cover.
        /// </summary>
        public void OnFinishTakeCover()
        {
            _isTakingCover = false;
        }

        /// <summary>
        /// Registers that the AI has began moving to cover.
        /// </summary>
        public void OnFoundCover()
        {
            _isTakingCover = true;
        }

        private void Update()
        {
            if (_autoWait > float.Epsilon) _autoWait -= Time.deltaTime;
            if (_checkWait > float.Epsilon) _checkWait -= Time.deltaTime;
            if (_coverTimer > float.Epsilon) _coverTimer -= Time.deltaTime;

            if (_active != null && (!_active.Update() || (!_active.HasNoTimeout && (Time.timeSinceLevelLoad - _actionStart) > Timeout)))
                end();

            if (_actor.Cover == null)
            {
                _isInCover = false;
                _coverTimer = 0;
            }
            else if (!_isInCover)
            {
                _coverTimer = CoverDelay;
                _isInCover = true;
            }

            if (_active == null &&
                _autoWait <= float.Epsilon &&
                _checkWait <= float.Epsilon &&
                _brain.enabled &&
                _brain.State != FighterState.process &&
                _actor.IsAlive &&
                _coverTimer <= float.Epsilon &&
                (!WaitForCoverActions || !_isTakingCover))
            {
                _checkWait = 0.5f;
                _nearbyActors.Clear();

                {
                    var count = AIUtil.FindActorsIncludingDead(transform.position, AutoDistance);
                    for (int i = 0; i < count; i++)
                        _nearbyActors.Add(AIUtil.Actors[i]);
                }

                var canUseAttackAction = !AutoAttackOnlyFromCover || _actor.Cover != null;

                AIAction bestSingleAction = null;
                Actor bestSingleActionTarget = null;

                ///////////////////////////////////////////////////////
                //
                // CONSIDER MULTIPLE TARGET ACTIONS
                //
                ///////////////////////////////////////////////////////

                AIAction bestMultipleAction = null;
                int bestMultipleActionTargetCount = 0;

                for (int i = 0; i < Actions.Length; i++)
                {
                    var action = Actions[i];
                    if (action.Wait > float.Epsilon || !action.Auto || action.NeedsSingleTargetActor || action.NeedsTargetLocation || action.NeedsOnlySelf)
                        continue;

                    int count = 0;

                    for (int j = 0; j < Actors.Count; j++)
                    {
                        var actor = Actors.Get(j);

                        if (!actor.IsAlive && action.ShouldIgnoreDead)
                            continue;

                        if (actor.Side == _actor.Side)
                        {
                            if (((actor == _actor && action.CanTargetSelf) || action.CanTargetAlly) && action.IsNeededFor(actor))
                                count++;
                        }
                        else if (canUseAttackAction && action.CanTargetEnemy && action.IsNeededFor(actor) && (!AutoAttackOnlyFromCover || !action.WillMoveForActor(actor)))
                            count++;
                    }

                    if (count > bestMultipleActionTargetCount)
                    {
                        bestMultipleAction = action;
                        bestMultipleActionTargetCount = count;
                    }
                }

                if (bestMultipleAction != null)
                {
                    _autoWait = AutoCooldown;
                    Execute(bestMultipleAction);
                    return;
                }

                ///////////////////////////////////////////////////////
                //
                // CONSIDER SELF ACTIONS
                //
                ///////////////////////////////////////////////////////

                for (int i = 0; i < Actions.Length; i++)
                {
                    var action = Actions[i];
                    if (action.Wait > float.Epsilon || !action.Auto || !action.NeedsOnlySelf)
                        continue;

                    if (action.CanTargetSelf && action.IsNeededFor(_actor))
                    {
                        if (_actor == Priority)
                        {
                            _autoWait = AutoCooldown;
                            Execute(action, _actor);
                            return;
                        }

                        bestSingleAction = action;
                        bestSingleActionTarget = _actor;
                        break;
                    }
                }

                ///////////////////////////////////////////////////////
                //
                // CONSIDER SINGLE TARGET ACTIONS
                //
                ///////////////////////////////////////////////////////

                for (int i = 0; i < Actions.Length; i++)
                {
                    var action = Actions[i];
                    if (action.Wait > float.Epsilon || !action.Auto || !action.NeedsSingleTargetActor)
                        continue;

                    Actor best = null;
                    var bestDistance = 0f;
                    var isBestSelf = false;

                    for (int j = 0; j < _nearbyActors.Count; j++)
                    {
                        var actor = _nearbyActors[j];

                        if (!actor.IsAlive && action.ShouldIgnoreDead)
                            continue;

                        if (actor == _actor)
                        {
                            if (action.CanTargetSelf && action.IsNeededFor(actor))
                            {
                                if (actor == Priority)
                                {
                                    _autoWait = AutoCooldown;
                                    Execute(action, actor);
                                    return;
                                }

                                if (best == null)
                                {
                                    best = actor;
                                    bestDistance = 0;
                                    isBestSelf = true;
                                }
                            }
                        }
                        else if (actor.Side == _actor.Side)
                        {
                            if (action.CanTargetAlly && action.IsNeededFor(actor))
                            {
                                if (actor == Priority)
                                {
                                    _autoWait = AutoCooldown;
                                    Execute(action, actor);
                                    return;
                                }

                                var distance = Vector3.Distance(transform.position, actor.transform.position);

                                if (best == null || isBestSelf || distance < bestDistance)
                                {
                                    best = actor;
                                    bestDistance = distance;
                                    isBestSelf = false;
                                }
                            }
                        }
                        else if (canUseAttackAction && action.CanTargetEnemy && action.IsNeededFor(actor) && (!AutoAttackOnlyFromCover || !action.WillMoveForActor(actor)))
                        {
                            if (actor == Priority)
                            {
                                _autoWait = AutoCooldown;
                                Execute(action, actor);
                                return;
                            }

                            var distance = Vector3.Distance(transform.position, actor.transform.position);

                            if (best == null || isBestSelf || distance < bestDistance)
                            {
                                best = actor;
                                bestDistance = distance;
                                isBestSelf = false;
                            }
                        }
                    }

                    if (best != null)
                    {
                        bestSingleAction = action;
                        bestSingleActionTarget = best;
                        break;
                    }
                }

                ///////////////////////////////////////////////////////
                //
                // CONSIDER AREA ACTIONS
                //
                ///////////////////////////////////////////////////////

                for (int i = 0; i < Actions.Length; i++)
                {
                    var action = Actions[i];
                    if (action.Wait > float.Epsilon || !action.Auto || !action.NeedsTargetLocation)
                        continue;

                    _possibleActionTargets.Clear();

                    for (int j = 0; j < _nearbyActors.Count; j++)
                    {
                        var actor = _nearbyActors[j];

                        if (!actor.IsAlive && action.ShouldIgnoreDead)
                            continue;

                        if (actor == _actor)
                        {
                            if (action.CanTargetSelf && action.IsNeededFor(actor))
                                _possibleActionTargets.Add(actor);
                        }
                        else if (actor.Side == _actor.Side)
                        {
                            if (action.CanTargetAlly && action.IsNeededFor(actor))
                                _possibleActionTargets.Add(actor);
                        }
                        else if (canUseAttackAction && action.CanTargetEnemy && action.IsNeededFor(actor))
                            _possibleActionTargets.Add(actor);
                    }

                    if (_possibleActionTargets.Count == 0 || (_possibleActionTargets.Count == 1 && bestSingleAction != null))
                        continue;

                    if (action.UIRadius > float.Epsilon)
                    {
                        var radius = action.UIRadius;
                        var steps = (int)(2 * AutoDistance / radius);

                        var hasBest = false;
                        var bestPosition = Vector3.zero;
                        var bestDistance = 0f;
                        var bestActorCount = 0;
                        var bestHasPriority = false;

                        for (int x = -steps / 2; x < steps / 2; x++)
                            for (int y = -steps / 2; y < steps / 2; y++)
                            {
                                var position = transform.position + new Vector3(x * action.UIRadius, 0, y * radius);
                                var distance = Vector3.Distance(position, transform.position);

                                if (distance > AutoDistance)
                                    continue;

                                if (AutoAttackOnlyFromCover && action.CanTargetEnemy && action.WillMoveForPosition(position))
                                    continue;

                                var actorCount = 0;
                                var hasPriority = false;

                                for (int j = 0; j < _possibleActionTargets.Count; j++)
                                {
                                    var a = _possibleActionTargets[j];

                                    if (Vector3.Distance(position, a.transform.position) < radius)
                                    {
                                        actorCount++;

                                        if (a == Priority)
                                            hasPriority = true;
                                    }
                                }

                                if (actorCount > 0)
                                {
                                    var isBetter = false;

                                    if (!hasBest)
                                        isBetter = true;
                                    else if (hasPriority && !bestHasPriority)
                                        isBetter = false;
                                    else if (!bestHasPriority)
                                    {
                                        if (actorCount > bestActorCount)
                                            isBetter = true;
                                        else if (actorCount == bestActorCount)
                                            isBetter = distance < bestDistance;
                                    }

                                    if (isBetter)
                                    {
                                        hasBest = true;
                                        bestPosition = position;
                                        bestDistance = distance;
                                        bestHasPriority = hasPriority;
                                        bestActorCount = actorCount;
                                    }
                                }
                            }

                        if (hasBest &&
                            (bestSingleAction == null || bestActorCount > 1))
                        {
                            _autoWait = AutoCooldown;
                            Execute(action, bestPosition);
                            return;
                        }
                    }
                }

                ///////////////////////////////////////////////////////
                //
                // EXECUTE BEST SINGLE IF NO AREA POSSIBLE
                //
                ///////////////////////////////////////////////////////

                if (bestSingleAction != null)
                {
                    _autoWait = AutoCooldown;
                    Execute(bestSingleAction, bestSingleActionTarget);
                }

                ///////////////////////////////////////////////////////
                //
                // CONSIDER NON-TARGET ACTIONS
                //
                ///////////////////////////////////////////////////////

                for (int i = 0; i < Actions.Length; i++)
                {
                    var action = Actions[i];
                    if (action.Wait > float.Epsilon || !action.Auto || action.CanTargetAny || action.CanTargetGround || action.UIRadius > float.Epsilon || action.CanTargetMultiple || action.NeedsOnlySelf)
                        continue;

                    _autoWait = AutoCooldown;
                    Execute(action);
                    return;
                }
            }
        }
    }
}
