using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Regroups other brains around the AI.
    /// </summary>
    public abstract class AIBaseRegrouper : AIBase
    {
        /// <summary>
        /// All friends that are regrouping.
        /// </summary>
        public List<Actor> Friends
        {
            get { return _friends; }
        }

        /// <summary>
        /// Maximum distance at which the regrouped actors will stand next to the regrouper.
        /// </summary>
        [Tooltip("Maximum distance at which the regrouped actors will stand next to the regrouper.")]
        public float Radius = 8;

        /// <summary>
        /// Time in seconds the units will maintain their new uncovered positions before searching for better spots.
        /// </summary>
        [Tooltip("Time in seconds the units will maintain their new uncovered positions before searching for better spots.")]
        public float UncoveredDuration = 8;

        /// <summary>
        /// Distance at which to search for friendly AI that will be regrouped.
        /// </summary>
        [Tooltip("Distance at which to search for friendly AI that will be regrouped.")]
        public float CallDistance = 20;

        /// <summary>
        /// Maximum number of regrouped units.
        /// </summary>
        [Tooltip("Maximum number of regrouped units.")]
        public int Limit = 6;

        private Actor _actor;
        private BaseBrain _brain;

        private List<Actor> _friends = new List<Actor>();
        private List<Vector3> _takenPositions = new List<Vector3>();

        protected virtual void Awake()
        {
            _actor = GetComponent<Actor>();
            _brain = GetComponent<BaseBrain>();
        }

        protected BaseBrain Brain
        {
            get { return _brain; }
        }

        /// <summary>
        /// Registers a taken position.
        /// </summary>
        public void TakePosition(Vector3 value)
        {
            _takenPositions.Add(value);
        }

        /// <summary>
        /// Returns true if the given position has been taken by one of the regrouping AIs.
        /// </summary>
        public bool IsPositionTaken(Vector3 position, float threshold = 1.0f)
        {
            for (int i = 0; i < _takenPositions.Count;i++)
                if (Vector3.Distance(_takenPositions[i], position) < threshold)
                    return true;

            return false;
        }

        /// <summary>
        /// Finds nearby allies and tells them to regroup.
        /// </summary>
        public void Regroup()
        {
            _friends.Clear();
            _takenPositions.Clear();

            var count = Physics.OverlapSphereNonAlloc(_actor.transform.position, CallDistance, Util.Colliders, Layers.Character, QueryTriggerInteraction.Ignore);
            var limit = Limit;

            for (int i = 0; i < count; i++)
            {
                var friend = Actors.Get(Util.Colliders[i].gameObject);

                if (friend != null && friend != _actor && friend.IsAlive && friend.Side == _actor.Side)
                {
                    var distance = Vector3.Distance(_actor.transform.position, friend.transform.position);

                    if (distance < CallDistance)
                    {
                        if (limit <= 0)
                            break;
                        else
                            limit--;

                        friend.SendMessage("ToLeaveCover", SendMessageOptions.DontRequireReceiver);
                        _friends.Add(friend);
                    }
                }
            }

            for (int i = 0; i < _friends.Count; i++)
                _friends[i].SendMessage("ToRegroupAround", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}
