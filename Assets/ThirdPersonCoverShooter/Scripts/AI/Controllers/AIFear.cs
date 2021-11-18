using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Checks the situation and issues a command to the brains to become scared. Can be used by both civilians and fighters.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(AIFlee))]
    public class AIFear : AIBase, IAlertListener
    {
        #region Public fields

        /// <summary>
        /// AI doesn't get afraid of enemies with no equipped weapons.
        /// </summary>
        [Tooltip("AI doesn't get afraid of enemies with no equipped weapons.")]
        public bool OnlyScaredOfArmed = true;

        /// <summary>
        /// Chance that an alerted civilian will immediately flee.
        /// </summary>
        [Tooltip("Chance that an alerted civilian will immediately flee.")]
        [Range(0, 1)]
        public float ImmediateScareChance = 0.25f;

        /// <summary>
        /// Fraction of health at which the AI runs in fear.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Fraction of health at which the AI runs in fear.")]
        public float MinFightHealth = 0.25f;

        /// <summary>
        /// Should the AI flee after some time passes.
        /// </summary>
        [Tooltip("Should the AI flee after some time passes.")]
        public bool FleeAfterSomeTime = false;

        /// <summary>
        /// Minimum possible time the AI will fight before fleeing. Ised only when FleeAfterSomeTime is enabled.
        /// </summary>
        [Tooltip("Minimum possible time the AI will fight before fleeing.. Ised only when FleeAfterSomeTime is enabled.")]
        public float MinFleeTime = 0;

        /// <summary>
        /// Maximum possible the AI will fight before fleeing.
        /// </summary>
        [Tooltip("Maximum possible the AI will fight before fleeing.. Ised only when FleeAfterSomeTime is enabled.")]
        public float MaxFleeTime = 60;

        /// <summary>
        /// Will the AI flee on hearing gunfire and other hostile alerts.
        /// </summary>
        [Tooltip("Will the AI flee on hearing gunfire and other hostile alerts.")]
        public bool FleeOnHostileAlerts = false;

        /// <summary>
        /// Will the AI flee on sight of an active military.
        /// </summary>
        [Tooltip("Will the AI flee on sight of an active military.")]
        public bool FleeOnSeingMilitary = false;

        #endregion

        #region Private fields

        private Actor _actor;
        private CharacterHealth _health;

        private bool _isScared;
        private float _time;
        private bool _isCountingTime;

        private bool _wasThreatArmed;
        private Actor _threat;

        private List<Actor> _visibleFighters = new List<Actor>();

        #endregion

        #region Events

        /// <summary>
        /// Registers an alert. Flees if hostile and is told to do so.
        /// </summary>
        public void OnAlert(ref GeneratedAlert alert)
        {
            if (alert.IsHostile && FleeOnHostileAlerts && isActiveAndEnabled)
                flee();
        }

        /// <summary>
        /// Is notified of an existance of a threat. May immediately flee.
        /// </summary>
        public void OnThreat(Actor threat)
        {
            _threat = threat;
            _wasThreatArmed = threat.IsArmed;

            if (isActiveAndEnabled)
                if (_wasThreatArmed || !OnlyScaredOfArmed)
                    checkScare();
        }

        /// <summary>
        /// Is notified that there is no threat.
        /// </summary>
        public void OnNoThreat()
        {
            _threat = null;
            return;
        }

        /// <summary>
        /// Notified by the sight AI that an actor has entered the view.
        /// </summary>
        public void OnSeeActor(Actor actor)
        {
            if (actor.Side == _actor.Side && actor.IsAggressive)
                if (!_visibleFighters.Contains(actor))
                    _visibleFighters.Add(actor);
        }

        /// <summary>
        /// Notified by the sight AI that an actor has dissappeared from the view.
        /// </summary>
        public void OnUnseeActor(Actor actor)
        {
            if (actor.Side == _actor.Side && actor.IsAggressive)
                if (_visibleFighters.Contains(actor))
                    _visibleFighters.Remove(actor);
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _health = GetComponent<CharacterHealth>();
        }

        private void Update()
        {
            if (_isScared || !_actor.IsAlive)
                return;

            if (_threat != null && _threat.IsArmed && !_wasThreatArmed)
            {
                _wasThreatArmed = true;
                checkScare();
            }

            if (_health.Health < _health.MaxHealth * MinFightHealth)
            {
                flee();
                return;
            }

            if (FleeAfterSomeTime && _isCountingTime)
            {
                _time -= Time.deltaTime;

                if (_time < float.Epsilon)
                {
                    flee();
                    return;
                }
            }

            if (FleeOnSeingMilitary)
                for (int i = 0; i < _visibleFighters.Count; i++)
                    if (_visibleFighters[i].IsAlerted)
                    {
                        flee();
                        return;
                    }
        }

        #endregion

        #region Private methods

        private void checkScare()
        {
            if (!_isCountingTime)
            {
                _isCountingTime = true;

                if (MinFleeTime > MaxFleeTime)
                    _time = MinFleeTime;
                else
                    _time = Random.Range(MinFleeTime, MaxFleeTime);
            }

            if (Random.Range(0f, 1f) <= ImmediateScareChance)
                flee();
        }

        private void flee()
        {
            _isScared = true;
            Message("ToBecomeScared");
        }

        #endregion
    }
}
