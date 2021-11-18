using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Allows the AI to search the level to find an enemy. The AI searches each cover and search zone. Searches are done by positions. For example, a cover might have 4 positions the AI has to inspect. Positions are grouped into blocks and AI always checks every position inside a block before moving onto another block.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AISearch : AIBase
    {
        /// <summary>
        /// Offset from cover the AI keeps when approaching it from a side.
        /// </summary>
        public static float CoverOffset = 2;

        /// <summary>
        /// Search points belong in the same search block if they are closer to each other than this distance.
        /// </summary>
        public static float BlockThreshold = 3;

        /// <summary>
        /// A search point is considered to belong in a block if it is closer than this value to it's center.
        /// </summary>
        public static float BlockCenterThreshold = 6;

        /// <summary>
        /// Distance to target location the AI has to reach for it to be marked as investigated.
        /// </summary>
        public static float VerifyDistance = 16;

        /// <summary>
        /// Maximum distance of a location for AI to search.
        /// </summary>
        public static float MaxDistance = 10000;

        #region Public fields

        /// <summary>
        /// At which height the AI confirms the point as investigated.
        /// </summary>
        [Tooltip("At which height the AI confirms the point as investigated.")]
        public float VerifyHeight = 0.7f;

        /// <summary>
        /// Field of sight to register the search position.
        /// </summary>
        [Tooltip("Field of sight to register the search position.")]
        public float FieldOfView = 90;

        /// <summary>
        /// Distance at which AI turns from running to walking to safely investigate the position.
        /// </summary>
        [Tooltip("Distance at which AI turns from running to walking to safely investigate the position.")]
        public float WalkDistance = 8;

        /// <summary>
        /// Should a line to the intended search point be drawn in the editor.
        /// </summary>
        [Tooltip("Should a line to the intended search point be drawn in the editor.")]
        public bool DebugTarget = false;

        /// <summary>
        /// Should information about search points be displayed.
        /// </summary>
        [Tooltip("Should information about search points be displayed.")]
        public bool DebugPoints = false;

        #endregion

        #region Private fields

        private Actor _actor;

        private bool _hasSearchDirection;
        private bool _hasBlockDirection;
        private Vector3 _searchDirection;
        private Vector3 _blockDirection;
        private Vector3 _searchPosition;

        private bool _hasPoint;
        private int _pointIndex;
        private SearchPoint _point;

        private bool _hasPreviousPoint;
        private int _previousPointIndex;

        private bool _isSearching;
        private bool _wasRunning;
        private bool _hasApproached;

        private SearchPointData _points = new SearchPointData();
        private SearchBlock _block;
        private SearchBlockCache _blockCache;
        private List<SearchBlock> _investigatedBlocks = new List<SearchBlock>();
        private List<SearchBlock> _blocks = new List<SearchBlock>();
   
        private List<InvestigatedPoint> _investigated = new List<InvestigatedPoint>();

        private List<AISearch> _friends = new List<AISearch>();

        private float _timeOfReset;

        private float _checkWait;

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains to start search at the current location.
        /// </summary>
        public void ToSearch()
        {
            startSearch();
            _searchPosition = transform.position;
            _hasSearchDirection = false;
        }

        /// <summary>
        /// Told by the brains to start searching a ta position.
        /// </summary>>
        public void ToSearchAt(SearchPoint point)
        {
            startSearch();
            setPoint(addPoint(point));
            _hasBlockDirection = _hasSearchDirection;
            _blockDirection = _searchDirection;
        }

        /// <summary>
        /// Told by the brains to stop searching.
        /// </summary>
        public void ToStopSearch()
        {
            _isSearching = false;
        }

        /// <summary>
        /// Told by the brains to force mark a position as investigated.
        /// </summary>
        public void ToMarkPointInspected(Vector3 position)
        {
            if (!isActiveAndEnabled)
                return;

            var point = new InvestigatedPoint(position);

            if (!considerPoint(point))
                markInvestigated(point);

            for (int i = 0; i < _friends.Count; i++)
                _friends[i].considerPoint(point);
        }

        /// <summary>
        /// Told by the brains to forget all search history.
        /// </summary>
        public void ToClearSearchHistory()
        {
            _timeOfReset = Time.timeSinceLevelLoad;
            _investigated.Clear();
            _blocks.Clear();
            _block.Clear();
        }

        #endregion

        #region Events

        /// <summary>
        /// Responds with an answer to a brain enquiry.
        /// </summary>
        public void SearchCheck()
        {
            if (isActiveAndEnabled)
                Message("SearchResponse");
        }

        /// <summary>
        /// Notified that a friend was found.
        /// </summary>
        public void OnFoundFriend(Actor friend)
        {
            var search = friend.GetComponent<AISearch>();

            if (search != null && !_friends.Contains(search))
            {
                if (isActiveAndEnabled)
                    for (int i = 0; i < search._investigated.Count; i++)
                        considerPoint(search._investigated[i]);

                _friends.Add(search);
            }
        }

        /// <summary>
        /// Notified that a friend got out of range.
        /// </summary>
        public void OnLostFriend(Actor friend)
        {
            var search = friend.GetComponent<AISearch>();

            if (search != null && _friends.Contains(search))
                _friends.Remove(search);
        }

        /// <summary>
        /// Notified that a position is unreachable.
        /// </summary>
        public void OnPositionUnreachable(Vector3 point)
        {
            if (_isSearching && _hasPoint && Vector3.Distance(point, _point.ApproachPosition) < 0.5f)
                finishInvestigatingThePoint();
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _block = new SearchBlock(_points);
            _blockCache = new SearchBlockCache(_points);
            _actor = GetComponent<Actor>();
        }

        private void OnLevelWasLoaded(int level)
        {
            GlobalSearchCache.Restart();
        }

        private void Update()
        {
            GlobalSearchCache.Update();

            if (!_isSearching)
                return;

            if (_blocks.Count == 0 && !_hasPoint)
            {
                _isSearching = false;
                Message("OnFinishSearch");
            }

            if (DebugPoints)
            {
                foreach (var block in _blocks)
                    debugBlock(block);

                foreach (var block in _investigatedBlocks)
                    debugBlock(block);
            }

            if (_block.Empty && !_hasPoint && _blocks.Count > 0)
            {
                var pickedIndex = -1;
                var previousValue = 0f;

                for (int i = 0; i < _blocks.Count; i++)
                {
                    var vector = _searchPosition - _blocks[i].Center;
                    var distance = vector.magnitude;
                    var direction = vector / distance;

                    var value = distance;

                    if (_hasBlockDirection)
                        value *= -Vector3.Dot(direction, _blockDirection) * 0.5f + 1.5f;
                    else
                        value *= -Vector3.Dot(direction, _actor.HeadDirection) * 0.5f + 1.5f;

                    if (pickedIndex < 0 || value < previousValue)
                    {
                        pickedIndex = i;
                        previousValue = value;
                    }
                }

                _block = _blocks[pickedIndex];
                _blocks.RemoveAt(pickedIndex);
                _investigatedBlocks.Add(_block);

                _hasBlockDirection = true;
                _blockDirection = (_block.Center - _searchPosition).normalized;
            }

            if (!_hasPoint)
            {
                int index;
                float value;

                if (findBestPoint(_block, out index, out value))
                {
                    setPoint(_block.Indices[index]);
                    _block.Investigate(index);
                }
            }

            if (!_hasPoint)
                return;

            if (!_hasApproached && !shouldApproach(_point))
            {
                _hasApproached = true;

                if (_wasRunning)
                    run();
                else
                    walk();
            }

            if (_wasRunning && !shouldRunTo(_point.Position))
                walk();

            _checkWait -= Time.deltaTime;

            if (_checkWait <= float.Epsilon)
            {
                glimpse(_block);

                for (int b = _blocks.Count - 1; b >= 0; b--)
                {
                    glimpse(_blocks[b]);

                    if (_blocks[b].Empty)
                    {
                        _investigatedBlocks.Add(_blocks[b]);
                        _blocks.RemoveAt(b);
                    }
                }

                _checkWait = 0.25f;
            }

            if (DebugTarget)
                Debug.DrawLine(transform.position, _point.Position, Color.yellow);

            if (canBeInvestigated(_point))
                finishInvestigatingThePoint();
        }

        #endregion

        #region Private methods

        private void finishInvestigatingThePoint()
        {
            var point = new InvestigatedPoint(_point.Position);
            _hasPoint = false;

            markInvestigated(point);

            for (int i = 0; i < _friends.Count; i++)
                _friends[i].considerPoint(point);
        }

        private int addPoint(SearchPoint point)
        {
            point.CalcVisibility(VerifyDistance, false);
            var index = _points.Add(point);

            if (!_block.Empty)
                if (_block.IsClose(point, BlockThreshold, BlockCenterThreshold))
                {
                    _block.Add(index);
                    return index;
                }

            for (int i = 0; i < _blocks.Count; i++)
                if (_blocks[i].IsClose(point, BlockThreshold, BlockCenterThreshold))
                {
                    _blocks[i].Add(index);
                    return index;
                }

            var new_ = _blockCache.Take();
            new_.Add(index);
            _blocks.Add(new_);

            return index;
        }

        private void debugBlock(SearchBlock block)
        {
            var color = Color.white;

            switch (block.Index % 5)
            {
                case 0: color = Color.red; break;
                case 1: color = Color.green; break;
                case 2: color = Color.blue; break;
                case 3: color = Color.yellow; break;
                case 4: color = Color.cyan; break;
            }

            for (int i = 0; i < block.Count; i++)
                debugPoint(block.Get(i), false, color);

            foreach (var index in block.InvestigatedIndices)
                debugPoint(_points.Points[index], !_hasPoint || index != _pointIndex, color);    
        }

        private void debugPoint(SearchPoint point, bool wasInvestigated, Color color)
        {
            Debug.DrawLine(point.Position, point.Position + Vector3.up * (wasInvestigated ? 0.2f : 0.75f), color);

            //if (point.Left >= 0) Debug.DrawLine(point.Position, point.Position + (_points.Points[point.Left].Position - point.Position) * 0.25f, Color.white);
            //if (point.Right >= 0) Debug.DrawLine(point.Position, point.Position + (_points.Points[point.Right].Position - point.Position) * 0.25f, Color.magenta);
        }

        private bool findBestPoint(SearchBlock block, out int pointIndex, out float pointValue)
        {
            var pickedIndex = -1;
            var previousValue = 0f;

            var previousLeft = -1;
            var previousRight = -1;

            if (_hasPreviousPoint)
            {
                var previousPoint = _points.Points[_previousPointIndex];
                previousLeft = previousPoint.Left;
                previousRight = previousPoint.Right;
            }

            for (int i = 0; i < block.Count; i++)
            {
                var index = block.Indices[i];
                var point = block.Get(i);

                var vector = _searchPosition - point.Position;
                var distance = vector.magnitude;
                var direction = vector / distance;

                var value = distance;

                if (_hasPreviousPoint && (index == previousLeft || index == previousRight))
                    value *= -1;
                else
                {
                    if (_hasSearchDirection)
                        value *= -Vector3.Dot(direction, _searchDirection) * 0.5f + 1.5f;
                    else
                        value *= -Vector3.Dot(direction, _actor.HeadDirection) * 0.5f + 1.5f;
                }

                if (pickedIndex < 0 || (value > 0 && value < previousValue) || (value < 0 && previousValue < 0 && value > previousValue) || (value < 0 && previousValue > 0))
                {
                    pickedIndex = i;
                    previousValue = value;
                }
            }

            pointIndex = pickedIndex;
            pointValue = previousValue;

            return pointIndex >= 0;
        }

        private bool canBeInvestigated(SearchPoint point)
        {
            var position = point.Position + Vector3.up * VerifyHeight;
            var distanceToPoint = Vector3.Distance(transform.position, position);

            var checkDistance = VerifyDistance;

            if (point.Visibility < checkDistance)
                checkDistance = point.Visibility;

            if (distanceToPoint < checkDistance &&
                (distanceToPoint < 1 ||
                 AIUtil.IsInSight(_actor, position, checkDistance, FieldOfView)))
                return !point.RequiresReaching || distanceToPoint < 1.1f;

            return false;
        }

        private void glimpse(SearchBlock block)
        {
            for (int i = block.Count - 1; i >= 0; i--)
            {
                var p = block.Get(i);

                if (canBeInvestigated(p))
                {
                    var point = new InvestigatedPoint(p.Position);
                    markInvestigated(point);

                    for (int f = 0; f < _friends.Count; f++)
                        _friends[f].considerPoint(point);

                    block.Investigate(i);
                }
            }
        }

        private bool considerPoint(InvestigatedPoint point)
        {
            if (point.Time < _timeOfReset)
                return false;

            if (_hasPoint && areCloseEnough(point, _point))
            {
                _hasPoint = false;
                markInvestigated(point);
                return true;
            }

            if (considerPoint(_block, point))
                return true;

            for (int i = 0; i < _blocks.Count; i++)
                if (considerPoint(_blocks[i], point))
                {
                    if (_blocks[i].Empty)
                    {
                        _investigatedBlocks.Add(_blocks[i]);
                        _blocks.RemoveAt(i);
                    }

                    return true;
                }

            return false;
        }

        private bool considerPoint(SearchBlock block, InvestigatedPoint point)
        {
            for (int i = 0; i < block.Count; i++)
                if (areCloseEnough(point, block.Get(i)))
                {
                    block.Investigate(i);
                    markInvestigated(point);
                    return true;
                }

            return false;
        }

        private void markInvestigated(InvestigatedPoint point)
        {
            _investigated.Add(point);
            Message("OnPointInvestigated", point.Position);
        }

        private bool areCloseEnough(InvestigatedPoint a, SearchPoint b)
        {
            if (Vector3.Distance(a.Position, b.Position) < 0.5f)
                return true;

            return false;
        }

        private bool shouldRunTo(Vector3 position)
        {
            var distance = Vector3.Distance(transform.position, position);

            if (distance > WalkDistance || (distance > VerifyDistance && !AIUtil.IsInSight(_actor, position, VerifyDistance, 360)))
                return true;
            else
                return false;
        }

        private void setPoint(int index)
        {
            _pointIndex = index;
            _point = _points.Points[index];
            _searchPosition = _point.Position;
            _hasPoint = true;

            _hasPreviousPoint = true;
            _previousPointIndex = index;

            _hasSearchDirection = true;
            _searchDirection = (_point.Position - transform.position).normalized;

            _hasApproached = !shouldApproach(_point);

            if (shouldRunTo(_point.Position))
                run();
            else
                walk();
        }

        private bool shouldApproach(SearchPoint point)
        {
            return Vector3.Dot(point.Normal, point.Position - transform.position) > 0;
        }

        private void walk()
        {
            _wasRunning = false;
            Message("ToSlowlyAimAt", _point.Position);
            Message("ToTurnAt", _point.Position);

            if (!_hasApproached)
                Message("ToWalkTo", _point.ApproachPosition);
            else
                Message("ToWalkTo", _point.Position);
        }

        private void run()
        {
            _wasRunning = true;
            Message("ToFaceWalkDirection");

            if (!_hasApproached)
                Message("ToRunTo", _point.ApproachPosition);
            else
                Message("ToRunTo", _point.Position);
        }

        private void startSearch()
        {
            _isSearching = true;
            _hasPoint = false;

            _blocks.Clear();

            ToClearSearchHistory();

            for (int i = 0; i < _investigatedBlocks.Count; i++)
                _blockCache.Give(_investigatedBlocks[i]);

            _investigatedBlocks.Clear();

            _hasPreviousPoint = false;
            _points.Clear();

            GlobalSearchCache.GeneratedPoints.WriteTo(_points);

            for (int i = 0; i < GlobalSearchCache.GeneratedBlocks.Count; i++)
            {
                var block = _blockCache.Take();
                GlobalSearchCache.GeneratedBlocks[i].WriteTo(ref block);

                _blocks.Add(block);
            }
        }

        #endregion
    }
}
