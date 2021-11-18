using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    public struct ActorMemory
    {
        public Actor Actor;
        public Vector3 Position;
        public float Time;
    }

    public enum ThreatPriority
    {
        Closest,
        LeastHealth,
        LeadersTarget
    }

    /// <summary>
    /// Manually manages the active enemy of the FighterBrain component.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AIThreatControl : AIBase
    {
        /// <summary>
        /// For how long the AI remembers a threat as a possible target after losing sight of it.
        /// </summary>
        [Tooltip("For how long the AI remembers a threat as a possible target after losing sight of it.")]
        public float MemoryDuration = 3;

        /// <summary>
        /// Mode for choosing which threat to attack. Leader is taken from AIFormation.
        /// </summary>
        [Tooltip("Mode for choosing which threat to attack. Leader is taken from AIFormation.")]
        public ThreatPriority Priority = ThreatPriority.Closest;

        public float HealthDistance = 15;

        private Actor _actor;
        private Actor _previous;
        private Vector3 _previousThreatPosition;
        private Actor _reference;
        private float _previousThreatHealth;
        private List<Actor> _visible = new List<Actor>();
        private List<ActorMemory> _memory = new List<ActorMemory>();
        private AIFormation _formation;
        private FighterBrain _brain;

        private Actor _threatOverride;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _formation = GetComponent<AIFormation>();
            _brain = GetComponent<FighterBrain>();
        }

        /// <summary>
        /// Registers an actor whose target is looked at and attacked.
        /// </summary>
        public void ToSetThreatReference(Actor reference)
        {
            _reference = reference;
        }

        /// <summary>
        /// Notified that a threat has to be attacked and all other threats ignored.
        /// </summary>
        public void ToOverrideThreat(Actor threat)
        {
            _threatOverride = threat;
        }

        private void Update()
        {
            for (int i = _memory.Count - 1; i >= 0; i--)
                if (Time.timeSinceLevelLoad - _memory[i].Time >= MemoryDuration)
                    _memory.RemoveAt(i);

            if (_threatOverride != null && !_threatOverride.IsAlive)
                _threatOverride = null;

            if (_previous != null && !_previous.IsAlive)
                _previous = null;

            if (_threatOverride != null)
            {
                _previous = _threatOverride;

                if (_visible.Contains(_threatOverride))
                    _previousThreatPosition = _threatOverride.transform.position;
                else
                    foreach (var memory in _memory)
                        if (memory.Actor == _threatOverride)
                            _previousThreatPosition = memory.Position;
            }
            else
            {
                var reference = (_reference != null && _reference.IsAlive) ? _reference : _actor;

                var isOk = true;

                if (_brain != null)
                    if (_brain.Threat == null && !_brain.ImmediateThreatReaction)
                        isOk = false;

                switch (Priority)
                {
                    case ThreatPriority.Closest:
                        {
                            Actor closest = null;
                            float minDist = 0;
                            Vector3 closestPosition = Vector3.zero;

                            if (isOk)
                            {
                                for (int i = 0; i < _visible.Count; i++)
                                    if (_visible[i].IsAlive)
                                    {
                                        var dist = Vector3.Distance(_visible[i].transform.position, reference.transform.position);

                                        if (dist < minDist || closest == null)
                                        {
                                            closestPosition = _visible[i].transform.position;
                                            closest = _visible[i];
                                            minDist = dist;
                                        }
                                    }

                                for (int i = 0; i < _memory.Count; i++)
                                    if (_memory[i].Actor.IsAlive)
                                    {
                                        var dist = Vector3.Distance(_memory[i].Position, reference.transform.position);

                                        if (dist < minDist || closest == null)
                                        {
                                            closestPosition = _memory[i].Position;
                                            closest = _memory[i].Actor;
                                            minDist = dist;
                                        }
                                    }
                            }

                            if (_previous != null && closest != null &&
                                _previous != closest &&
                                Vector3.Distance(_previousThreatPosition, reference.transform.position) < minDist + 0.2f)
                            {
                                closest = _previous;

                                if (_visible.Contains(closest))
                                    closestPosition = closest.transform.position;
                                else
                                    closestPosition = _previousThreatPosition;

                            }

                            _previous = closest;
                            _previousThreatPosition = closestPosition;

                            if (_previous != null)
                                _previousThreatHealth = _previous.Health;
                        }
                        break;

                    case ThreatPriority.LeastHealth:
                        {
                            Actor closest = null;
                            float minHealth = 0;
                            Vector3 closestPosition = Vector3.zero;

                            if (isOk)
                            {
                                for (int i = 0; i < _visible.Count; i++)
                                    if (_visible[i].IsAlive && Vector3.Distance(_visible[i].transform.position, reference.transform.position) <= HealthDistance)
                                        if (_visible[i].Health < minHealth || closest == null)
                                        {
                                            closestPosition = _visible[i].transform.position;
                                            closest = _visible[i];
                                            minHealth = closest.Health;
                                        }

                                for (int i = 0; i < _memory.Count; i++)
                                    if (_memory[i].Actor.IsAlive && Vector3.Distance(_memory[i].Position, reference.transform.position) <= HealthDistance)
                                        if (_memory[i].Actor.Health < minHealth || closest == null)
                                        {
                                            closestPosition = _memory[i].Position;
                                            closest = _memory[i].Actor;
                                            minHealth = closest.Health;
                                        }

                                if (_previous != null && closest != null &&
                                    _previous != closest &&
                                    _previousThreatHealth < minHealth + 1)
                                {
                                    closest = _previous;

                                    if (_visible.Contains(closest))
                                        closestPosition = closest.transform.position;
                                    else
                                        closestPosition = _previousThreatPosition;

                                }
                            }

                            _previous = closest;
                            _previousThreatPosition = closestPosition;

                            if (_previous != null)
                                _previousThreatHealth = _previous.Health;
                        }
                        break;

                    case ThreatPriority.LeadersTarget:
                        {
                            var formationLeader = (_formation != null && _formation.Leader != null && _formation.Leader.Actor != null) ? _formation.Leader.Actor : null;
                            var leader = (_reference != null && _reference.IsAlive) ? _reference : formationLeader;

                            if (leader != null && leader.Threat != null && leader.Threat.IsAlive)
                            {
                                _previous = leader.Threat;

                                if (_previous != null)
                                {
                                    var brain = leader.GetComponent<BaseBrain>();

                                    if (brain != null)
                                        _previousThreatPosition = brain.LastKnownThreatPosition;

                                    _previousThreatHealth = _previous.Health;
                                }
                            }
                            else
                                _previous = null;
                            break;
                        }
                }
            }

            if (_previous != null)
            {
                Threat threat;
                threat.Actor = _previous;
                threat.Position = _previousThreatPosition;

                Message("ToSetThreat", threat);
            }
        }

        private void removeFromMemory(Actor actor)
        {
            for (int i = 0; i < _memory.Count; i++)
                if (_memory[i].Actor == actor)
                {
                    _memory.RemoveAt(i);
                    break;
                }
        }

        private void addToMemory(Actor actor, Vector3 position)
        {
            if (_visible.Contains(actor))
                _visible.Remove(actor);

            ActorMemory memory;
            memory.Actor = actor;
            memory.Position = position;
            memory.Time = Time.timeSinceLevelLoad;

            for (int i = 0; i < _memory.Count; i++)
                if (_memory[i].Actor == actor)
                {
                    _memory[i] = memory;
                    return;
                }

            _memory.Add(memory);
        }

        /// <summary>
        /// A death was witnessed.
        /// </summary>
        public void OnSeeDeath(Actor actor)
        {
            if (_visible.Contains(actor))
                _visible.Remove(actor);

            removeFromMemory(actor);
        }

        /// <summary>
        /// Notified by the sight AI that an actor has entered the view.
        /// </summary>
        public void OnSeeActor(Actor actor)
        {
            if (actor.Side != _actor.Side)
            {
                removeFromMemory(actor);

                if (!_visible.Contains(actor))
                    _visible.Add(actor);
            }
        }

        /// <summary>
        /// Notified by the sight AI that an actor has dissappeared from the view.
        /// </summary>
        public void OnUnseeActor(Actor actor)
        {
            if (actor.Side != _actor.Side)
                addToMemory(actor, actor.transform.position);
        }

        /// <summary>
        /// Notified by a friend that they found a new enemy position.
        /// </summary>
        public void OnFriendFoundEnemy(Actor friend)
        {
            var brain = friend.GetComponent<BaseBrain>();
            if (brain == null)
                return;

            if (brain.Threat != null)
                if (!_visible.Contains(brain.Threat))
                    addToMemory(brain.Threat, brain.LastKnownThreatPosition);
        }
    }
}
