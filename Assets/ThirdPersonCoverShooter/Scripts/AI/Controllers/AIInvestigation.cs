using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// When the fighter brains enters investigation mode it passes the command of the character motor to this component. It makes the AI walk slowly towards the position the enemy was last seen at.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AIInvestigation : AIBase
    {
        #region Public fields

        /// <summary>
        /// Distance to the investigation position for it to be marked as investigated.
        /// </summary>
        [Tooltip("Distance to the investigation position for it to be marked as investigated.")]
        public float VerifyDistance = 10;

        /// <summary>
        /// At which height the AI confirms the point as investigated.
        /// </summary>
        [Tooltip("At which height the AI confirms the point as investigated.")]
        public float VerifyHeight = 0.3f;

        /// <summary>
        /// Radius of an investigation point when it's close to a cover. AI tries to verify all of it is clear of enemies when investigating. Aiming is done at the central point so keep the radius small.
        /// </summary>
        [Tooltip("Radius of an investigation point when it's close to a cover. AI tries to verify all of it is clear of enemies when investigating. Aiming is done at the central point so keep the radius small.")]
        [HideInInspector]
        public float VerifyRadius = 1;

        /// <summary>
        /// Field of view when checking an investigation position.
        /// </summary>
        [Tooltip("Field of view when checking an investigation position.")]
        public float FieldOfView = 90;

        /// <summary>
        /// Distance to a cover to maintain when approaching to see behind it.
        /// </summary>
        [Tooltip("Distance to a cover to maintain when approaching to see behind it.")]
        public float CoverOffset = 2;

        /// <summary>
        /// Cover search radius around an investigation point. Closest cover will be checked when investigating.
        /// </summary>
        [Tooltip("Cover search radius around an investigation point. Closest cover will be checked when investigating.")]
        [HideInInspector]
        public float CoverSearchDistance = 3;

        #endregion

        #region Private fields

        private Actor _actor;

        private bool _isInvestigating;
        private Vector3 _positionToInvestigate;
        private Vector3 _positionAskedToInvestigate;
        private Cover _coverToInvestigate;
        private bool _hasReachedCoverLine;
        private Vector3 _approachPosition;
        private float _verifyDistance;

        private Vector3 _walkingTo;
        private bool _isWalkingTo;

        private NavMeshPath _path;
        private Vector3[] _corners = new Vector3[32];

        #endregion

        #region Commands

        /// <summary>
        /// Responds with an answer to a brain enquiry.
        /// </summary>
        public void InvestigationCheck()
        {
            if (isActiveAndEnabled)
                Message("InvestigationResponse");
        }

        /// <summary>
        /// Told by the brains to investigate a position.
        /// </summary>
        public void ToInvestigatePosition(Vector3 position)
        {
            _isInvestigating = true;

            _positionAskedToInvestigate = position;
            _positionToInvestigate = position;

            Util.GetClosestCover(position, CoverSearchDistance, ref _coverToInvestigate, ref _positionToInvestigate);

            _verifyDistance = Util.GetViewDistance(_positionToInvestigate, VerifyDistance, true);

            if (_coverToInvestigate == null)
            {
                _hasReachedCoverLine = false;

                if (isActiveAndEnabled)
                {
                    walkTo(position);
                    Message("OnInvestigationStart");
                }
            }
            else
            {
                var vector = _positionToInvestigate - transform.position;
                _hasReachedCoverLine = Vector3.Dot(_coverToInvestigate.Forward, vector + _coverToInvestigate.Forward * 0.2f) > 0;

                if (_hasReachedCoverLine)
                {
                    if (isActiveAndEnabled)
                    {
                        walkTo(_positionToInvestigate);
                        Message("OnInvestigationStart");
                    }
                }
                else
                {
                    var start = transform.position;

                    if (AIUtil.GetClosestStandablePosition(ref start))
                    {
                        var leftOrigin = _coverToInvestigate.LeftCorner(_coverToInvestigate.Bottom, 0) - _coverToInvestigate.Forward * 1.0f;
                        var rightOrigin = _coverToInvestigate.RightCorner(_coverToInvestigate.Bottom, 0) - _coverToInvestigate.Forward * 1.0f;

                        var left = _coverToInvestigate.LeftCorner(_coverToInvestigate.Bottom, CoverOffset) - _coverToInvestigate.Forward * 1.0f;
                        var right = _coverToInvestigate.RightCorner(_coverToInvestigate.Bottom, CoverOffset) - _coverToInvestigate.Forward * 1.0f;

                        var originalLeft = left;
                        var originalRight = right;

                        var hasLeft = AIUtil.GetClosestStandablePosition(ref left) && !AIUtil.IsNavigationBlocked(leftOrigin, left);
                        var hasRight = AIUtil.GetClosestStandablePosition(ref right) && !AIUtil.IsNavigationBlocked(rightOrigin, right);

                        if (hasLeft || hasRight)
                        {
                            var leftLength = 999999f;
                            var rightLength = 999999f;

                            if (hasLeft)
                            {
                                AIUtil.Path(ref _path, start, left);

                                if (_path.status != NavMeshPathStatus.PathInvalid)
                                {
                                    leftLength = 0;
                                    var cornerCount = _path.GetCornersNonAlloc(_corners);

                                    for (int i = 1; i < cornerCount; i++)
                                        leftLength += Vector3.Distance(_corners[i], _corners[i - 1]);
                                }
                            }

                            if (hasRight)
                            {
                                AIUtil.Path(ref _path, start, right);

                                if (_path.status != NavMeshPathStatus.PathInvalid)
                                {
                                    rightLength = 0;
                                    var cornerCount = _path.GetCornersNonAlloc(_corners);

                                    for (int i = 1; i < cornerCount; i++)
                                        rightLength += Vector3.Distance(_corners[i], _corners[i - 1]);
                                }
                            }

                            if (leftLength < rightLength)
                                _approachPosition = left;
                            else
                                _approachPosition = right;
                        }
                        else
                            _approachPosition = position;
                    }
                    else
                        _approachPosition = position;

                    var distance = Vector3.Distance(_approachPosition, _positionToInvestigate);

                    if (distance + VerifyRadius > _verifyDistance)
                        _approachPosition = _positionToInvestigate + Vector3.Normalize(_approachPosition - _positionToInvestigate) * (_verifyDistance + VerifyRadius - 0.1f);

                    if (isActiveAndEnabled)
                    {
                        walkTo(_approachPosition);
                        Message("OnInvestigationStart");
                    }
                }
            }
        }

        /// <summary>
        /// Told by the brains to stop investigating.
        /// </summary>
        public void ToStopInvestigation()
        {
            _isWalkingTo = false;
            _isInvestigating = false;
            Message("OnInvestigationStop");
        }

        #endregion

        #region Events

        /// <summary>
        /// Notified that a position is unreachable. Sets the investigation as finished if the position has been being investigated.
        /// </summary>
        public void OnPositionUnreachable(Vector3 position)
        {
            if (_isWalkingTo && Vector3.Distance(_walkingTo, position) <= 0.2f)
            {
                _isWalkingTo = false;

                if (_isInvestigating)
                    done();
            }
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _path = new NavMeshPath();
        }

        private void Update()
        {
            if (!_isInvestigating)
                return;

            if (_coverToInvestigate != null)
            {
                if (!_hasReachedCoverLine && Vector3.Dot(_coverToInvestigate.Forward, _approachPosition - transform.position + _coverToInvestigate.Forward * 0.2f) > 0)
                {
                    _hasReachedCoverLine = true;
                    walkTo(_positionToInvestigate);
                }

                if (verify(_positionToInvestigate) &&
                     verify(_positionToInvestigate + _coverToInvestigate.Right * VerifyRadius) &&
                     verify(_positionToInvestigate - _coverToInvestigate.Left * VerifyRadius))
                {
                    done();
                }
            }
            else if (verify(_positionToInvestigate))
                done();
        }

        private bool verify(Vector3 position)
        {
            var distance = Vector3.Distance(transform.position, position);

            if (distance <= VerifyRadius + 0.1f || (distance <= _verifyDistance + 0.1f && _verifyDistance <= VerifyRadius))
                return true;

            return AIUtil.IsInSight(_actor, position + Vector3.up * VerifyHeight, _verifyDistance, FieldOfView);
        }

        private void walkTo(Vector3 position)
        {
            _isWalkingTo = true;
            _walkingTo = position;
            Message("ToWalkTo", position);
        }

        private void done()
        {
            _isWalkingTo = false;

            Message("ToMarkPointInspected", _positionToInvestigate);

            if (Vector3.Distance(_positionAskedToInvestigate, _positionToInvestigate) > float.Epsilon)
                Message("OnPointInvestigated", _positionAskedToInvestigate);

            Message("OnPointInvestigated", _positionToInvestigate);
        }

        #endregion
    }
}
