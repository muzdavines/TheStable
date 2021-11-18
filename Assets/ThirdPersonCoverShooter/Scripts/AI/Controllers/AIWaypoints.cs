using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// When asked to, walks the character motor around the waypoints. The AI can be made to run or wait at each position.
    /// </summary>
    public class AIWaypoints : AIBase
    {
        /// <summary>
        /// Points to visit.
        /// </summary>
        [Tooltip("Points to visit.")]
        [HideInInspector]
        public Waypoint[] Waypoints;

        private bool _isVisiting;
        private bool _isWaiting;
        private int _waypoint;
        private float _waitTime;
        private bool _forceTake = true;

        private bool _foundWaypoints = false;

        /// <summary>
        /// Told by the brains to start visiting points in order.
        /// </summary>
        public void ToStartVisitingWaypoints()
        {
            _isVisiting = true;
            _isWaiting = false;
            _waypoint = -1;

            _foundWaypoints = Waypoints != null && Waypoints.Length > 0;

            if (_foundWaypoints && isActiveAndEnabled)
                Message("OnWaypointsFound");
        }

        /// <summary>
        /// Told by the brains to stop visiting waypoints.
        /// </summary>
        public void ToStopVisitingWaypoints()
        {
            _isVisiting = false;
        }

        private void Update()
        {
            if (!_isVisiting)
                return;

            if (!_foundWaypoints && Waypoints != null && Waypoints.Length > 0)
            {
                _foundWaypoints = true;
                Message("OnWaypointsFound");
            }

            if (!_foundWaypoints)
                return;

            if (_waypoint < 0 || _waypoint >= Waypoints.Length)
                _isWaiting = false;

            if (_isWaiting)
            {
                _waitTime += Time.deltaTime;

                if (Waypoints[_waypoint].Pause <= _waitTime)
                {
                    _waypoint = (_waypoint + 1) % Waypoints.Length;
                    _isWaiting = false;
                    _forceTake = true;
                    _waitTime = 0;
                }
            }
            else
            {
                var moveTo = false;

                if (_waypoint < 0 || _waypoint >= Waypoints.Length)
                {
                    _waypoint = 0;
                    var dist = Vector3.Distance(transform.position, Waypoints[0].Position);

                    for (int i = 1; i < Waypoints.Length; i++)
                    {
                        var current = Vector3.Distance(transform.position, Waypoints[i].Position);

                        if (current < dist)
                        {
                            dist = current;
                            _waypoint = i;
                        }
                    }

                    moveTo = true;
                }
                else
                    moveTo = _forceTake;

                _forceTake = false;

                if (Vector3.Distance(transform.position, Waypoints[_waypoint].Position) < 0.65f)
                {
                    if (Waypoints[_waypoint].Pause > 1f / 60f || Waypoints.Length == 1)
                    {
                        _isWaiting = true;
                        moveTo = false;
                        Message("ToStopMoving");
                    }
                    else
                    {
                        _waypoint = (_waypoint + 1) % Waypoints.Length;
                        moveTo = true;
                    }
                }

                if (moveTo)
                {
                    if (Waypoints[_waypoint].Run)
                        Message("ToRunTo", Waypoints[_waypoint].Position);
                    else
                        Message("ToWalkTo", Waypoints[_waypoint].Position);

                    Message("ToFaceWalkDirection");
                }
            }
        }
    }
}
