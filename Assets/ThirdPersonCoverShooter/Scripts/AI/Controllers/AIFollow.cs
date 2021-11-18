using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Moves and rotates the character motor so that it faces and follows the enemy when needed.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AIFollow : AIBase
    {
        #region Public fields

        /// <summary>
        /// Distance to maintain against a threat.
        /// </summary>
        [Tooltip("Distance to maintain against a threat.")]
        public float AvoidDistance = 10;

        /// <summary>
        /// AI walks closer if it is further than this distance.
        /// </summary>
        [Tooltip("AI walks closer if it is further than this distance.")]
        public float FollowDistance = 20;

        /// <summary>
        /// Is the AI using covers to follow a threat.
        /// </summary>
        [Tooltip("Is the AI using covers to follow a threat.")]
        public bool UseCovers = true;

        /// <summary>
        /// Chance that the AI will leave the cover to follow the threat.
        /// </summary>
        [Tooltip("Chance that the AI will leave covers to follow the threat.")]
        [Range(0, 1)]
        public float FollowFromCoverChance = 0.6f;

        /// <summary>
        /// Is the AI running towards covers.
        /// </summary>
        [Tooltip("Is the AI running towards covers.")]
        public bool RunToCovers = false;

        /// <summary>
        /// Should the AI continuously turn back and forward between walk direction and the threat.
        /// </summary>
        [Tooltip("Should the AI continuously turn back and forward between walk direction and the threat.")]
        public bool UsePeeks = false;

        /// <summary>
        /// Duration of a peek in seconds.
        /// </summary>
        [Tooltip("Duration of a peek in seconds.")]
        public float PeekDuration = 1.5f;

        /// <summary>
        /// How long to wait between peeks.
        /// </summary>
        [Tooltip("How long to wait between peeks.")]
        public float PeekDelay = 3f;

        #endregion

        #region Private fields

        private Actor _actor;

        private bool _isFollowing;
        private Vector3 _threatPosition;

        private bool _hasFoundCover;
        private bool _isUsingCover;

        private bool _isFollowingFromCovers;

        private float _peek = 0;

        private bool _isKeepingCloseTo;
        private KeepCloseTo _keepCloseTo;

        #endregion

        #region Events

        /// <summary>
        /// Notified by another component that a cover was found.
        /// </summary>
        public void OnFoundCover()
        {
            _hasFoundCover = true;
        }

        /// <summary>
        /// Notified by another component that current cover is no longer valid.
        /// </summary>
        public void OnInvalidCover()
        {
            if (_isUsingCover && _isFollowing && isActiveAndEnabled)
                findNewCover();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        /// <param name="position"></param>
        public void OnThreatPosition(Vector3 position)
        {
            _threatPosition = position;

            if (isActiveAndEnabled && _isFollowing && (_isFollowingFromCovers || _actor.Cover == null))
            {
                if (needsNewPosition)
                {
                    if (!_isUsingCover)
                    {
                        if (UseCovers)
                            findNewCover();
                        else
                            findNewWalkPosition();
                    }
                }
                else if (UseCovers && !_isUsingCover)
                    findNewCover();
            }
        }

        /// <summary>
        /// Told by the brains to follow a position.
        /// </summary>
        public void ToStartFollowing(Vector3 position)
        {
            _isFollowing = true;
            _threatPosition = position;
            _isFollowingFromCovers = Random.Range(0f, 1f) <= FollowFromCoverChance;

            if (isActiveAndEnabled)
            {
                if (UseCovers)
                    findNewCover();
                else
                    findNewWalkPosition();
            }
        }

        /// <summary>
        /// Told by the brains to stop following.
        /// </summary>
        public void ToStopFollowing()
        {
            if (_isFollowing && isActiveAndEnabled)
                Message("ToStopMoving");

            _isFollowing = false;
        }

        /// <summary>
        /// Told by the brains to keep close to a position.
        /// </summary>
        public void ToKeepCloseTo(KeepCloseTo value)
        {
            _isKeepingCloseTo = true;
            _keepCloseTo = value;
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        private void Update()
        {
            if (!_isFollowing)
                return;

            if (!_isUsingCover || _actor.Cover != null || _peek > PeekDuration || !UsePeeks)
            {
                Message("ToTurnAt", _threatPosition);
                Message("ToAimAt", _threatPosition);
            }
            else
                Message("ToFaceWalkDirection");

            _peek += Time.deltaTime;
            _peek %= PeekDuration + PeekDelay;
        }

        #endregion

        #region Find methods

        private void findNewCover()
        {
            _hasFoundCover = false;
            _isUsingCover = false;

            if (RunToCovers)
                Message("ToRunToCovers");
            else
                Message("ToWalkToCovers");

            Message("ToTakeCoverAgainst", _threatPosition);

            if (_hasFoundCover)
                _isUsingCover = true;
            else
                findNewWalkPosition();
        }

        private bool needsNewPosition
        {
            get
            {
                var vector = _threatPosition - transform.position;
                vector.y = 0;

                return vector.magnitude < AvoidDistance || vector.magnitude > FollowDistance;
            }
        }

        private void findNewWalkPosition()
        {
            var vector = _threatPosition - transform.position;
            vector.y = 0;

            if (vector.magnitude < AvoidDistance)
                walkTo(_threatPosition - vector.normalized * FollowDistance);
            else if (vector.magnitude > FollowDistance)
                walkTo(_threatPosition - vector.normalized * AvoidDistance);
        }

        private void walkTo(Vector3 position)
        {
            if (_isKeepingCloseTo && Vector3.Distance(position, _keepCloseTo.Position) > _keepCloseTo.Distance)
                position = _keepCloseTo.Position + (position - _keepCloseTo.Position).normalized * _keepCloseTo.Distance;

            Message("ToWalkTo", position);
        }

        #endregion
    }
}
