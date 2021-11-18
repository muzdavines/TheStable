using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoverShooter
{
    /// <summary>
    /// Continuously updates a list of visible actors. Without this component the AI is blind. 
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AISight : AIBase
    {
        #region Public fields

        /// <summary>
        /// Distance for AI to see objects in the world.
        /// </summary>
        [Tooltip("Distance for AI to see objects in the world.")]
        public float Distance = 25;

        /// <summary>
        /// Distance at which the AI ignores closer obstacles preventing it from seeing something.
        /// </summary>
        [Tooltip("Distance at which the AI ignores closer obstacles preventing it from seeing something.")]
        public float ObstacleIgnoreDistance = 1;

        /// <summary>
        /// Field of sight to notice changes in the world.
        /// </summary>
        [Tooltip("Field of sight to notice changes in the world.")]
        public float FieldOfView = 160;

        /// <summary>
        /// Time in seconds between each visibility update.
        /// </summary>
        [Tooltip("Time in seconds between each visibility update.")]
        public float UpdateDelay = 0.1f;

        /// <summary>
        /// Should a debug graphic be drawn to show the field of view.
        /// </summary>
        [Tooltip("Should a debug graphic be drawn to show the field of view.")]
        public bool DebugFOV = false;

        #endregion

        #region Private fields

        private Actor _actor;
        private FighterBrain _brain;

        private List<Actor> _visible = new List<Actor>();
        private List<Actor> _oldVisible = new List<Actor>();

        private HashSet<Actor> _visibleHash = new HashSet<Actor>();
        private HashSet<Actor> _oldVisibleHash = new HashSet<Actor>();
        private HashSet<Actor> _seenDeadHash = new HashSet<Actor>();

        private Dictionary<Actor, float> _lastSeenAlive = new Dictionary<Actor, float>();

        private Collider[] _colliders = new Collider[128];

        private float _wait = 0;

        private bool _isAlerted;

        #endregion

        #region Commands

        /// <summary>
        /// Notified that there is no longer a threat.
        /// </summary>
        public void OnNoThreat()
        {
            _isAlerted = false;
        }

        /// <summary>
        /// Checks if the given actor is no longer visible.
        /// </summary>
        public void DoubleCheck(Actor actor)
        {
            if (!CheckVisibility(actor))
                if (_visibleHash.Contains(actor))
                {
                    _visible.Remove(actor);
                    _visibleHash.Remove(actor);
                    Message("OnUnseeActor", actor);
                }
        }

        /// <summary>
        /// Checks if the given actor is in darkness.
        /// </summary>
        public bool IsInDarkness(Actor actor)
        {
            var viewDistance = actor.GetViewDistance(Distance, _isAlerted);

            return viewDistance < Distance && Vector3.Distance(actor.TopPosition, _actor.TopPosition) > viewDistance;
        }

        /// <summary>
        /// Checks if the given actor is field of view. Result is immediate, does not execute any events or messages.
        /// </summary>
        public bool CheckVisibility(Actor actor)
        {
            var viewDistance = actor.GetViewDistance(Distance, _isAlerted);

            return AIUtil.IsInSight(_actor, actor.TopPosition, viewDistance, FieldOfView, ObstacleIgnoreDistance);
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _brain = GetComponent<FighterBrain>();
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            _wait -= Time.deltaTime;

            if (_wait > float.Epsilon)
            {
                _oldVisible.Clear();
                _oldVisibleHash.Clear();

                for (int i = 0; i < _visible.Count; i++)
                {
                    var actor = _visible[i];

                    if (actor != null)
                    {
                        _oldVisible.Add(actor);
                        _oldVisibleHash.Add(actor);
                    }
                }

                for (int i = 0; i < _oldVisible.Count; i++)
                {
                    var actor = _oldVisible[i];

                    if (!actor.IsAlive)
                    {
                        _visible.Remove(actor);
                        _visibleHash.Remove(actor);

                        if (!_seenDeadHash.Contains(actor))
                        {
                            _seenDeadHash.Add(actor);
                            Message("OnSeeDeath", actor);
                        }
                    }
                }

                return;
            }

            _wait = Random.Range(UpdateDelay * 0.8f, UpdateDelay * 1.2f);

            _oldVisible.Clear();
            _oldVisibleHash.Clear();

            for (int i = 0; i < _visible.Count; i++)
            {
                var actor = _visible[i];

                if (actor != null)
                {
                    _oldVisible.Add(actor);
                    _oldVisibleHash.Add(actor);
                }
            }

            _visible.Clear();
            _visibleHash.Clear();

            var count = AIUtil.FindActorsIncludingDead(_actor.TopPosition, Distance, _actor);

            for (int i = 0; i < count; i++)
            {
                var actor = AIUtil.Actors[i];
                
                if (CheckVisibility(actor))
                {
                    if (actor.IsAlive)
                    {
                        _visible.Add(actor);
                        _visibleHash.Add(actor);
                    }
                    else
                    {
                        if (!_seenDeadHash.Contains(actor))
                        {
                            _seenDeadHash.Add(actor);
                            Message("OnSeeDeath", actor);
                        }
                        // A hack, should fix in some other way
                        else if (_brain != null && _brain.Threat == actor)
                            _brain.OnSeeDeath(actor);
                    }
                }
            }

            for (int i = 0; i < _oldVisible.Count; i++)
            {
                var actor = _oldVisible[i];

                if (!_visibleHash.Contains(actor))
                    Message("OnUnseeActor", actor);
            }

            for (int i = 0; i < _visible.Count; i++)
            {
                var actor = _visible[i];

                if (!_oldVisibleHash.Contains(actor))
                    Message("OnSeeActor", actor);
            }
        }

        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (DebugFOV)
                drawFOV();
        }

        private void OnDrawGizmosSelected()
        {
            if (DebugFOV)
                drawFOV();
        }

        private void drawFOV()
        {
            var vector = _actor == null ? transform.forward : _actor.HeadDirection;
            var head = _actor == null ? (transform.position + Vector3.up * 2) : _actor.StandingTopPosition;
            var angle = Util.HorizontalAngle(vector);

            var left = Util.HorizontalVector(angle - FieldOfView * 0.5f);

            Handles.color = new Color(0.5f, 0.3f, 0.3f, 0.1f);
            Handles.DrawSolidArc(head, Vector3.up, left, FieldOfView, Distance);

            Handles.color = new Color(1, 0.3f, 0.3f, 0.75f);
            Handles.DrawWireArc(head, Vector3.up, left, FieldOfView, Distance);
            Handles.DrawLine(head, head + left * Distance);
            Handles.DrawLine(head, head + Util.HorizontalVector(angle + FieldOfView * 0.5f) * Distance);
        }

        #endif

        #endregion
    }
}
