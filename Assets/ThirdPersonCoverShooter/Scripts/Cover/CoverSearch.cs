using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages a cache of covers used for search.
    /// </summary>
    public class CoverSearch
    {
        private Vector3 _position;
        private Vector3 _head;
        private CoverState _current;
        private float _capsuleRadius;
        private float _takeRadius;
        private float _crouchRadius;
        private float _climbRadius;
        private CoverSettings _settings;

        private Cover[] _covers = new Cover[16];
        private int _coverCount;

        private static Dictionary<GameObject, Cover> _coverMap = new Dictionary<GameObject, Cover>();

        /// <summary>
        /// Optimised way to get a cover component.
        /// </summary>
        public static Cover GetCover(GameObject gameObject)
        {
            if (!_coverMap.ContainsKey(gameObject))
                _coverMap[gameObject] = gameObject.GetComponent<Cover>();

            return _coverMap[gameObject];
        }

        /// <summary>
        /// Clears the cover list.
        /// </summary>
        public void Clear()
        {
            _coverCount = 0;
        }

        /// <summary>
        /// Updates cover search.
        /// </summary>
        public void Update(CoverState current, Vector3 position, Vector3 head, float takeRadius, float crouchRadius, float climbRadius, float capsuleRadius, CoverSettings settings)
        {
            _current = current;
            _position = position;
            _head = head;
            _capsuleRadius = capsuleRadius;
            _settings = settings;
            _takeRadius = takeRadius;
            _crouchRadius = crouchRadius;
            _climbRadius = climbRadius;

            var searchRadius = takeRadius > crouchRadius ? takeRadius : crouchRadius;
            searchRadius = searchRadius > climbRadius ? searchRadius : climbRadius;

            var colliderCount = Physics.OverlapSphereNonAlloc(position, searchRadius, Util.Colliders, Layers.Cover);

            _coverCount = 0;

            for (int i = 0; i < colliderCount; i++)
            {
                var cover = GetCover(Util.Colliders[i].gameObject);

                if (cover != null)
                {
                    _covers[_coverCount++] = cover;

                    if (_coverCount == _covers.Length)
                        break;
                }
            }
        }

        /// <summary>
        /// Find a cover closest to the character.
        /// </summary>
        public Cover FindClosest()
        {
            Cover result = null;

            for (int i = 0; i < _coverCount; i++)
            {
                var cover = _covers[i];

                if (cover != null && cover == _current.Main && doesCoverFit(cover, true))
                    result = cover;
            }

            _head.y = _position.y;

            for (int i = 0; i < _coverCount; i++)
            {
                var cover = _covers[i];

                if (cover != null && cover != _current.Main && doesCoverFit(cover, true))
                {
                    if (result == null)
                        result = cover;
                    else
                    {
                        var headDistance = Vector3.Distance(_head, cover.ClosestPointTo(_head, 0, 0));

                        if (headDistance < 0.3f)
                            result = cover;
                    }
                }
            }

            return result;
        }

        public bool IsCloserThan(Cover first, Cover second, float threshold)
        {
            var firstDistance = Vector3.Distance(_head, first.ClosestPointTo(_head, 0, 0));
            var secondDistance = Vector3.Distance(_head, second.ClosestPointTo(_head, 0, 0));

            return firstDistance + threshold < secondDistance;
        }

        /// <summary>
        /// Find a cover closest to the character.
        /// </summary>
        public bool FindClosestCrouchPosition(ref Vector3 position)
        {
            Cover result = null;
            _head.y = _position.y;

            for (int i = 0; i < _coverCount; i++)
            {
                var cover = _covers[i];

                if (cover != null && !cover.IsTall && doesCoverFit(cover, false))
                {
                    if (result == null)
                    {
                        result = cover;
                        position = cover.ClosestPointTo(_head, 0, 0);
                    }
                    else
                    {
                        var closest = cover.ClosestPointTo(_head, 0, 0);
                        var headDistance = Vector3.Distance(_head, closest);

                        if (headDistance < _crouchRadius)
                        {
                            result = cover;
                            position = closest;
                        }
                    }
                }
            }

            return result != null;
        }

        /// <summary>
        /// Find a cover in the given direction to climb on.
        /// </summary>
        public Cover FindClimbCoverInDirection(Vector3 direction)
        {
            Cover result = null;
            float resultDot = 0;
            float resultDistance = 0;

            for (int i = 0; i < _coverCount; i++)
            {
                var cover = _covers[i];
                var closest = cover.ClosestPointTo(_position, 0, 0);

                var dot = Vector3.Dot(cover.Forward, direction);

                if (dot < 0.5f)
                    continue;

                var vector = closest - _position;
                var distance = vector.magnitude;

                if (distance <= float.Epsilon) distance = float.Epsilon;
                dot = Vector3.Dot(direction, vector / distance);

                if (dot < float.Epsilon)
                    continue;

                if (result == null || (dot > resultDot && distance < resultDistance))
                {
                    result = cover;
                    resultDot = dot;
                    resultDistance = distance;
                }
            }

            return result;
        }

        /// <summary>
        /// Check if the given cover is fitting.
        /// </summary>
        private bool doesCoverFit(Cover cover, bool checkDistance)
        {
            if (cover == null || cover.Top < _position.y + 0.5f)
                return false;

            var position = _position + cover.Forward * _capsuleRadius;
            var distance = Vector3.Distance(position, cover.ClosestPointTo(position, 0, 0));

            float radius;

            if (cover == _current.Main)
                radius = cover.CheckTall(_position.y) ? _settings.TallSideLeaveRadius : _settings.LowSideLeaveRadius;
            else
                radius = cover.CheckTall(_position.y) ? _settings.TallSideEnterRadius : _settings.LowSideEnterRadius;

            var isInFront = cover.IsInFront(_position, cover == _current.Main) &&
                            (cover.IsInFront(_position + cover.Right * radius, cover == _current.Main) || cover.RightAdjacent != null) &&
                            (cover.IsInFront(_position + cover.Left * radius, cover == _current.Main) || cover.LeftAdjacent != null);

            if (checkDistance)
            {
                var isOld = isInFront && distance <= _settings.LeaveDistance && cover == _current.Main;
                if (isOld) return true;

                var isNew = isInFront && distance <= _settings.EnterDistance && cover != _current.Main;
                if (isNew) return true;

                return false;
            }
            else
                return isInFront;
        }
    }
}