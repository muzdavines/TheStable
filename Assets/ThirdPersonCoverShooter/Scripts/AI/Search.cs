using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Stores covers and search blocks inside the level.
    /// </summary>
    public struct GlobalSearchCache
    {
        public static List<SearchBlock> GeneratedBlocks = new List<SearchBlock>();
        public static SearchPointData GeneratedPoints = new SearchPointData();

        private static List<SearchBlock> _blocks = new List<SearchBlock>();
        private static SearchPointData _points = new SearchPointData();
        private static CoverCache _coverCache = new CoverCache();
        private static SearchZoneCache _zoneCache = new SearchZoneCache();
        private static SearchBlockCache _blockCache = new SearchBlockCache(_points);
        private static HashSet<Cover> _usedCovers = new HashSet<Cover>();

        private static bool _isGenerating;
        private static int _currentMergedGeneratedBlockId;
        private static float _currentTime = -1000;
        private static float _timeAtGenerationStart = -1000;
        private static bool _hasJustRestarted;

        /// <summary>
        /// Rebuilds the search point database. Ignores other calls to rebuild in this frame.
        /// </summary>
        public static void Restart()
        {
            if (_hasJustRestarted)
                return;

            _hasJustRestarted = true;
            Rebuild();
        }

        /// <summary>
        /// Checks if the database has to be rebuilt.
        /// </summary>
        public static void Update()
        {
            if (Time.timeSinceLevelLoad - _currentTime < 1f / 60f)
                return;

            _currentTime = Time.timeSinceLevelLoad;
            _hasJustRestarted = false;

            if (_isGenerating)
                mergeBlocks();
            else if (_currentTime > _timeAtGenerationStart + 30 * 10)
                Rebuild();
        }

        /// <summary>
        /// Rebuilds the search point database.
        /// </summary>
        public static void Rebuild()
        {
            const float searchRadius = 1000;

            _isGenerating = true;
            _currentMergedGeneratedBlockId = 0;
            _timeAtGenerationStart = Time.timeSinceLevelLoad;

            for (int i = 0; i < _blocks.Count; i++)
                _blockCache.Give(_blocks[i]);

            _blocks.Clear();
            _usedCovers.Clear();
            _points.Clear();

            _coverCache.Items.Clear();
            _coverCache.Reset(Vector3.zero, searchRadius, false);

            for (int i = 0; i < _coverCache.Items.Count; i++)
            {
                var item = _coverCache.Items[i];

                if (_usedCovers.Contains(item.Cover))
                    continue;

                var cover = item.Cover;
                var starting = item.Cover;

                while (cover.LeftAdjacent != null && !_usedCovers.Contains(cover.LeftAdjacent))
                {
                    if (cover.LeftAdjacent == starting)
                        break;

                    cover = cover.LeftAdjacent;
                }

                var index = -1;

                while (cover != null)
                {
                    _usedCovers.Add(cover);

                    var left = cover.LeftCorner(cover.Bottom) - cover.Forward * 0.25f;
                    var right = cover.RightCorner(cover.Bottom) - cover.Forward * 0.25f;
                    var vector = right - left;
                    var length = vector.magnitude;

                    var leftApproach = left;
                    var rightApproach = right;

                    {
                        NavMeshHit hit;
                        var position = left + cover.Left * AISearch.CoverOffset;

                        if (NavMesh.Raycast(left, position, out hit, 1))
                            leftApproach = left;
                        else
                            leftApproach = position;
                    }

                    {
                        NavMeshHit hit;
                        var position = right + cover.Right * AISearch.CoverOffset;

                        if (NavMesh.Raycast(right, position, out hit, 1))
                            rightApproach = right;
                        else
                            rightApproach = position;
                    }

                    if (cover.LeftAdjacent != null && cover.RightAdjacent != null)
                    {
                        leftApproach = left;
                        rightApproach = right;
                    }
                    else if (cover.LeftAdjacent != null)
                        leftApproach = rightApproach;
                    else if (cover.RightAdjacent != null)
                        rightApproach = leftApproach;

                    possiblyAddRightPoint(ref index, new SearchPoint(left, leftApproach, -cover.Forward, false));

                    if (length > AISearch.BlockThreshold * 2)
                    {
                        possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.2f, leftApproach, -cover.Forward, false));
                        possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.4f, leftApproach, -cover.Forward, false));
                        possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.6f, rightApproach, -cover.Forward, false));
                        possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.8f, rightApproach, -cover.Forward, false));
                    }
                    else if (length > AISearch.BlockThreshold)
                    {
                        possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.33f, leftApproach, -cover.Forward, false));
                        possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.66f, rightApproach, -cover.Forward, false));
                    }

                    possiblyAddRightPoint(ref index, new SearchPoint(right, rightApproach, -cover.Forward, false));

                    if (cover.RightAdjacent != null && !_usedCovers.Contains(cover.RightAdjacent))
                        cover = cover.RightAdjacent;
                    else
                        cover = null;
                }
            }

            _zoneCache.Reset(Vector3.zero, searchRadius);

            for (int i = 0; i < _zoneCache.Items.Count; i++)
            {
                var block = _zoneCache.Items[i];

                foreach (var position in block.Points(AISearch.BlockThreshold))
                    addPoint(new SearchPoint(position, false));
            }
        }

        private static void possiblyAddRightPoint(ref int index, SearchPoint point)
        {
            NavMeshHit hit;

            if (!NavMesh.SamplePosition(point.Position, out hit, 0.2f, 1))
                return;
            else
                point.Position = hit.position;

            var new_ = addPoint(point);

            if (index >= 0)
                _points.LinkRight(index, new_);

            index = new_;
        }

        private static int addPoint(SearchPoint point)
        {
            point.CalcVisibility(AISearch.VerifyDistance, false);
            var index = _points.Add(point);

            for (int i = 0; i < _blocks.Count; i++)
                if (_blocks[i].IsClose(point, AISearch.BlockThreshold, AISearch.BlockCenterThreshold))
                {
                    _blocks[i].Add(index);
                    return index;
                }

            var new_ = _blockCache.Take();
            new_.Add(index);
            _blocks.Add(new_);

            return index;
        }

        private static void mergeBlocks()
        {
            int processed = 0;

            for (int a = _currentMergedGeneratedBlockId; a < _blocks.Count - 1; a++)
            {
                RESTART:

                if (processed > 0)
                {
                    _currentMergedGeneratedBlockId = a;
                    break;
                }

                processed++;

                for (int b = _blocks.Count - 1; b > a; b--)
                {
                    for (int p = 0; p < _blocks[a].Indices.Count; p++)
                    {
                        var ap = _blocks[a].Indices[p];

                        if (_blocks[b].IsClose(_points.Points[ap], AISearch.BlockThreshold, AISearch.BlockCenterThreshold))
                            goto SUCCESS;
                    }

                    continue;

                    SUCCESS:

                    for (int p = 0; p < _blocks[b].Indices.Count; p++)
                        _blocks[a].Add(_blocks[b].Indices[p]);

                    _blockCache.Give(_blocks[b]);
                    _blocks.RemoveAt(b);

                    goto RESTART;
                }
            }

            for (int i = 0; i < _blocks.Count; i++)
            {
                var block = _blocks[i];
                block.Index = i;
                _blocks[i] = block;
            }

            GeneratedPoints.Clear();
            _points.WriteTo(GeneratedPoints);

            for (int i = 0; i < _blocks.Count; i++)
            {
                var block = _blockCache.Take();
                _blocks[i].WriteTo(ref block);

                GeneratedBlocks.Add(block);
            }

            _isGenerating = false;
        }
    }

    /// <summary>
    /// Information about a searchable position.
    /// </summary>
    public struct SearchPoint
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 ApproachPosition;
        public bool HasNormal;
        public float Visibility;
        public int Left;
        public int Right;
        public bool RequiresReaching;

        public SearchPoint(Vector3 position, Vector3 approachPosition, Vector3 normal, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = approachPosition;
            Normal = normal;
            HasNormal = true;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public SearchPoint(Vector3 position, Vector3 normal, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = position;
            Normal = normal;
            HasNormal = true;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public SearchPoint(Vector3 position, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = position;
            Normal = Vector3.zero;
            HasNormal = false;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public void CalcVisibility(float maxDistance, bool isAlerted)
        {
            Visibility = Util.GetViewDistance(Position, maxDistance, isAlerted);
        }
    }

    public class SearchPointData
    {
        public List<SearchPoint> Points;

        public SearchPointData()
        {
            Points = new List<SearchPoint>();
        }

        public void WriteTo(SearchPointData other)
        {
            other.Points.Clear();

            for (int i = 0; i < Points.Count; i++)
                other.Points.Add(Points[i]);
        }

        public void LinkLeft(int left, int middle)
        {
            var point = Points[left];
            point.Right = middle;
            Points[left] = point;

            point = Points[middle];
            point.Left = left;
            Points[middle] = point;
        }

        public void LinkRight(int middle, int right)
        {
            var point = Points[middle];
            point.Right = right;
            Points[middle] = point;

            point = Points[right];
            point.Left = middle;
            Points[right] = point;
        }


        public int Add(SearchPoint point)
        {
            Points.Add(point);
            return Points.Count - 1;
        }

        public void Clear()
        {
            Points.Clear();
        }
    }

    public struct SearchBlock
    {
        public bool Empty
        {
            get { return Indices.Count == 0; }
        }

        public int Count
        {
            get { return Indices.Count; }
        }

        public SearchPointData Data;
        public List<int> Indices;
        public List<int> InvestigatedIndices;
        public Vector3 Center;
        public Vector3 Sum;
        public int Index;

        public SearchBlock(SearchPointData data)
        {
            Data = data;
            Indices = new List<int>();
            InvestigatedIndices = new List<int>();
            Center = Vector3.zero;
            Sum = Vector3.zero;
            Index = 0;
        }

        public void WriteTo(ref SearchBlock other)
        {
            other.Index = Index;
            other.Sum = Sum;
            other.Center = Center;

            other.Indices.Clear();
            other.InvestigatedIndices.Clear();

            for (int i = 0; i < Indices.Count; i++)
                other.Indices.Add(Indices[i]);

            for (int i = 0; i < InvestigatedIndices.Count; i++)
                other.InvestigatedIndices.Add(InvestigatedIndices[i]);
        }

        public void Investigate(int index)
        {
            InvestigatedIndices.Add(Indices[index]);
            Indices.RemoveAt(index);
        }

        public SearchPoint Get(int index)
        {
            return Data.Points[Indices[index]];
        }

        public void Add(int index)
        {
            Indices.Add(index);

            Sum += Data.Points[index].Position;
            Center = Sum / Indices.Count;
        }

        public bool IsClose(SearchPoint point, float threshold, float middleThreshold)
        {
            if (Vector3.Distance(Center, point.Position) < threshold)
                return true;

            foreach (var i in Indices)
                if (Vector3.Distance(Data.Points[i].Position, point.Position) < threshold)
                    return true;

            return false;
        }

        public void Clear()
        {
            Indices.Clear();
            InvestigatedIndices.Clear();
            Center = Vector3.zero;
            Sum = Vector3.zero;
        }
    }

    public class SearchBlockCache
    {
        private List<SearchBlock> _cache = new List<SearchBlock>();
        private SearchPointData _points;

        public SearchBlockCache(SearchPointData points)
        {
            _points = points;
        }

        public void Give(SearchBlock block)
        {
            _cache.Add(block);
        }

        public SearchBlock Take()
        {
            if (_cache.Count == 0)
                return new SearchBlock(_points);
            else
            {
                var block = _cache[_cache.Count - 1];
                _cache.RemoveAt(_cache.Count - 1);

                block.Clear();

                return block;
            }
        }
    }

    /// <summary>
    /// Information about an already investigated position.
    /// </summary>
    public struct InvestigatedPoint
    {
        public Vector3 Position;
        public float Time;

        public InvestigatedPoint(Vector3 position)
        {
            Position = position;
            Time = UnityEngine.Time.timeSinceLevelLoad;
        }
    }
}
