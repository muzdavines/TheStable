using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Continuously checks the situation. If the AI is in danger makes a call for backup.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterMotor))]
    public class AIBackupCall : AIBase
    {
        #region Public fields

        /// <summary>
        /// Target object and a message sent during a call.
        /// </summary>
        [Tooltip("Target object and a message sent during a call.")]
        public AICall Call = AICall.Default();

        /// <summary>
        /// Should the AI only call when in cover.
        /// </summary>
        [Tooltip("Should the AI only call when in cover.")]
        public bool OnlyCallInCover = true;

        /// <summary>
        /// A call is triggered if the number of nearby friends is equal or lower than this value.
        /// </summary>
        [Tooltip("A call is triggered if the number of nearby friends is equal or lower than this value.")]
        public int FriendCount = 0;

        /// <summary>
        /// Time in seconds after alerts before the number of friends is first being checked.
        /// </summary>
        [Tooltip("Time in seconds after alerts before the number of friends is first being checked.")]
        public float FirstCheckDelay = 3;

        /// <summary>
        /// Time in seconds to keep checking the number of friends before making the call.
        /// </summary>
        [Tooltip("Time in seconds to keep checking the number of friends before making the call.")]
        public float CheckDuration = 2;

        /// <summary>
        /// Time in seconds to wait after a call before returning to checking again.
        /// </summary>
        [Tooltip("Time in seconds to wait after a call before returning to checking again.")]
        public float CheckSpacing = 10;

        /// <summary>
        /// Do not call if the whole level contains enough friends already. Distance is ignored.
        /// </summary>
        [Tooltip("Do not call if the whole level contains enough friends already. Distance is ignored.")]
        public int MaxLevelCount = 6;

        #endregion

        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private bool _canCall;
        private float _firstTriggerWait;
        private float _triggerWait;
        private float _triggerSpacing;

        private HashSet<Actor> _visibleFriends = new HashSet<Actor>();
        private HashSet<Actor> _nearbyFriends = new HashSet<Actor>();

        private bool _isWaitingForCall;

        #endregion

        #region Events

        /// <summary>
        /// Registers the fact that the AI has seen an enemy or is fleeing. Initiates call checks.
        /// </summary>
        public void OnAlarmed()
        {
            if (!_canCall)
            {
                _triggerWait = 0;
                _triggerSpacing = 0;
            }

            _canCall = true;
        }

        /// <summary>
        /// Registers an event that the call animation has finished and executes the script.
        /// </summary>
        public void OnCallMade()
        {
            if (_isWaitingForCall && isActiveAndEnabled)
            {
                _isWaitingForCall = false;
                Call.Make(_actor);
                Message("OnBackupCall");
            }
        }

        /// <summary>
        /// Notified of a nearby friend. Increases the counter.
        /// </summary>
        public void OnFoundFriend(Actor friend)
        {
            _nearbyFriends.Add(friend);
        }

        /// <summary>
        /// Notified of a friend going away. Decreases the friend counter.
        /// </summary>
        public void OnLostFriend(Actor friend)
        {
            _nearbyFriends.Remove(friend);
        }

        /// <summary>
        /// Registers a visibility of a friend.
        /// </summary>
        public void OnSeeActor(Actor actor)
        {
            if (actor.Side == _actor.Side && actor.IsAggressive)
                _visibleFriends.Add(actor);
        }

        /// <summary>
        /// Registers a friend dissapearing out of sight.
        /// </summary>
        /// <param name="actor"></param>
        public void OnUnseeActor(Actor actor)
        {
            if (actor.Side == _actor.Side && actor.IsAggressive)
                _visibleFriends.Remove(actor);
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            if (!_actor.IsAlive || !_canCall)
                return;

            if (_firstTriggerWait < FirstCheckDelay)
            { 
                _firstTriggerWait += Time.deltaTime;
                return;
            }

            if (_triggerSpacing >= float.Epsilon)
            {
                _triggerSpacing -= Time.deltaTime;
                return;
            }

            var count = _nearbyFriends.Count;

            foreach (var friend in _visibleFriends)
                if (!_nearbyFriends.Contains(friend))
                    count++;

            if (count <= FriendCount)
            {
                var levelCount = 0;

                for (int i = 0; i < Actors.Count; i++)
                {
                    var actor = Actors.Get(i);

                    if (actor.IsAlive && actor.Side == _actor.Side && actor.IsAggressive)
                        levelCount++;
                }

                if (levelCount < MaxLevelCount)
                {
                    if (_triggerWait >= CheckDuration)
                    {
                        if (!OnlyCallInCover || _motor.IsInCover)
                        {
                            _triggerWait = 0;
                            _triggerSpacing = CheckSpacing;
                            _isWaitingForCall = true;
                            Message("ToMakeCall");
                        }
                    }
                    else
                        _triggerWait += Time.deltaTime;
                }
                else
                    _triggerWait = 0;
            }
            else
                _triggerWait = 0;
        }

        #endregion
    }
}
