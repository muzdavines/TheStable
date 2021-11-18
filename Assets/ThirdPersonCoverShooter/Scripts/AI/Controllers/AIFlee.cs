using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// If AI becomes scared they flee towards a flee zone, ignoring everything else.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AIFlee : AIBase
    {
        #region Public fields

        /// <summary>
        /// Distance to the enemy at which the AI changes their flee direction.
        /// </summary>
        [Tooltip("Distance to the enemy at which the AI changes their flee direction.")]
        public float AvoidDistance = 10;

        #endregion

        #region Private fields

        private bool _isFleeing;
        private bool _wasTooClose;
        private Vector3 _threatPosition;

        private Vector3 _targetPosition;
        private FleeZone _targetBlock;

        private NavMeshPath _path;
        private Vector3[] _corners = new Vector3[32];

        private FleeZone _zone;
        private bool _wasRegistered;

        #endregion

        #region Events

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        /// <param name="position"></param>
        public void OnThreatPosition(Vector3 position)
        {
            if (_isFleeing)
                _threatPosition = position;
        }

        private void OnTriggerEnter(Collider other)
        {
            var zone = other.GetComponent<FleeZone>();

            if (zone != null)
            {
                if (_zone != null) _zone.Unregister(gameObject);
                _zone = zone;

                if (_isFleeing)
                {
                    _zone.Register(gameObject);
                    _wasRegistered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var zone = other.GetComponent<FleeZone>();

            if (zone != null && zone == _zone)
            {
                _zone.Unregister(gameObject);
                _wasRegistered = false;
                _zone = null;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains to flee away from a threat position.
        /// </summary>
        /// <param name="position"></param>
        public void ToStartFleeing(Vector3 threatPosition)
        {
            _isFleeing = true;
            _threatPosition = threatPosition;
            _wasTooClose = Vector3.Distance(_threatPosition, transform.position) < AvoidDistance;

            if (isActiveAndEnabled)
                findNewFleePosition(_wasTooClose);
        }

        /// <summary>
        /// Told by the brains to stop fleeing.
        /// </summary>
        public void ToStopFleeing()
        {
            if (_isFleeing && isActiveAndEnabled)
                Message("ToStopMoving");

            _isFleeing = false;
            _targetBlock = null;
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _path = new NavMeshPath();
        }

        private void Update()
        {
            if (!_isFleeing)
                return;

            if (_isFleeing && !_wasRegistered)
            {
                if (_zone != null)
                {
                    _zone.Register(gameObject);
                    _wasRegistered = true;
                }
            }
            else if (!_isFleeing && _wasRegistered)
            {
                if (_zone != null) _zone.Unregister(gameObject);
                _wasRegistered = false;
            }

            var isTooClose = Vector3.Distance(_threatPosition, transform.position) < AvoidDistance;

            if ((isTooClose && !_wasTooClose) ||
                _targetBlock == null ||
                (Vector3.Distance(_threatPosition, _targetPosition) < AvoidDistance))
                findNewFleePosition(isTooClose);

            _wasTooClose = isTooClose;
        }

        private void findNewFleePosition(bool isAlreadyTooClose)
        {
            var targetPosition = transform.position;
            var targetDot = -10f;

            var fromThreat = transform.position - _threatPosition;
            fromThreat.y = 0;
            fromThreat.Normalize();

            FleeZone targetBlock = null;

            for (int z = 0; z < FleeZone.Count; z++)
            {
                var block = FleeZone.Get(z);

                var position = block.transform.position;
                position.y = block.Bottom;

                if (Vector3.Distance(position, _threatPosition) < AvoidDistance)
                    continue;

                if (block == _targetBlock)
                    continue;

                var dot = -1f;

                if (NavMesh.CalculatePath(transform.position, position, 1, _path))
                {
                    var count = _path.GetCornersNonAlloc(_corners);

                    if (count < 1)
                        continue;

                    if (!isAlreadyTooClose)
                    {
                        var isOk = true;

                        for (int i = 0; i < count; i++)
                        {
                            var a = i == 0 ? transform.position : _corners[i - 1];
                            var b = _corners[i];

                            var closest = Util.FindClosestToPath(a, b, _threatPosition);

                            if (Vector3.Distance(closest, _threatPosition) < AvoidDistance)
                            {
                                isOk = false;
                                break;
                            }
                        }

                        if (!isOk)
                            continue;
                    }

                    var first = _corners[0];
                    if (count > 1) first = _corners[1];

                    var vector = first - transform.position;
                    vector.y = 0;
                    vector.Normalize();

                    dot = Vector3.Dot(vector, fromThreat) * Vector3.Distance(position, transform.position);
                }
                else
                    continue;

                if (dot > targetDot)
                {
                    targetDot = dot;
                    targetPosition = position + (-block.Width * 0.5f + Random.Range(0f, block.Width)) * block.transform.right +
                                                (-block.Depth * 0.5f + Random.Range(0f, block.Depth)) * block.transform.forward;

                    targetBlock = block;
                }
            }

            _targetBlock = targetBlock;
            _targetPosition = targetPosition;

            Message("ToSprintTo", targetPosition);
            Message("ToFaceWalkDirection", targetPosition);
        }

        #endregion
    }
}
